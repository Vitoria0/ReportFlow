# Vendas API

API .NET 8 para registro de vendas com SQL Server em Docker.

## Arquitetura

- `Vendas.Domain`: entidades, regras de negocio e excecoes de dominio.
- `Vendas.Application`: casos de uso, contratos e abstracoes de persistencia.
- `Vendas.Infrastructure`: Entity Framework Core, SQL Server e repositorios.
- `Vendas.Api`: controllers, contratos HTTP, Swagger e composicao da aplicacao.
- `Vendas.Worker`: consumidor SQS e processamento assíncrono das solicitações.

As dependencias seguem o fluxo `Api -> Application -> Domain` e `Api -> Infrastructure -> Application/Domain`.

## Executar

1. Inicie o banco:

```powershell
docker compose up -d
```

2. Inicie a API:

```powershell
dotnet run --project .\Vendas.Api
```

Na primeira inicializacao a API cria o banco e as tabelas com `EnsureCreated`.

## Endpoint

`POST /api/vendas`

Exemplo de corpo:

```json
{
  "dataVenda": "2026-09-21T10:30:00Z",
  "cliente": "Maria Silva",
  "itens": [
    {
      "produto": "Teclado",
      "quantidade": 2,
      "valorUnitario": 50
    },
    {
      "produto": "Mouse",
      "quantidade": 1,
      "valorUnitario": 80
    }
  ]
}
```

A resposta `201 Created` contem `valorTotal` por item e da venda. Quantidade menor ou igual a zero, valor unitario negativo e vendas sem itens retornam `400 Bad Request`.

Para consultar uma venda criada: `GET /api/vendas/{id}`.

Para consultar todas as vendas: `GET /api/vendas`.

É possível filtrar por período usando `dataInicio` e `dataFim`, por exemplo:
`GET /api/vendas?dataInicio=2026-09-01T00:00:00Z&dataFim=2026-09-30T23:59:59Z`.
Os limites do período são inclusivos. Quando não houver vendas no período, a API retorna `200 OK` com uma lista vazia.

## Solicitação assíncrona de relatório

Inicie também o LocalStack para disponibilizar o SQS local:

```powershell
docker compose up -d
```

Solicite um relatório com `POST /api/relatorios`:

```json
{
  "startDate": "2026-09-01T00:00:00Z",
  "endDate": "2026-09-30T23:59:59Z"
}
```

A API persiste a solicitação com status `Pending`, publica uma mensagem na fila `sales-reports` e retorna imediatamente `202 Accepted`:

```json
{
  "reportId": "8c5...",
  "status": "Pending"
}
```

## Worker SQS

Em outro terminal, inicie o consumidor:

```powershell
dotnet run --project .\Vendas.Worker
```

O worker recebe mensagens com `reportId`, `startDate` e `endDate`, busca as vendas no período diretamente no SQL Server, gera `reports/relatorio-{reportId}.json`, altera a solicitação para `Processing` e depois `Completed`. A mensagem só é removida após a geração do arquivo. Falhas deixam a mensagem na fila para o retry padrão do SQS; mensagens inválidas ou com `reportId` inexistente são descartadas como não processáveis.

O JSON contém cabeçalho, quantidade de vendas, quantidade de itens vendidos, faturamento total, ticket médio e detalhamento agrupado por produto. Sem vendas, faturamento e ticket médio são `0`.
