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
| `Todo.Api` | Camada de apresentação com endpoints Minimal API, organizados em uma árvore de grupos de rotas (`IEndpointGroup`) com descoberta automática no startup. |
| `Todo.Tests.Unit` | Testes unitários. |
| `Todo.Tests.Integration` | Testes de integração. |
| `Todo.Tests.Architecture` | Testes de arquitetura (garantem convenções e dependências entre camadas). |

### Padrão de Endpoints

Os endpoints são descobertos e mapeados automaticamente em uma única passada de reflexão no startup (`MapEndpoints`), sem necessidade de mapear rotas manualmente em `Program.cs`.

O modelo é uma árvore em que **cada nó declara seu pai pelo argumento de tipo**:

- `IEndpointGroup` — grupo raiz, pendurado direto no app (ex.: `ApiEndpointGroup`, com prefixo `/api`).
- `IEndpointGroup<TParent>` — grupo aninhado em outro grupo (ex.: `V1EndpointGroup : IEndpointGroup<ApiEndpointGroup>`).
- `IEndpoint<TGroup>` — endpoint pertencente a um grupo.

O `RouteGroupBuilder` devolvido por `MapGroup` é o ponto único onde se concentra tudo que é comum ao grupo — prefixo, tags, autorização, rate limiting, CORS, filtros — e o que é configurado em um grupo cascateia para seus filhos e endpoints.

Grupos e endpoints são instanciados diretamente pelo registrador (construtor público sem parâmetros) e não vão para o container de DI: dependências são recebidas nos parâmetros do delegate da rota, que já respeitam o escopo da requisição. Inconsistências (endpoint sem grupo, grupo inexistente, ciclo na hierarquia) falham no startup com mensagem explícita, em vez de resultarem em rota silenciosamente ausente.

### Versionamento

Cada versão da API tem um grupo raiz próprio, entre o grupo `/api` e os grupos de funcionalidade:

```
ApiEndpointGroup            /api
├── V1EndpointGroup         /v1      → documento OpenAPI "v1"
│   └── TodoEndpointGroup   /todos
└── V2EndpointGroup         /v2      → documento OpenAPI "v2"
    └── TodoEndpointGroup   /todos
```

O grupo da versão aplica `WithGroupName`, que associa todos os seus endpoints a um documento OpenAPI de mesmo nome. Como resultado, cada versão é servida separadamente (`/openapi/v1.json`, `/openapi/v2.json`) e aparece como uma definição distinta na interface de documentação, contendo apenas os endpoints daquela versão.

As versões existentes ficam em `ApiVersions.All`, que é a lista percorrida tanto no registro dos documentos (`AddOpenApi`) quanto na configuração da interface de documentação. Para criar uma v3, basta adicionar a constante à lista e criar o `V3EndpointGroup` correspondente.

Nomes de rota (`WithName`) precisam ser únicos em toda a aplicação, e não apenas dentro de uma versão — por isso `EndpointNames` é segregado por versão.

### Caminhos de Rota

Cada grupo declara seu próprio prefixo direto no `MapGroup`, junto de quem o usa. Segmentos não são centralizados em constantes compartilhadas: `/todos` aparecer no grupo da v1 e no da v2 é intencional, já que versões são contratos independentes e precisam poder divergir sem arrastar uma à outra. A exceção é o segmento da versão, que vem de `ApiVersions` porque precisa coincidir com o nome do documento OpenAPI.

Caminhos absolutos não são montados à mão. Quem precisa deles — como o header `Location` — os obtém da própria tabela de rotas, referenciando o endpoint de destino pelo nome:

```csharp
Results.CreatedAtRoute(EndpointNames.V2.GetTodoById, new { id = result.Data }, result)
```

A vantagem é que o caminho passa a ser derivado da definição da rota de destino: se ela mudar, o link acompanha sem que o endpoint de criação seja tocado. Note que `CreatedAtRoute` gera uma **URI absoluta** (com esquema e host); para um caminho relativo, use `LinkGenerator.GetPathByName`.

### Documentação da API

Qual interface de documentação é exposta — e se alguma é — vem da configuração, não do ambiente:

```jsonc
// appsettings.json (padrão)
"Documentation": { "Provider": "None" }

// appsettings.Development.json
"Documentation": { "Provider": "Swagger" }
```

Os valores aceitos são `None`, `Swagger` e `Scalar`. Todos servem no mesmo prefixo, `/documentation`: o provider é um detalhe interno, então trocá-lo não deve invalidar links nem bookmarks. O prefixo é a constante `DocumentationOptions.RoutePrefix`, lida pelos dois providers, para que não haja como divergirem.

`None` é o padrão do enum, de modo que a ausência da seção resulte em documentação desligada — inclusive o documento OpenAPI, já que Swagger e Scalar são apenas cascas que o consomem. As opções são validadas com `ValidateOnStart`, então um valor inválido derruba a aplicação no boot com mensagem explícita, em vez de silenciosamente desligar a documentação.

### Tratamento de Erros

Os casos de uso retornam `Result`/`Result<TData>` em vez de lançar exceções para fluxos de erro esperados (validação, não encontrado, conflito, etc.), permitindo que a camada de API decida como traduzir cada tipo de erro em uma resposta HTTP apropriada.

## Tecnologias

- .NET 10
- ASP.NET Core Minimal APIs
- FluentValidation
- Scrutor (registro automático dos handlers da camada de aplicação via convenção)
- Swashbuckle / OpenAPI
- xUnit

## Como Executar

```powershell
dotnet restore
dotnet run --project Todo.Api
```

Após iniciar, a documentação estará disponível em `/documentation` (ambiente de desenvolvimento).

## Como Testar

```powershell
dotnet test
```

## Status

Projeto em desenvolvimento ativo. Funcionalidades e endpoints estão sendo adicionados incrementalmente.
