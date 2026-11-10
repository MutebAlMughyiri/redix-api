using System.Text.Json;
using System.Threading.RateLimiting;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Redix.Api;
using Redix.Api.Accounts;
using Redix.Api.Attachments;
using Redix.Api.Config;
using Redix.Api.Auth;
using Redix.Api.Contracts;
using Redix.Api.Documents;
using Redix.Api.Entries;
using Redix.Api.Partners;
using Redix.Api.Reports;
using Redix.Api.Revenue;
using Redix.Api.Projects;
using Redix.Api.Settings;
using Redix.Api.Structure;
using Redix.Api.Notifications;
using Redix.Api.Suppliers;
using Redix.Api.Sync;
using Redix.Api.Translations;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables("REDIX_");
builder.Configuration.AddEnvironmentVariables($"REDIX_{builder.Environment.EnvironmentName}_");

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddOptions<JwtOptions>().BindConfiguration("Jwt").Validate(jwt => jwt.SigningKey.Length > 0, "Set the REDIX_Jwt__SigningKey environment variable.").ValidateOnStart();
builder.Services.AddSingleton<AccessTokens>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme).Configure<IOptions<JwtOptions>, TimeProvider>((bearer, jwt, time) =>
{
    bearer.MapInboundClaims = false;
    bearer.TokenValidationParameters = AccessTokens.ValidationParameters(jwt.Value, time);
    bearer.TimeProvider = time;
    bearer.Events = new JwtBearerEvents { OnTokenValidated = SessionPolicies.CheckSessionIsOpen };
});
var authorization = builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(SessionPolicies.Working)
    .AddPolicy(SessionPolicies.MayChangePassword, policy => policy.RequireAuthenticatedUser());
foreach (var permission in Enum.GetValues<Permission>())
{
    authorization.AddPolicy(permission.ToString(), policy => policy.Combine(SessionPolicies.Working).AddRequirements(new PermissionRequirement(permission)));
}
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddRateLimiter(limiter =>
{
    limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    limiter.AddPolicy(RegistrationEndpoints.RegistrationLimit, context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = context.RequestServices.GetRequiredService<IConfiguration>().GetValue("Registration:PermitsPerHour", 5), Window = TimeSpan.FromHours(1) }));
});
builder.Services.AddDbContext<RedixDbContext>((services, options) => options
    .UseSqlServer(DatabaseConnection.Of(services.GetRequiredService<IConfiguration>()), sql => sql.UseParameterizedCollectionMode(ParameterTranslationMode.Parameter))
    .UseSeeding((context, _) => AdminSeed.Seed(context, services))
    .UseAsyncSeeding((context, _, cancellation) => AdminSeed.SeedAsync(context, services, cancellation)));
builder.Services.AddOptions<AttachmentOptions>().BindConfiguration("Attachments");
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
app.UseRateLimiter();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapTranslations();
app.MapConfig();
app.MapAuth();
app.MapProjects();
app.MapProjectUsers();
app.MapRegistration();
app.MapUsers();
app.MapCategories();
app.MapColumns();
app.MapPaymentMethods();
app.MapAccounts();
app.MapPartners();
app.MapSuppliers();
app.MapContracts();
app.MapAttachments();
app.MapExpenses();
app.MapEntryRecording();
app.MapEntries();
app.MapApprovals();
app.MapNotifications();
app.MapInvoiceAttach();
app.MapReconciliations();
app.MapCapital();
app.MapDashboard();
app.MapItemReport();
app.MapDocuments();
app.MapPeriodicReports();
app.MapReportFiles();
app.MapSyncEndpoints();

app.Run();
