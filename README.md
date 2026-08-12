# dotnet-todo

Uma API de gerenciamento de tarefas (Todo) construída com **.NET 10** e **ASP.NET Core Minimal APIs**, organizada em camadas seguindo princípios de **Clean Architecture**.

## Visão Geral

O projeto expõe endpoints REST para criação e gerenciamento de tarefas, com validação de regras de negócio, tratamento padronizado de erros via padrão `Result`, e documentação automática via OpenAPI/Swagger.

## Arquitetura

A solução é dividida nos seguintes projetos:

| Projeto | Responsabilidade |
|---|---|
| `Todo.Domain` | Entidades e regras de domínio. |
| `Todo.Application` | Casos de uso, validações (FluentValidation), abstrações de mensageria (`IRequest`, `IServiceHandler`) e de persistência (`ITodoRepository`, `IUnitOfWork`), e o padrão `Result` para tratamento de sucesso/falha. |
| `Todo.Infrastructure` | Implementações de infraestrutura: contexto e mapeamentos do EF Core, repositórios, migrations e a escolha do banco. |
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

### Persistência e Múltiplos Bancos

A aplicação roda sobre **PostgreSQL**, **SQL Server** ou **SQLite**, escolhidos por configuração:

```jsonc
// appsettings.Development.json
"Database": {
  "Provider": "Sqlite",
  "ConnectionString": "Data Source=todo.db",
  "ApplyMigrationsOnStartup": true
}
```

São duas ferramentas com responsabilidades separadas: o **EF Core** lê e grava dados, o **FluentMigrator** aplica o schema. A escolha do provider acontece em um único arquivo, `DatabaseProviderExtensions`, uma vez para cada uma delas.

O enum é aberto por `DatabaseProviderEnum.Match`, que recebe um delegate obrigatório por banco. Acrescentar um provider obriga a acrescentar um parâmetro ali, e isso quebra a compilação de toda chamada existente — nenhum ponto de decisão por banco fica para trás. É o motivo de a migration também consultar o enum em vez do `IfDatabase` do FluentMigrator: o `IfDatabase` compara strings que pertencem ao pacote, e um identificador que deixa de casar não gera erro — o ramo é descartado, a migration é aplicada sem produzir SQL e a versão é gravada como se tivesse funcionado.

O FluentMigrator é usado no lugar das migrations do EF Core justamente por causa do multi-banco: as do EF são geradas por provider e exigiriam um assembly de migrations para cada um, com o schema mantido em triplicata. Aqui a migration é uma só, escrita em uma API neutra que cada dialeto traduz.

Ela se ramifica por banco apenas onde os tipos nativos realmente divergem — o identificador e a data:

| Coluna | PostgreSQL | SQL Server | SQLite |
|---|---|---|---|
| `id` | `uuid` | `uniqueidentifier` | `TEXT` |
| `due_date` | `timestamp with time zone` | `datetime2(7)` | `TEXT` |

A ramificação está na criação da tabela, e não em um `Alter` posterior, porque o SQLite não suporta alterar o tipo de uma coluna existente.

#### Nomenclatura das migrations

Cada migration é um arquivo com versão em timestamp e nome descritivo — `202608100001_Create_Table_Todos` —, uma tabela por arquivo. A versão é o que o FluentMigrator ordena e registra; o nome é o que se lê no diretório.

Constraints e índices não são nomeados à mão. `MigrationConventionSet` substitui as convenções do FluentMigrator e gera os nomes a partir da tabela e das colunas:

| Objeto | Formato | Exemplo |
|---|---|---|
| Chave primária | `pk_{tabela}` | `pk_todos` |
| Chave estrangeira | `fk_{tabela}_{colunas}_{tabela_referida}_{colunas}` | `fk_todos_user_id_users_id` |
| Índice | `ix_{tabela}_{colunas}` | `ix_todos_due_date` |
| Constraint única | `uc_{tabela}_{colunas}` | `uc_todos_title` |

Sem isso, uma constraint sem nome explícito sai sem nome no SQL e cada banco inventa o seu — `todos_pkey` no Postgres, `PK__todos__3213E83F...` com sufixo aleatório no SQL Server. Um `DROP CONSTRAINT` em migration futura viraria consulta ao catálogo, com nome diferente em cada ambiente.

As convenções são aplicadas em tempo de execução e valem para todas as migrations, inclusive as já escritas — então são congeladas na prática: alterá-las muda o nome que uma migration antiga gera em um banco novo, sem mudar o que ela já gerou nos bancos existentes. Para um nome fora do padrão, basta nomear a constraint na migration, que o nome explícito vence a convenção.

A tabela de controle do próprio FluentMigrator segue a mesma regra, via `MigrationVersionTable`: `version_info` com colunas `version`, `applied_on` e `description`, no lugar do `VersionInfo` padrão.

#### Comportamento uniforme entre bancos

Duas decisões evitam divergência de comportamento entre os bancos:

- **Nomes físicos em minúsculas** (`todos`, `due_date`), em `TodoItemTable`, compartilhados pelo mapeamento e pela migration. O Postgres rebaixa identificadores não citados para minúsculas, então um nome com maiúsculas só seria alcançável entre aspas.
- **Datas sempre em UTC** — ver a seção abaixo.

A ausência da seção `Database`, um provider inválido ou uma connection string vazia derrubam a aplicação no boot: `DatabaseProviderEnum` não tem membro de valor zero, então não existe padrão para o qual cair por acidente.

`ApplyMigrationsOnStartup` é `false` por padrão — em produção o schema costuma ser aplicado por um passo próprio do deploy, com credenciais próprias, e não pela aplicação subindo.

### Datas em UTC

O sistema guarda instantes, não horários locais: nenhum fuso é persistido. Um `DateTime` em .NET carrega um `DateTimeKind` que a comparação entre dois valores simplesmente ignora, e cada banco trata o fuso de um jeito — a combinação produz erros que só aparecem quando o servidor não roda em UTC, o que os esconde em desenvolvimento.

A regra de normalização é uma só, `UtcDateTime.Normalize`, no domínio: `Utc` passa, `Local` é convertido, `Unspecified` é assumido como UTC. Assumir o fuso do servidor no último caso faria a mesma requisição gravar instantes diferentes conforme a máquina que hospeda a aplicação.

Ela é aplicada em quatro pontos, cada um cobrindo um caminho que os outros não alcançam:

| Ponto | Cobre |
|---|---|
| `AsUtcDateTime` na migration | O tipo da coluna. No Postgres precisa ser `timestamp with time zone`: os tipos portáteis do FluentMigrator geram `timestamp` sem fuso, e o valor seria deslocado conforme o `TimeZone` da sessão, sem erro nenhum. |
| `UtcDateTimeJsonConverter` na API | Corpo JSON. Sem ele, `"…-03:00"` chega como `Local` e `"…"` sem offset como `Unspecified`. |
| `UtcDateTime` em query string e rota | Parâmetros vinculados fora do JSON, que não passam pelo converter acima. |
| `UtcDateTimeConverter` no EF Core | Gravação e leitura, por convenção sobre todo `DateTime` do modelo. |

`TodoItem.Create` também normaliza, tornando "`DueDate` é UTC" invariante do tipo para quem constrói a tarefa sem passar por HTTP — teste, seed ou rotina.

No Postgres a coluna é `timestamp with time zone`, mas o fuso **não** é gravado: o tipo são 8 bytes de microssegundos desde a epoch, e o offset que aparece em um `SELECT` é o cliente renderizando no `TimeZone` da sessão. `SET TIME ZONE 'UTC'` mostra o mesmo valor com `+00`.

### Tratamento de Erros

Os casos de uso retornam `Result`/`Result<TData>` em vez de lançar exceções para fluxos de erro esperados (validação, não encontrado, conflito, etc.), permitindo que a camada de API decida como traduzir cada tipo de erro em uma resposta HTTP apropriada.

## Tecnologias

- .NET 10
- ASP.NET Core Minimal APIs
- Entity Framework Core (PostgreSQL, SQL Server e SQLite)
- FluentMigrator (migrations únicas, aplicadas em qualquer um dos três bancos)
- FluentValidation
- Scrutor (registro automático dos handlers da camada de aplicação e dos repositórios via convenção)
- Swashbuckle / OpenAPI
- xUnit

## Como Executar

```powershell
dotnet restore
dotnet run --project Todo.Api
```

Após iniciar, a documentação estará disponível em `/documentation` (ambiente de desenvolvimento).

Em desenvolvimento o banco padrão é um SQLite em arquivo (`todo.db`), criado e migrado no primeiro boot — nenhum servidor precisa estar instalado. Para rodar sobre PostgreSQL ou SQL Server, basta trocar `Database:Provider` e a connection string.

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/v1/todos` | Cria uma tarefa. |
| `GET` | `/api/v1/todos` | Lista tarefas, com filtros opcionais. |
| `GET` | `/api/v1/todos/{id}` | Busca uma tarefa pelo identificador. |

A listagem aceita `title` (contém), `isCompleted`, `dueFrom` e `dueTo`. Filtro ausente não restringe, e o intervalo de vencimento é fechado dos dois lados. Como as datas são comparadas por instante, as três formas abaixo selecionam exatamente as mesmas tarefas:

```
GET /api/v1/todos?dueFrom=2027-03-10T12:00:00Z
GET /api/v1/todos?dueFrom=2027-03-10T09:00:00-03:00
GET /api/v1/todos?dueFrom=2027-03-10T21:00:00%2B09:00
```

## Como Testar

```powershell
dotnet test
```

## Status

Projeto em desenvolvimento ativo. Funcionalidades e endpoints estão sendo adicionados incrementalmente.
