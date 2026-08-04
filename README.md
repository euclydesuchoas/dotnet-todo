# dotnet-todo

Uma API de gerenciamento de tarefas (Todo) construída com **.NET 10** e **ASP.NET Core Minimal APIs**, organizada em camadas seguindo princípios de **Clean Architecture**.

## Visão Geral

O projeto expõe endpoints REST para criação e gerenciamento de tarefas, com validação de regras de negócio, tratamento padronizado de erros via padrão `Result`, e documentação automática via OpenAPI/Swagger.

## Arquitetura

A solução é dividida nos seguintes projetos:

| Projeto | Responsabilidade |
|---|---|
| `Todo.Domain` | Entidades e regras de domínio. |
| `Todo.Application` | Casos de uso, validações (FluentValidation), abstrações de mensageria (`IRequest`, `IServiceHandler`) e o padrão `Result` para tratamento de sucesso/falha. |
| `Todo.Infrastructure` | Implementações de infraestrutura (persistência, serviços externos). |
| `Todo.Api` | Camada de apresentação com endpoints Minimal API, organizados por grupo de rotas (`IEndpointGroup`) com descoberta automática via injeção de dependência. |
| `Todo.Tests.Unit` | Testes unitários. |
| `Todo.Tests.Integration` | Testes de integração. |
| `Todo.Tests.Architecture` | Testes de arquitetura (garantem convenções e dependências entre camadas). |

### Padrão de Endpoints

Os endpoints são descobertos e registrados automaticamente via injeção de dependência (usando [Scrutor](https://github.com/khellang/Scrutor)), sem necessidade de mapear rotas manualmente em `Program.cs`. Cada grupo de rotas implementa `IEndpointGroup<TSelf>` (padrão CRTP) para compartilhar prefixo de rota e metadados comuns (tags, autorização, etc.), enquanto cada endpoint individual implementa `IEndpoint<TEndpointGroup>`.

### Tratamento de Erros

Os casos de uso retornam `Result`/`Result<TData>` em vez de lançar exceções para fluxos de erro esperados (validação, não encontrado, conflito, etc.), permitindo que a camada de API decida como traduzir cada tipo de erro em uma resposta HTTP apropriada.

## Tecnologias

- .NET 10
- ASP.NET Core Minimal APIs
- FluentValidation
- Scrutor (registro automático de serviços via convenção)
- Swashbuckle / OpenAPI
- xUnit

## Como Executar

```powershell
dotnet restore
dotnet run --project Todo.Api
```

Após iniciar, a documentação Swagger estará disponível em `/swagger` (ambiente de desenvolvimento).

## Como Testar

```powershell
dotnet test
```

## Status

Projeto em desenvolvimento ativo. Funcionalidades e endpoints estão sendo adicionados incrementalmente.
