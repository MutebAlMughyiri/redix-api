using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Projects;
using Redix.Api.Structure;

namespace Redix.Api.Settings;

/// <summary>A project's payment methods (UC-014, FR-40): added, renamed, disabled and enabled, and deleted while no expense was paid with them.</summary>
public static class PaymentMethodsEndpoints
{
    /// <summary>Maps the routes that read a project's payment methods, for anyone who reads the project, and those that change them, for whoever holds <see cref="Permission.ManageSettings"/>.</summary>
    public static void MapPaymentMethods(this WebApplication app)
    {
        var reads = app.MapGroup("/projects/{projectId:guid}/payment-methods").RequireAuthorization(nameof(Permission.ReadProject));
        reads.MapGet("/", List);
        reads.MapGet("/offered", Offered);

        var changes = app.MapProjectChanges(Permission.ManageSettings);
        changes.MapPost("/payment-methods", Add);
        changes.MapPut("/payment-methods/{paymentMethodId:guid}", Rename);
        changes.MapPost("/payment-methods/{paymentMethodId:guid}/disable", Disable);
        changes.MapPost("/payment-methods/{paymentMethodId:guid}/enable", Enable);
        changes.MapDelete("/payment-methods/{paymentMethodId:guid}", Delete);
    }

    /// <summary>Every payment method of the project, disabled ones included, in order.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database they live in.</param>
    public static Task<List<PaymentMethodView>> List(Guid projectId, RedixDbContext db) => ReadAsync(db.PaymentMethods.Where(method => method.ProjectId == projectId));

    /// <summary>What an entry may choose from: the active payment methods, in order (AC-40.1).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database they live in.</param>
    public static Task<List<PaymentMethodView>> Offered(Guid projectId, RedixDbContext db) => ReadAsync(db.PaymentMethods.Where(method => method.ProjectId == projectId && method.IsActive));

    /// <summary>Adds a payment method at the end of the project's list. An unacceptable name answers <c>invalidName</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The method's name.</param>
    /// <param name="db">The database they live in.</param>
    public static Task<IResult> Add(Guid projectId, NameRequest request, RedixDbContext db) =>
        ProjectChanges.WithNameAsync(request.Name, async name =>
        {
            await AddAsync(projectId, name, db);
            return Results.Ok();
        });

    /// <summary>Adds the payment method <paramref name="name"/>, a name already accepted, at the end of the project's list, and answers its id: the one way one is added, by hand and from a project file's load alike (redix-api #132 D6).</summary>
    public static async Task<Guid> AddAsync(Guid projectId, string name, RedixDbContext db)
    {
        var sortOrder = await db.PaymentMethods.Where(row => row.ProjectId == projectId).OrderByDescending(row => row.SortOrder).Select(row => row.SortOrder + 1).FirstOrDefaultAsync();
        var method = new PaymentMethod { ProjectId = projectId, Name = name, SortOrder = sortOrder };
        db.PaymentMethods.Add(method);
        await db.SaveChangesAsync();
        return method.Id;
    }

    /// <summary>Renames a payment method. An unacceptable name answers <c>invalidName</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="paymentMethodId">The method.</param>
    /// <param name="request">Its new name.</param>
    /// <param name="db">The database they live in.</param>
    public static Task<IResult> Rename(Guid projectId, Guid paymentMethodId, NameRequest request, RedixDbContext db) =>
        ProjectChanges.WithNameAsync(request.Name, name => ChangeAsync(projectId, paymentMethodId, db, method => method.Name = name));

    /// <summary>Disables a payment method: it disappears from entry and stays on the entries that used it (AC-40.1).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="paymentMethodId">The method.</param>
    /// <param name="db">The database they live in.</param>
    public static Task<IResult> Disable(Guid projectId, Guid paymentMethodId, RedixDbContext db) => ChangeAsync(projectId, paymentMethodId, db, method => method.IsActive = false);

    /// <summary>Brings a disabled payment method back into entry.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="paymentMethodId">The method.</param>
    /// <param name="db">The database they live in.</param>
    public static Task<IResult> Enable(Guid projectId, Guid paymentMethodId, RedixDbContext db) => ChangeAsync(projectId, paymentMethodId, db, method => method.IsActive = true);

    /// <summary>Deletes a payment method no expense was paid with. One an expense was paid with answers <c>paymentMethodUsed</c>, since the expense keeps it: disabling it takes it out of entry instead.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="paymentMethodId">The method.</param>
    /// <param name="db">The database they live in.</param>
    public static async Task<IResult> Delete(Guid projectId, Guid paymentMethodId, RedixDbContext db)
    {
        var paymentMethod = await db.PaymentMethods.SingleOrDefaultAsync(method => method.Id == paymentMethodId && method.ProjectId == projectId);
        if (paymentMethod is null)
        {
            return Results.NotFound();
        }
        if (await db.ExpenseDetails.AnyAsync(details => details.PaymentMethodId == paymentMethodId))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "PaymentMethodUsed");
        }
        db.PaymentMethods.Remove(paymentMethod);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static Task<List<PaymentMethodView>> ReadAsync(IQueryable<PaymentMethod> methods) =>
        methods.OrderBy(method => method.SortOrder).Select(method => new PaymentMethodView(method.Id, method.Name, method.IsActive)).ToListAsync();

    private static async Task<IResult> ChangeAsync(Guid projectId, Guid paymentMethodId, RedixDbContext db, Action<PaymentMethod> change)
    {
        var paymentMethod = await db.PaymentMethods.SingleOrDefaultAsync(method => method.Id == paymentMethodId && method.ProjectId == projectId);
        if (paymentMethod is null)
        {
            return Results.NotFound();
        }
        change(paymentMethod);
        await db.SaveChangesAsync();
        return Results.Ok();
    }
}

/// <summary>A payment method as the app shows it.</summary>
/// <param name="Id">The method's id.</param>
/// <param name="Name">The method's name.</param>
/// <param name="IsActive">False once disabled.</param>
public record PaymentMethodView(Guid Id, string Name, bool IsActive);
