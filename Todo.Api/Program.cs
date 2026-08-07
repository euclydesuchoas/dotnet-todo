using Todo.Api;
using Todo.Api.Endpoints;
using Todo.Application;
using Todo.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApi()
    .AddApplication()
    .AddInfrastructure();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Todo API");
        options.DocumentTitle = "Todo API Documentation";
    });
}

app.UseHttpsRedirection();

app.MapEndpoints();

app.Run();
