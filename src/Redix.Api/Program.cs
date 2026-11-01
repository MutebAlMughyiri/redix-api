using Microsoft.EntityFrameworkCore;
using Redix.Api;
using Redix.Api.Translations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RedixDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Redix")));
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks().AddDbContextCheck<RedixDbContext>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "REDIX API"));
}

app.UseHttpsRedirection();
app.MapHealthChecks("/health");
app.MapTranslations();

app.Run();
