using Todo.Api;
using Todo.Api.Common;
using Todo.Api.Endpoints;
using Todo.Application;
using Todo.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApi(builder.Configuration)
    .AddApplication()
    .AddInfrastructure();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseApiDocumentation();

app.UseHttpsRedirection();

app.MapEndpoints();

app.Run();
