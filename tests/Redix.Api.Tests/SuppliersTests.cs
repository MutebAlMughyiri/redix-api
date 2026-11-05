using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Auth;
using Redix.Api.Projects;
using Redix.Api.Structure;
using Redix.Api.Suppliers;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class SuppliersTests(MigratedApiFactory factory)
{
    private const string Password = "Suppliers-pass-1";
    private const string Finance = "users.roleFinance";
    private const string Field = "users.roleField";
    private const string Reader = "users.roleReader";
    private const string Manager = "users.roleManager";
    private const string VatNumber = "300000000000003";
    private const string OtherVatNumber = "311111111111113";

    private static readonly JsonSerializerOptions Json = MigratedApiFactory.Json;

    private async Task<(HttpClient Client, ProjectView Project)> FinanceAsync(string name = "غصون 6")
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        var project = await factory.ApprovedProjectAsync(client, name);
        await factory.GiveRoleAsync(user.Id, project.Id, Finance);
        return (client, project);
    }

    private async Task<HttpClient> MemberAsync(ProjectView project, string role)
    {
        var user = await factory.AddUserAsync(Password);
        await factory.GiveRoleAsync(user.Id, project.Id, role);
        return await factory.ClientSignedInAsAsync(user, Password);
    }

    private static async Task<List<SupplierView>> RegisterOf(HttpClient client, Guid projectId) =>
        (await client.GetFromJsonAsync<List<SupplierView>>($"/projects/{projectId}/suppliers", Json))!;

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, Guid projectId, SupplierRequest request) =>
        client.PostAsJsonAsync($"/projects/{projectId}/suppliers", request, Json);

    private static async Task<SupplierView> AddAsync(HttpClient client, Guid projectId, string name, string vatNumber = VatNumber) =>
        (await (await PostAsync(client, projectId, new SupplierRequest(name) { VatNumber = vatNumber })).EnsureSuccessStatusCode().Content.ReadFromJsonAsync<SupplierView>(Json))!;

    private static async Task<string> CodeOf(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    private async Task<Guid> AnItemOfAsync(Guid projectId)
    {
        var itemId = Guid.Empty;
        await factory.WithDatabaseAsync(async db => itemId = await db.Items.Where(item => db.Categories.Any(category => category.Id == item.CategoryId && category.ProjectId == projectId)).Select(item => item.Id).FirstAsync());
        return itemId;
    }

    [Fact]
    public async Task FinanceAddsASupplierWithAllItsDetails()
    {
        var (client, project) = await FinanceAsync();
        var itemId = await AnItemOfAsync(project.Id);

        var added = await PostAsync(client, project.Id, new SupplierRequest(" شركة النخيل ") { VatNumber = $" {VatNumber} ", Mobile = " 0500000000 ", DefaultItemId = itemId });

        Assert.Equal(HttpStatusCode.Created, added.StatusCode);
        var supplier = (await added.Content.ReadFromJsonAsync<SupplierView>(Json))!;
        Assert.Equal("شركة النخيل", supplier.Name);
        Assert.Equal(VatNumber, supplier.VatNumber);
        Assert.Equal("0500000000", supplier.Mobile);
        Assert.Equal(itemId.ToString(), supplier.DefaultItemId);
        Assert.NotEqual("", supplier.DefaultItemName);
        Assert.Equal(SupplierStatus.Approved, supplier.Status);
        Assert.True(supplier.IsActive);
    }

    [Fact]
    public async Task AVatNumberOf14DigitsIsRefused()
    {
        var (client, project) = await FinanceAsync();

        var refused = await PostAsync(client, project.Id, new SupplierRequest("مورد") { VatNumber = "30088967710003" });

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("invalidVatNumber", await CodeOf(refused));
    }

    [Theory]
    [InlineData("100000000000003")]
    [InlineData("300000000000001")]
    [InlineData("3000000000000003")]
    [InlineData("30000000000000a")]
    [InlineData("300000 00000003")]
    [InlineData("٣٠٠٠٠٠٠٠٠٠٠٠٠٠٣")]
    [InlineData("")]
    public async Task AVatNumberMustBe15DigitsStartingAndEndingWith3(string vatNumber)
    {
        var (client, project) = await FinanceAsync();

        var refused = await PostAsync(client, project.Id, new SupplierRequest("مورد") { VatNumber = vatNumber });

        Assert.Equal("invalidVatNumber", await CodeOf(refused));
    }

    [Fact]
    public async Task AVatNumberAnotherSupplierHoldsIsRefusedNamingThatSupplier()
    {
        var (client, project) = await FinanceAsync();
        var holder = await AddAsync(client, project.Id, "شركة النخيل");

        var refused = await PostAsync(client, project.Id, new SupplierRequest("النخيل للتجارة") { VatNumber = VatNumber });

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        var answer = await refused.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("vatNumberTaken", answer.GetProperty("code").GetString());
        var existing = answer.GetProperty("existing").Deserialize<SupplierView>(Json)!;
        Assert.Equal(holder.Id, existing.Id);
        Assert.Equal("شركة النخيل", existing.Name);
    }

    [Fact]
    public async Task ADisabledSupplierStillHoldsItsVatNumber()
    {
        var (client, project) = await FinanceAsync();
        var holder = await AddAsync(client, project.Id, "شركة النخيل");
        (await client.PostAsync($"/projects/{project.Id}/suppliers/{holder.Id}/disable", null)).EnsureSuccessStatusCode();

        var refused = await PostAsync(client, project.Id, new SupplierRequest("النخيل للتجارة") { VatNumber = VatNumber });

        var existing = (await refused.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("existing").Deserialize<SupplierView>(Json)!;
        Assert.False(existing.IsActive);
    }

    [Fact]
    public async Task TheDatabaseHoldsAVatNumberToOneSupplierInAProject()
    {
        var (client, project) = await FinanceAsync();
        var holder = await AddAsync(client, project.Id, "شركة النخيل");

        await factory.WithDatabaseAsync(async db =>
        {
            db.Suppliers.Add(new Supplier { ProjectId = project.Id, Name = "نسخة", VatNumber = VatNumber, CreatedByUserId = (await db.Suppliers.SingleAsync(row => row.Id == holder.Id)).CreatedByUserId });
            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        });
    }

    [Fact]
    public async Task OneVatNumberMayBeInTwoProjects()
    {
        var (client, project) = await FinanceAsync();
        var (otherClient, other) = await FinanceAsync("غصون 5");
        await AddAsync(client, project.Id, "شركة النخيل");

        Assert.Equal(VatNumber, (await AddAsync(otherClient, other.Id, "شركة النخيل")).VatNumber);
    }

    [Fact]
    public async Task ASupplierNotVatRegisteredIsSavedWithNoNumber()
    {
        var (client, project) = await FinanceAsync();

        var added = await PostAsync(client, project.Id, new SupplierRequest("بقالة الحي") { NotVatRegistered = true, VatNumber = "123" });

        Assert.Equal("", (await added.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<SupplierView>(Json))!.VatNumber);
    }

    [Fact]
    public async Task AnyNumberOfSuppliersMayHaveNoVatNumber()
    {
        var (client, project) = await FinanceAsync();

        (await PostAsync(client, project.Id, new SupplierRequest("بقالة الحي") { NotVatRegistered = true })).EnsureSuccessStatusCode();
        (await PostAsync(client, project.Id, new SupplierRequest("بقالة الشارع") { NotVatRegistered = true })).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AnEditChangesASupplierAndLeavesItItsOwnNumber()
    {
        var (client, project) = await FinanceAsync();
        var supplier = await AddAsync(client, project.Id, "شركة النخيل");
        var itemId = await AnItemOfAsync(project.Id);

        (await client.PostAsJsonAsync($"/projects/{project.Id}/suppliers/{supplier.Id}", new SupplierRequest("النخيل") { VatNumber = VatNumber, Mobile = "0511111111", DefaultItemId = itemId }, Json)).EnsureSuccessStatusCode();

        var edited = (await RegisterOf(client, project.Id)).Single(row => row.Id == supplier.Id);
        Assert.Equal("النخيل", edited.Name);
        Assert.Equal(VatNumber, edited.VatNumber);
        Assert.Equal("0511111111", edited.Mobile);
        Assert.Equal(itemId.ToString(), edited.DefaultItemId);
    }

    [Fact]
    public async Task AnEditCanTakeTheDefaultItemAndTheVatNumberAway()
    {
        var (client, project) = await FinanceAsync();
        var added = await PostAsync(client, project.Id, new SupplierRequest("شركة النخيل") { VatNumber = VatNumber, DefaultItemId = await AnItemOfAsync(project.Id) });
        var supplier = (await added.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<SupplierView>(Json))!;

        (await client.PostAsJsonAsync($"/projects/{project.Id}/suppliers/{supplier.Id}", new SupplierRequest("شركة النخيل") { NotVatRegistered = true }, Json)).EnsureSuccessStatusCode();

        var edited = (await RegisterOf(client, project.Id)).Single(row => row.Id == supplier.Id);
        Assert.Equal("", edited.VatNumber);
        Assert.Equal("", edited.DefaultItemId);
        Assert.Equal("", edited.DefaultItemName);
    }

    [Fact]
    public async Task AnEditToANumberAnotherSupplierHoldsIsRefused()
    {
        var (client, project) = await FinanceAsync();
        await AddAsync(client, project.Id, "شركة النخيل");
        var supplier = await AddAsync(client, project.Id, "شركة الأفق", OtherVatNumber);

        var refused = await client.PostAsJsonAsync($"/projects/{project.Id}/suppliers/{supplier.Id}", new SupplierRequest("شركة الأفق") { VatNumber = VatNumber }, Json);

        Assert.Equal("vatNumberTaken", await CodeOf(refused));
    }

    [Fact]
    public async Task ASupplierNeedsANameOfAtMost100Characters()
    {
        var (client, project) = await FinanceAsync();

        foreach (var name in new[] { "   ", new string('x', Project.NameMaxLength + 1) })
        {
            Assert.Equal("invalidName", await CodeOf(await PostAsync(client, project.Id, new SupplierRequest(name) { VatNumber = VatNumber })));
        }
    }

    [Fact]
    public async Task AMobileMayHaveAtMost20Characters()
    {
        var (client, project) = await FinanceAsync();

        var refused = await PostAsync(client, project.Id, new SupplierRequest("مورد") { VatNumber = VatNumber, Mobile = new string('5', User.MobileMaxLength + 1) });

        Assert.Equal("invalidMobile", await CodeOf(refused));
    }

    [Fact]
    public async Task ADefaultItemMustBeInTheProject()
    {
        var (client, project) = await FinanceAsync();
        var (_, other) = await FinanceAsync("غصون 5");

        foreach (var itemId in new[] { await AnItemOfAsync(other.Id), Guid.CreateVersion7() })
        {
            Assert.Equal("unknownItem", await CodeOf(await PostAsync(client, project.Id, new SupplierRequest("مورد") { VatNumber = VatNumber, DefaultItemId = itemId })));
        }
    }

    [Fact]
    public async Task ADisabledSupplierStaysInTheRegisterAndCanBeEnabled()
    {
        var (client, project) = await FinanceAsync();
        var supplier = await AddAsync(client, project.Id, "شركة النخيل");

        (await client.PostAsync($"/projects/{project.Id}/suppliers/{supplier.Id}/disable", null)).EnsureSuccessStatusCode();
        Assert.False((await RegisterOf(client, project.Id)).Single(row => row.Id == supplier.Id).IsActive);

        (await client.PostAsync($"/projects/{project.Id}/suppliers/{supplier.Id}/enable", null)).EnsureSuccessStatusCode();
        Assert.True((await RegisterOf(client, project.Id)).Single(row => row.Id == supplier.Id).IsActive);
    }

    [Fact]
    public async Task TheRegisterListsSuppliersByName()
    {
        var (client, project) = await FinanceAsync();
        await AddAsync(client, project.Id, "ياسر");
        await AddAsync(client, project.Id, "باسم", OtherVatNumber);

        Assert.Equal(["باسم", Supplier.GeneralCashName, "ياسر"], (await RegisterOf(client, project.Id)).Select(row => row.Name));
    }

    [Fact]
    public async Task AnApprovedProjectStartsWithTheGeneralCashSupplier()
    {
        var (client, project) = await FinanceAsync();

        var general = Assert.Single(await RegisterOf(client, project.Id));

        Assert.Equal(Supplier.GeneralCashName, general.Name);
        Assert.Equal("", general.VatNumber);
        Assert.Equal(SupplierStatus.Approved, general.Status);
        Assert.True(general.IsActive);
    }

    [Fact]
    public async Task ACopiedProjectStartsWithOnlyTheGeneralCashSupplier()
    {
        var (client, earlier) = await FinanceAsync();
        await AddAsync(client, earlier.Id, "شركة النخيل");
        var created = await client.PostAsJsonAsync("/projects", new CreateProjectRequest("غصون 7", new DateOnly(2026, 11, 1), []) { StructureSourceId = earlier.Id });
        var later = (await created.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ProjectView>(Json))!;

        var admin = await factory.ClientSignedInAsAsync(await factory.AddUserAsync(Password, isAdmin: true), Password);
        (await admin.PostAsync($"/projects/{later.Id}/approve", null)).EnsureSuccessStatusCode();

        Assert.Equal(Supplier.GeneralCashName, Assert.Single(await RegisterOf(client, later.Id)).Name);
    }

    [Theory]
    [InlineData(Manager)]
    [InlineData(Field)]
    [InlineData(Reader)]
    public async Task EveryoneWithAccessReadsTheRegister(string role)
    {
        var (client, project) = await FinanceAsync();
        await AddAsync(client, project.Id, "شركة النخيل");
        var memberClient = await MemberAsync(project, role);

        Assert.Equal(2, (await RegisterOf(memberClient, project.Id)).Count);
    }

    [Theory]
    [InlineData(Manager)]
    [InlineData(Reader)]
    public async Task OnlyFinanceAndFieldAddSuppliers(string role)
    {
        var (_, project) = await FinanceAsync();
        var memberClient = await MemberAsync(project, role);

        Assert.Equal(HttpStatusCode.Forbidden, (await PostAsync(memberClient, project.Id, new SupplierRequest("مورد") { VatNumber = OtherVatNumber })).StatusCode);
    }

    [Theory]
    [InlineData(Manager)]
    [InlineData(Field)]
    [InlineData(Reader)]
    public async Task OnlyFinanceChangesSuppliers(string role)
    {
        var (client, project) = await FinanceAsync();
        var supplier = await AddAsync(client, project.Id, "شركة النخيل");
        var memberClient = await MemberAsync(project, role);

        foreach (var refused in new[]
        {
            await memberClient.PostAsJsonAsync($"/projects/{project.Id}/suppliers/{supplier.Id}", new SupplierRequest("مورد") { VatNumber = VatNumber }, Json),
            await memberClient.PostAsync($"/projects/{project.Id}/suppliers/{supplier.Id}/disable", null),
            await memberClient.PostAsync($"/projects/{project.Id}/suppliers/{supplier.Id}/enable", null),
        })
        {
            Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
        }
    }

    [Fact]
    public async Task OneProjectsSuppliersAreNeverAnothers()
    {
        var (client, project) = await FinanceAsync();
        var (otherClient, other) = await FinanceAsync("غصون 5");
        var theirs = await AddAsync(otherClient, other.Id, "موردهم");

        Assert.DoesNotContain(await RegisterOf(client, project.Id), row => row.Id == theirs.Id);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync($"/projects/{project.Id}/suppliers/{theirs.Id}", new SupplierRequest("سرقة") { VatNumber = OtherVatNumber }, Json)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync($"/projects/{project.Id}/suppliers/{theirs.Id}/disable", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/projects/{other.Id}/suppliers")).StatusCode);
    }

    private static async Task<List<SupplierView>> SearchAsync(HttpClient client, Guid projectId, string text) =>
        (await client.GetFromJsonAsync<List<SupplierView>>($"/projects/{projectId}/suppliers/search?text={Uri.EscapeDataString(text)}", Json))!;

    [Fact]
    public async Task ThreeLettersOfTheNameFindTheSupplier()
    {
        var (client, project) = await FinanceAsync();
        await AddAsync(client, project.Id, "شركة النخيل");
        await AddAsync(client, project.Id, "مؤسسة الواحة", OtherVatNumber);

        Assert.Equal("شركة النخيل", Assert.Single(await SearchAsync(client, project.Id, "نخي")).Name);
    }

    [Fact]
    public async Task PartOfTheVatNumberFindsTheSupplier()
    {
        var (client, project) = await FinanceAsync();
        await AddAsync(client, project.Id, "شركة النخيل");
        await AddAsync(client, project.Id, "مؤسسة الواحة", OtherVatNumber);

        Assert.Equal("مؤسسة الواحة", Assert.Single(await SearchAsync(client, project.Id, "1111")).Name);
    }

    [Fact]
    public async Task FewerThanThreeLettersNarrowNothingDown()
    {
        var (client, project) = await FinanceAsync();
        await AddAsync(client, project.Id, "شركة النخيل");

        Assert.Equal(2, (await SearchAsync(client, project.Id, "نخ")).Count);
    }

    [Fact]
    public async Task NoTextOffersTheSuppliersByName()
    {
        var (client, project) = await FinanceAsync();
        await AddAsync(client, project.Id, "ياسر");
        await AddAsync(client, project.Id, "باسم", OtherVatNumber);

        Assert.Equal(["باسم", Supplier.GeneralCashName, "ياسر"], (await client.GetFromJsonAsync<List<SupplierView>>($"/projects/{project.Id}/suppliers/search", Json))!.Select(row => row.Name));
    }

    [Fact]
    public async Task AnExactVatNumberFindsItsSupplierAlone()
    {
        var (client, project) = await FinanceAsync();
        await AddAsync(client, project.Id, "شركة النخيل");
        await AddAsync(client, project.Id, "مؤسسة الواحة", OtherVatNumber);

        var found = Assert.Single(await SearchAsync(client, project.Id, VatNumber));

        Assert.Equal("شركة النخيل", found.Name);
        Assert.Equal(VatNumber, found.VatNumber);
    }

    [Fact]
    public async Task AVatNumberNoSupplierHoldsFindsNothing()
    {
        var (client, project) = await FinanceAsync();
        await AddAsync(client, project.Id, "شركة النخيل");

        Assert.Empty(await SearchAsync(client, project.Id, OtherVatNumber));
    }

    [Fact]
    public async Task ADisabledSupplierIsNotOffered()
    {
        var (client, project) = await FinanceAsync();
        var supplier = await AddAsync(client, project.Id, "شركة النخيل");
        (await client.PostAsync($"/projects/{project.Id}/suppliers/{supplier.Id}/disable", null)).EnsureSuccessStatusCode();

        Assert.Empty(await SearchAsync(client, project.Id, "نخيل"));
        Assert.DoesNotContain(await SearchAsync(client, project.Id, ""), row => row.Id == supplier.Id);
    }

    [Fact]
    public async Task ASearchAnswersAtMost20SuppliersWithinASecond()
    {
        var (client, project) = await FinanceAsync();
        var userId = Guid.Empty;
        await factory.WithDatabaseAsync(async db =>
        {
            userId = await db.ProjectMembers.Where(member => member.ProjectId == project.Id).Select(member => member.UserId).FirstAsync();
            db.Suppliers.AddRange(Enumerable.Range(0, 500).Select(number => new Supplier { ProjectId = project.Id, Name = $"مورد البناء {number}", CreatedByUserId = userId }));
            await db.SaveChangesAsync();
        });
        await SearchAsync(client, project.Id, "");

        var watch = System.Diagnostics.Stopwatch.StartNew();
        var found = await SearchAsync(client, project.Id, "البناء");
        watch.Stop();

        Assert.Equal(Supplier.SearchLimit, found.Count);
        Assert.True(watch.Elapsed < TimeSpan.FromSeconds(1), $"The search took {watch.Elapsed}.");
    }

    [Theory]
    [InlineData(Manager)]
    [InlineData(Field)]
    [InlineData(Reader)]
    public async Task EveryoneWithAccessSearches(string role)
    {
        var (client, project) = await FinanceAsync();
        await AddAsync(client, project.Id, "شركة النخيل");
        var memberClient = await MemberAsync(project, role);

        Assert.Single(await SearchAsync(memberClient, project.Id, "نخيل"));
    }

    [Fact]
    public async Task ASearchNeverReachesAnotherProject()
    {
        var (client, project) = await FinanceAsync();
        var (otherClient, other) = await FinanceAsync("غصون 5");
        await AddAsync(otherClient, other.Id, "شركة النخيل");

        Assert.Empty(await SearchAsync(client, project.Id, "نخيل"));
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/projects/{other.Id}/suppliers/search")).StatusCode);
    }

    [Fact]
    public async Task WhatFieldAddsWaitsForApproval()
    {
        var (_, project) = await FinanceAsync();
        var fieldClient = await MemberAsync(project, Field);

        var supplier = await AddAsync(fieldClient, project.Id, "شركة النخيل");

        Assert.Equal(SupplierStatus.PendingApproval, supplier.Status);
        Assert.Equal(SupplierStatus.PendingApproval, (await RegisterOf(fieldClient, project.Id)).Single(row => row.Id == supplier.Id).Status);
    }

    [Fact]
    public async Task WhatFieldAddsIsValidatedAsAnySupplier()
    {
        var (_, project) = await FinanceAsync();
        var fieldClient = await MemberAsync(project, Field);

        Assert.Equal("invalidVatNumber", await CodeOf(await PostAsync(fieldClient, project.Id, new SupplierRequest("مورد") { VatNumber = "30088967710003" })));
        Assert.Equal("invalidName", await CodeOf(await PostAsync(fieldClient, project.Id, new SupplierRequest(" ") { VatNumber = VatNumber })));
    }

    [Fact]
    public async Task AVatNumberAlreadyInTheRegisterIsRefusedToFieldNamingItsSupplier()
    {
        var (client, project) = await FinanceAsync();
        var holder = await AddAsync(client, project.Id, "شركة النخيل");
        var fieldClient = await MemberAsync(project, Field);

        var refused = await PostAsync(fieldClient, project.Id, new SupplierRequest("النخيل") { VatNumber = VatNumber });

        var answer = await refused.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("vatNumberTaken", answer.GetProperty("code").GetString());
        Assert.Equal(holder.Id, answer.GetProperty("existing").Deserialize<SupplierView>(Json)!.Id);
    }

    [Fact]
    public async Task APendingSupplierAlreadyHoldsItsVatNumber()
    {
        var (_, project) = await FinanceAsync();
        var firstClient = await MemberAsync(project, Field);
        var secondClient = await MemberAsync(project, Field);
        await AddAsync(firstClient, project.Id, "شركة النخيل");

        Assert.Equal("vatNumberTaken", await CodeOf(await PostAsync(secondClient, project.Id, new SupplierRequest("النخيل") { VatNumber = VatNumber })));
    }

    [Fact]
    public async Task APendingSupplierIsOfferedToWhoAddedItAndToFinanceOnly()
    {
        var (client, project) = await FinanceAsync();
        var adderClient = await MemberAsync(project, Field);
        var otherClient = await MemberAsync(project, Field);
        var pending = await AddAsync(adderClient, project.Id, "شركة النخيل");

        Assert.Equal(pending.Id, Assert.Single(await SearchAsync(adderClient, project.Id, "نخيل")).Id);
        Assert.Equal(pending.Id, Assert.Single(await SearchAsync(client, project.Id, "نخيل")).Id);
        Assert.Empty(await SearchAsync(otherClient, project.Id, "نخيل"));
        Assert.Empty(await SearchAsync(otherClient, project.Id, VatNumber));
    }

    [Fact]
    public async Task EveryoneWithAccessSeesAPendingSupplierInTheRegister()
    {
        var (_, project) = await FinanceAsync();
        var adderClient = await MemberAsync(project, Field);
        var readerClient = await MemberAsync(project, Reader);
        var pending = await AddAsync(adderClient, project.Id, "شركة النخيل");

        Assert.Equal(SupplierStatus.PendingApproval, (await RegisterOf(readerClient, project.Id)).Single(row => row.Id == pending.Id).Status);
    }

    [Fact]
    public async Task AFinishedProjectRefusesWhatFieldAdds()
    {
        var (client, project) = await FinanceAsync();
        var fieldClient = await MemberAsync(project, Field);
        (await client.PostAsync($"/projects/{project.Id}/close", null)).EnsureSuccessStatusCode();

        Assert.Equal("projectFinished", await CodeOf(await PostAsync(fieldClient, project.Id, new SupplierRequest("مورد") { VatNumber = VatNumber })));
    }

    [Fact]
    public async Task AFinishedProjectRefusesEveryChangeAndStaysReadable()
    {
        var (client, project) = await FinanceAsync();
        var supplier = await AddAsync(client, project.Id, "شركة النخيل");
        (await client.PostAsync($"/projects/{project.Id}/close", null)).EnsureSuccessStatusCode();

        foreach (var refused in new[]
        {
            await PostAsync(client, project.Id, new SupplierRequest("مورد") { VatNumber = OtherVatNumber }),
            await client.PostAsJsonAsync($"/projects/{project.Id}/suppliers/{supplier.Id}", new SupplierRequest("مورد") { VatNumber = VatNumber }, Json),
            await client.PostAsync($"/projects/{project.Id}/suppliers/{supplier.Id}/disable", null),
        })
        {
            Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
            Assert.Equal("projectFinished", await CodeOf(refused));
        }
        Assert.Equal(2, (await RegisterOf(client, project.Id)).Count);
    }
}
