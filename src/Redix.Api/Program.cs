using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Redix.Api;
using Redix.Api.Auth;
using Redix.Api.Translations;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables("REDIX_");

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddOptions<JwtOptions>().BindConfiguration("Jwt").Validate(jwt => jwt.SigningKey.Length > 0, "Set the REDIX_Jwt__SigningKey environment variable.").ValidateOnStart();
builder.Services.AddSingleton<AccessTokens>();
builder.Services.AddDbContext<RedixDbContext>((services, options) => options
    .UseSqlServer(services.GetRequiredService<IConfiguration>().GetConnectionString("Redix"))
    .UseSeeding((context, _) => OwnerSeed.Seed(context, services))
    .UseAsyncSeeding((context, _, cancellation) => OwnerSeed.SeedAsync(context, services, cancellation)));
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
app.MapAuth();

app.Run();
