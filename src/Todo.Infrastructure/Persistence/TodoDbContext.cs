using Microsoft.EntityFrameworkCore;
using Todo.Application.Abstractions.Persistence;
using Todo.Domain.Todos;
using Todo.Infrastructure.Persistence.Converters;

namespace Todo.Infrastructure.Persistence;

public sealed class TodoDbContext(DbContextOptions<TodoDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<TodoItem> Todos => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TodoDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Aplicado por convenção, e não propriedade a propriedade: qualquer data futura
        // do modelo nasce com o mesmo tratamento de fuso.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();

        base.ConfigureConventions(configurationBuilder);
    }
}
