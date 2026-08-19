using FluentMigrator;
using Microsoft.Extensions.Options;
using Todo.Infrastructure.Common;
using Todo.Infrastructure.Common.Options;

namespace Todo.Infrastructure.Persistence.Migrations;

/// <summary>
/// Cria a tabela de tarefas em cada banco suportado.
/// </summary>
/// <remarks>
/// Nomes e tamanhos são literais desta migration, e não constantes do domínio ou do
/// mapeamento. Uma migration é registro histórico: descreve o schema como ele nasceu, e
/// precisa continuar gerando exatamente o mesmo resultado depois que o domínio evoluir.
/// Se um limite mudar, o que muda o banco é uma migration nova — nunca esta.
///
/// A chave primária não é nomeada aqui: o nome vem de
/// <see cref="Conventions.MigrationConventionSet"/>, que produz <c>pk_todo_items</c>.
///
/// A única coluna que se ramifica por banco é a data, porque nenhum tipo do FluentMigrator
/// serve aos três — ver <see cref="MigrationColumnExtensions.AsUtcDateTime{TNext}"/>. A
/// ramificação está na criação, e não em um <c>Alter</c> posterior, porque o SQLite não
/// altera tipo de coluna.
///
/// Quem decide o ramo é <see cref="DatabaseOptions.Provider"/>, injetado pelo container, e
/// não o <c>IfDatabase</c> do FluentMigrator. Os dois resolvem a mesma coisa, mas o
/// <c>IfDatabase</c> compara strings que pertencem ao pacote: <c>AddPostgres()</c> registra
/// hoje o <c>Postgres15_0Processor</c>, cujos ids são <c>PostgreSQL15_0</c> e
/// <c>PostgreSQL</c> — trocar essa escolha em uma atualização é mudança interna deles, e um
/// id que deixa de casar não dá erro: o ramo é descartado, a migration é aplicada sem gerar
/// SQL e a versão é gravada como se tivesse funcionado. O enum é do projeto e é conferido
/// pelo compilador.
/// </remarks>
[Migration(202608100001, "Create table todo_items")]
public sealed class _202608100001_Create_Table_Todo_Items(IOptions<DatabaseOptions> databaseOptions) : Migration
{
    public override void Up()
    {
        Create.Table("todo_items")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("title").AsString(100).NotNullable()
            .WithColumn("description").AsString(500).NotNullable()
            .WithColumn("due_date").AsUtcDateTime(databaseOptions.Value.Provider).NotNullable()
            .WithColumn("is_completed").AsBoolean().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("todo_items");
    }
}
