using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Todo.Domain.TodoItems;
using Todo.Infrastructure.Persistence.Schema;

namespace Todo.Infrastructure.Persistence.Configurations;

internal sealed class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.ToTable(TodoItemTable.Name);

        builder.HasKey(todo => todo.Id)
            .HasName(TodoItemTable.PrimaryKeyName);

        builder.Property(todo => todo.Id)
            .HasColumnName(TodoItemTable.Columns.Id)
            // O identificador vem do domínio (UUID v7), então nenhum provider deve
            // tentar gerar um valor no lugar dele.
            .ValueGeneratedNever();

        builder.Property(todo => todo.Title)
            .HasColumnName(TodoItemTable.Columns.Title)
            .HasMaxLength(TodoItem.TitleMaxLength)
            .IsRequired();

        builder.Property(todo => todo.Description)
            .HasColumnName(TodoItemTable.Columns.Description)
            .HasMaxLength(TodoItem.DescriptionMaxLength)
            .IsRequired();

        builder.Property(todo => todo.DueDate)
            .HasColumnName(TodoItemTable.Columns.DueDate);

        builder.Property(todo => todo.IsCompleted)
            .HasColumnName(TodoItemTable.Columns.IsCompleted);

        // Nenhum tipo de coluna é fixado aqui: o tipo nativo de cada provider é escolhido
        // pela migration, e amarrá-lo no mapeamento tornaria o modelo válido em um banco só.
    }
}
