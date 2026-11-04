using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Redix.Api;
using Redix.Api.Auth;
using Redix.Api.Projects;
using Redix.Api.Translations;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables("REDIX_");

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddOptions<JwtOptions>().BindConfiguration("Jwt").Validate(jwt => jwt.SigningKey.Length > 0, "Set the REDIX_Jwt__SigningKey environment variable.").ValidateOnStart();
builder.Services.AddSingleton<AccessTokens>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme).Configure<IOptions<JwtOptions>, TimeProvider>((bearer, jwt, time) =>
{
    bearer.MapInboundClaims = false;
    bearer.TokenValidationParameters = AccessTokens.ValidationParameters(jwt.Value);
    bearer.TimeProvider = time;
    bearer.Events = new JwtBearerEvents { OnTokenValidated = SessionPolicies.CheckSessionIsOpen };
});
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(SessionPolicies.Working)
    .AddPolicy(SessionPolicies.MayChangePassword, policy => policy.RequireAuthenticatedUser())
    .AddPolicy(SessionPolicies.Owner, policy => policy.Combine(SessionPolicies.Working).RequireClaim(AccessTokens.OwnerClaim, "true"));
builder.Services.AddDbContext<RedixDbContext>((services, options) => options
    .UseSqlServer(services.GetRequiredService<IConfiguration>().GetConnectionString("Redix"))
    .UseSeeding((context, _) => OwnerSeed.Seed(context, services))
    .UseAsyncSeeding((context, _, cancellation) => OwnerSeed.SeedAsync(context, services, cancellation)));
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    options.SerializerOptions.RespectRequiredConstructorParameters = true;
    options.SerializerOptions.RespectNullableAnnotations = true;
});
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = false);
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks().AddDbContextCheck<RedixDbContext>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (!app.Environment.IsProduction())
{
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "REDIX API"));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapTranslations();
app.MapAuth();
app.MapProjects();

app.Run();
