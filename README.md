# Vendas e Relatorios

API .NET 8 para registro e consulta de vendas, com processamento assíncrono de relatórios por Amazon SQS.

O projeto separa o recebimento das solicitações, as regras de negócio, a persistência e o processamento em background. A API responde rapidamente ao usuário, enquanto o Worker processa relatórios sem bloquear a requisição HTTP.

## Funcionalidades

- Registro de vendas com itens e cálculo automático dos totais.
- Consulta de vendas com filtro opcional por período.
- Solicitação assíncrona de relatório.
- Consulta do status de uma solicitação.
- Worker consumidor de mensagens SQS.
- Relatório JSON com resumo e detalhamento por produto.
- Retry automático e Dead Letter Queue para falhas definitivas.
- Swagger para exploração dos endpoints.

## Arquitetura

```text
Cliente
  |
  v
Vendas.Api -----> SQL Server
  |
  +-------------> SQS: sales-reports
                         |
                         v
                    Vendas.Worker -----> SQL Server
                         |
                         v
                    reports/*.json
```

### Camadas

#### `Vendas.Domain`

Contém o núcleo do negócio, sem dependências de ASP.NET Core, EF Core ou AWS. Mantém as entidades `Venda`, `ItemVenda` e `ReportRequest`, suas validações, cálculos e estados (`Pending`, `Processing`, `Completed` e `Failed`).

Essa independência mantém as regras testáveis e evita que detalhes de infraestrutura contaminem o domínio.

#### `Vendas.Application`

Implementa os casos de uso e define as abstrações necessárias para persistência, mensageria e geração de relatórios. A camada não conhece EF Core nem o SDK da AWS, o que permite trocar essas tecnologias sem alterar as regras de aplicação.

#### `Vendas.Infrastructure`

Implementa as integrações externas: Entity Framework Core, SQL Server, repositórios, publisher SQS, redrive policy, Dead Letter Queue e escrita do relatório JSON.

#### `Vendas.Api`

É a entrada HTTP da aplicação. Contém controllers, contratos HTTP, Swagger e composição de dependências. Não calcula totais, acessa o `DbContext` ou publica diretamente no SQS.

#### `Vendas.Worker`

É um processo independente que faz long polling no SQS, consulta o banco, gera o relatório, atualiza o status e remove a mensagem somente após sucesso.

### Por que essa separação?

O domínio permanece independente, os casos de uso podem ser executados por HTTP ou background worker, e as integrações externas ficam substituíveis. Isso reduz o acoplamento e facilita testes, manutenção e evolução do sistema.

## Pré-requisitos

- .NET SDK 8.
- Docker Desktop com containers Linux habilitados.
- Docker Compose.

O ambiente local usa SQL Server 2022 e LocalStack com o serviço SQS em `http://localhost:4566`.

## Executando o projeto

Na raiz do projeto, inicie as dependências:

```powershell
docker compose up -d
```

Inicie a API:

```powershell
dotnet run --project .\Vendas.Api --launch-profile http
```

A API ficará disponível em `http://localhost:5189`. O Swagger ficará em `http://localhost:5189/swagger`.

Em outro terminal, inicie o Worker:

```powershell
dotnet run --project .\Vendas.Worker
```

Na primeira inicialização, a aplicação cria o banco e as tabelas usando `EnsureCreated`.

Para verificar ou parar o ambiente:

```powershell
docker compose ps
docker compose down
```

## Endpoints

### Registrar venda

```http
POST /api/vendas
Content-Type: application/json
```

```json
{
  "dataVenda": "2026-09-21T10:30:00Z",
  "cliente": "Maria Silva",
  "itens": [
    { "produto": "Teclado", "quantidade": 2, "valorUnitario": 50 },
    { "produto": "Mouse", "quantidade": 1, "valorUnitario": 80 }
  ]
}
```

A resposta é `201 Created`. O valor do primeiro item será `100` e o valor total da venda será `180`.

Quantidade menor ou igual a zero, valor unitário negativo, cliente ausente, produto ausente ou venda sem itens retornam `400 Bad Request`.

### Consultar vendas

```http
GET /api/vendas
GET /api/vendas/{id}
```

Filtros opcionais e inclusivos:

```http
GET /api/vendas?dataInicio=2026-09-01T00:00:00Z&dataFim=2026-09-30T23:59:59Z
```

Sem vendas no período, a API retorna `200 OK` com uma lista vazia.

### Solicitar relatório

```http
POST /api/relatorios
Content-Type: application/json
```

```json
{
  "startDate": "2026-09-01T00:00:00Z",
  "endDate": "2026-09-21T23:59:59Z"
}
```

A API persiste a solicitação com status `Pending`, publica uma mensagem no SQS e retorna imediatamente `202 Accepted`:

```json
{
  "reportId": "8c5f7f54-6ec9-48a0-8a49-9aa6d7e5c111",
  "status": "Pending"
}
```

### Consultar status

```http
GET /reports/{id}
```

```json
{
  "id": "8c5f7f54-6ec9-48a0-8a49-9aa6d7e5c111",
  "startDate": "2026-09-01T00:00:00Z",
  "endDate": "2026-09-21T23:59:59Z",
  "status": "Completed",
  "createdAt": "2026-09-21T10:00:00Z",
  "processedAt": "2026-09-21T10:00:08Z",
  "errorMessage": null
}
```

Os status possíveis são `Pending`, `Processing`, `Completed` e `Failed`. Uma solicitação inexistente retorna `404 Not Found`.

## Como funciona o SQS

Quando `POST /api/relatorios` é chamado, a API cria a `ReportRequest`, persiste o status `Pending`, garante as filas e publica uma mensagem em `sales-reports`.

A mensagem contém somente os dados necessários para localizar a solicitação:

```json
{
  "reportId": "8c5f7f54-6ec9-48a0-8a49-9aa6d7e5c111",
  "startDate": "2026-09-01T00:00:00Z",
  "endDate": "2026-09-21T23:59:59Z"
}
```

Clientes, produtos e valores não são enviados na mensagem. O Worker consulta esses dados diretamente no SQL Server.

O Worker usa long polling, recebe até 10 mensagens por busca e aplica visibility timeout de 60 segundos. Para cada mensagem válida, ele:

1. Localiza a solicitação pelo `reportId`.
2. Altera o status para `Processing`.
3. Consulta no banco somente as vendas do período.
4. Calcula e grava o relatório.
5. Marca a solicitação como `Completed`.
6. Remove a mensagem usando o receipt handle.

## Retry e Dead Letter Queue

Em uma falha temporária, o Worker não chama `DeleteMessage`. Após o visibility timeout, o SQS disponibiliza a mensagem novamente.

O limite padrão é de 3 recebimentos e pode ser alterado em `Vendas.Worker/appsettings.json`:

```json
{
  "AWS": {
    "MaxReceiveAttempts": 3
  }
}
```

Após o limite, a fila `sales-reports` encaminha a mensagem para `sales-reports-dlq` por meio da redrive policy. Na última tentativa, a solicitação é marcada como `Failed`. Em caso de sucesso, ela é marcada como `Completed` e a mensagem é removida.

Mensagens inválidas ou com `reportId` inexistente não são tratadas como relatórios válidos. Falhas de processamento permanecem sujeitas ao retry.

## Relatório gerado

O Worker grava `reports/relatorio-{reportId}.json` com:

- cabeçalho e período;
- quantidade de vendas;
- quantidade de itens vendidos;
- faturamento total, calculado pela soma de `Venda.ValorTotal`;
- ticket médio, calculado como faturamento total dividido pela quantidade de vendas;
- detalhamento por produto com quantidade vendida e faturamento.

Quando não existem vendas, faturamento total e ticket médio são `0`, e o detalhamento fica vazio.

## Configuração e segurança

O SQL Server local usa a connection string configurada nos arquivos `appsettings.json` da API e do Worker. A senha existente é apenas para desenvolvimento local.

No ambiente local, o SQS usa:

```json
{
  "AWS": {
    "Region": "us-east-1",
    "SqsServiceUrl": "http://localhost:4566",
    "ReportQueueName": "sales-reports",
    "ReportDeadLetterQueueName": "sales-reports-dlq"
  }
}
```

Em produção, remova `SqsServiceUrl` e use IAM Role, secret manager ou a cadeia padrão de credenciais da AWS. Nunca inclua access keys no código ou no repositório.

## Testes e validação

```powershell
dotnet build
dotnet test
```

Os endpoints também podem ser explorados pelo Swagger em `http://localhost:5189/swagger`.
