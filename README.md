# Vendas API

API .NET 8 para registro de vendas com SQL Server em Docker.

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
