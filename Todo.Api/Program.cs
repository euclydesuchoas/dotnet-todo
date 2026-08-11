using Todo.Api;
using Todo.Api.Common;
using Todo.Api.Endpoints;
using Todo.Application;
using Todo.Infrastructure;
using Todo.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApi(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Antes de qualquer requisição ser atendida: subir com o schema desatualizado só adiantaria
// a falha para o primeiro acesso.
app.Services.ApplyDatabaseMigrations();

// Configure the HTTP request pipeline.
app.UseApiDocumentation();

app.UseHttpsRedirection();

app.MapEndpoints();

app.Run();
