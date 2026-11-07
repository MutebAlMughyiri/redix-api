using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Redix.Api.Auth;
using Redix.Api.Projects;
using Redix.Api.Structure;
using Redix.Api.Suppliers;

namespace Redix.Api.Tests;

[Collection(DatabaseCollection.Name)]
public class SuppliersTests(MigratedApiFactory factory)
{
    private const string Password = "Suppliers-pass-1";
    private const string Finance = "Finance";
    private const string Field = "Field";
    private const string Reader = "Reader";
    private const string Manager = "Manager";
    private const string VatNumber = "300000000000003";
    private const string OtherVatNumber = "311111111111113";

    private static readonly JsonSerializerOptions Json = MigratedApiFactory.Json;

    private async Task<(HttpClient Client, ProjectView Project)> FinanceAsync(string name = "غصون 6")
    {
        var user = await factory.AddUserAsync(Password);
        var client = await factory.ClientSignedInAsAsync(user, Password);
        var project = await factory.ApprovedProjectAsync(client, name);
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(Finance));
        return (client, project);
    }

    private async Task<HttpClient> MemberAsync(ProjectView project, string role)
    {
        var user = await factory.AddUserAsync(Password);
        await factory.GiveAsync(user.Id, project.Id, Holds.Of(role));
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
    public async Task ASuppliersMobileIsDigitsOnAddAndOnEdit()
    {
        var (client, project) = await FinanceAsync();
        var supplier = await AddAsync(client, project.Id, "مورد");

        Assert.Equal("invalidMobile", await CodeOf(await PostAsync(client, project.Id, new SupplierRequest("مورد آخر") { NotVatRegistered = true, Mobile = "٠٥٠٠٠٠٠٠٠٠" })));
        Assert.Equal("invalidMobile", await CodeOf(await client.PostAsJsonAsync($"/projects/{project.Id}/suppliers/{supplier.Id}", new SupplierRequest("مورد") { VatNumber = VatNumber, Mobile = "050 000" }, Json)));
        (await client.PostAsJsonAsync($"/projects/{project.Id}/suppliers/{supplier.Id}", new SupplierRequest("مورد") { VatNumber = VatNumber, Mobile = "+966500000000" }, Json)).EnsureSuccessStatusCode();
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

        Assert.Equal(["باسم", Supplier.GeneralCashSupplierName, "ياسر"], (await RegisterOf(client, project.Id)).Select(row => row.Name));
    }

    [Fact]
    public async Task AnApprovedProjectStartsWithTheGeneralCashSupplier()
    {
        var (client, project) = await FinanceAsync();

        var general = Assert.Single(await RegisterOf(client, project.Id));

        Assert.Equal(Supplier.GeneralCashSupplierName, general.Name);
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

        Assert.Equal(Supplier.GeneralCashSupplierName, Assert.Single(await RegisterOf(client, later.Id)).Name);
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

        Assert.Equal(["باسم", Supplier.GeneralCashSupplierName, "ياسر"], (await client.GetFromJsonAsync<List<SupplierView>>($"/projects/{project.Id}/suppliers/search", Json))!.Select(row => row.Name));
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
            userId = await db.ProjectPermissions.Where(held => held.ProjectId == project.Id).Select(held => held.UserId).FirstAsync();
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

    private static Task<HttpResponseMessage> DisableAsync(HttpClient client, Guid projectId, Guid supplierId) =>
        client.PostAsync($"/projects/{projectId}/suppliers/{supplierId}/disable", null);

    [Fact]
    public async Task TheGeneralCashSupplierCannotBeDisabled()
    {
        var (client, project) = await FinanceAsync();
        var general = Assert.Single(await RegisterOf(client, project.Id));

        var refused = await DisableAsync(client, project.Id, general.Id);

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal("generalCashSupplier", await CodeOf(refused));
        Assert.True(Assert.Single(await RegisterOf(client, project.Id)).IsActive);
    }

    [Fact]
    public async Task TheGeneralCashSupplierCanBeRenamedAndStaysTheGeneralOne()
    {
        var (client, project) = await FinanceAsync();
        var general = Assert.Single(await RegisterOf(client, project.Id));

        (await client.PostAsJsonAsync($"/projects/{project.Id}/suppliers/{general.Id}", new SupplierRequest("نقدي") { NotVatRegistered = true }, Json)).EnsureSuccessStatusCode();

        Assert.Equal("نقدي", Assert.Single(await RegisterOf(client, project.Id)).Name);
        Assert.Equal("generalCashSupplier", await CodeOf(await DisableAsync(client, project.Id, general.Id)));
    }

    [Fact]
    public async Task ASupplierNamedLikeTheGeneralOneCanBeDisabled()
    {
        var (client, project) = await FinanceAsync();
        var general = Assert.Single(await RegisterOf(client, project.Id));
        (await client.PostAsJsonAsync($"/projects/{project.Id}/suppliers/{general.Id}", new SupplierRequest("نقدي") { NotVatRegistered = true }, Json)).EnsureSuccessStatusCode();
        var named = await AddAsync(client, project.Id, Supplier.GeneralCashSupplierName);

        (await DisableAsync(client, project.Id, named.Id)).EnsureSuccessStatusCode();
    }

    /// <summary>A project written as the schema of these older migrations has it, so a column added to projects later does not reach back into them.</summary>
    private static async Task<Guid> OldProjectAsync(RedixDbContext db, string name)
    {
        var id = Guid.CreateVersion7();
        await db.Database.ExecuteSqlAsync($"INSERT INTO Projects (Id, Name, StartDate, Status, RejectionReason, IsTemplate, StructureSourceId, VatRate) VALUES ({id}, {name}, {new DateOnly(2026, 11, 1)}, 1, '', 0, {Guid.Empty}, 0.15)");
        return id;
    }

    [Fact]
    public Task TheMigrationMarksTheRowTheApprovalNamedInAProjectApprovedBeforeIt() =>
        factory.OnScratchDatabaseAsync("RedixGeneralCashTests", async db =>
        {
            var migrator = db.GetService<IMigrator>();
            var names = db.GetService<IMigrationsAssembly>().Migrations.Keys.ToList();
            var target = names.Single(name => name.EndsWith("_GeneralCashSupplierFlag"));
            await migrator.MigrateAsync(names[names.IndexOf(target) - 1]);
            var user = new User { Username = "approver", CreatedAt = factory.Time.GetUtcNow().UtcDateTime };
            db.Add(user);
            await db.SaveChangesAsync();
            var project = await OldProjectAsync(db, "غصون 6");
            var otherProject = await OldProjectAsync(db, "غصون 7");
            Guid general = Guid.CreateVersion7(), other = Guid.CreateVersion7(), renamed = Guid.CreateVersion7();
            foreach (var (id, projectId, name) in new[] { (general, project, Supplier.GeneralCashSupplierName), (other, project, "شركة النخيل"), (renamed, otherProject, "نقدي") })
            {
                await db.Database.ExecuteSqlAsync($"INSERT INTO Suppliers (Id, ProjectId, Name, VatNumber, Mobile, Status, IsActive, CreatedByUserId) VALUES ({id}, {projectId}, {name}, '', '', 0, 1, {user.Id})");
            }

            await migrator.MigrateAsync(target);

            var marked = await db.Database.SqlQuery<Guid>($"SELECT Id AS Value FROM Suppliers WHERE IsGeneralCash = 1").ToListAsync();
            Assert.Equal(general, Assert.Single(marked));
        });

    [Theory]
    [InlineData("أحمد للمقاولات", "احمد")]
    [InlineData("مؤسسة الواحة", "مؤسسه")]
    [InlineData("إبراهيم وأولاده", "ابراهيم")]
    [InlineData("آل سعود", "ال سعود")]
    [InlineData("مصطفى للبناء", "مصطفي")]
    [InlineData("شئون البناء", "شيون")]
    [InlineData("سَعِيد الحَرْبي", "سعيد")]
    [InlineData("احمد للمقاولات", "أحمد")]
    [InlineData("محمد للبناء", "مُحَمَّد")]
    public async Task ASearchTreatsArabicSpellingVariantsAlikeAndIgnoresTashkeel(string name, string typed)
    {
        var (client, project) = await FinanceAsync();
        var supplier = await AddAsync(client, project.Id, name);

        Assert.Equal(supplier.Id, Assert.Single(await SearchAsync(client, project.Id, typed)).Id);
    }

    [Fact]
    public async Task AnAnswerShowsTheNameAsItWasTyped()
    {
        var (client, project) = await FinanceAsync();
        await AddAsync(client, project.Id, "مؤسسة الواحة");

        Assert.Equal("مؤسسة الواحة", Assert.Single(await SearchAsync(client, project.Id, "مؤسسه")).Name);
    }

    [Fact]
    public async Task ARenamedSupplierIsFoundByItsNewNameAndNoLongerByItsOldOne()
    {
        var (client, project) = await FinanceAsync();
        var supplier = await AddAsync(client, project.Id, "أحمد للمقاولات");

        (await client.PostAsJsonAsync($"/projects/{project.Id}/suppliers/{supplier.Id}", new SupplierRequest("مؤسسة الواحة") { VatNumber = VatNumber }, Json)).EnsureSuccessStatusCode();

        Assert.Equal(supplier.Id, Assert.Single(await SearchAsync(client, project.Id, "مؤسسه")).Id);
        Assert.Empty(await SearchAsync(client, project.Id, "احمد"));
    }

    [Fact]
    public async Task TashkeelCountsForNothingTowardTheThreeLettersASearchNeeds()
    {
        var (client, project) = await FinanceAsync();
        await AddAsync(client, project.Id, "شركة النخيل");

        Assert.Equal(2, (await SearchAsync(client, project.Id, "نَخِ")).Count);
    }

    [Theory]
    [InlineData("أإآ", "ااا")]
    [InlineData("مدرسة", "مدرسه")]
    [InlineData("موسى", "موسي")]
    [InlineData("مسؤول", "مسوول")]
    [InlineData("مسئول", "مسيول")]
    [InlineData("مَدْرَسَةٌ", "مدرسه")]
    [InlineData("شَدَّة وتنوينًا ٍ", "شده وتنوينا ")]
    [InlineData("Company 15", "Company 15")]
    public void ArabicSpellingKeepsOneFormOfEachLetterAndNoTashkeel(string text, string normalized) => Assert.Equal(normalized, ArabicSpelling.Normalize(text));

    [Fact]
    public Task TheMigrationFillsTheNormalizedNameOfSuppliersSavedBeforeIt() =>
        factory.OnScratchDatabaseAsync("RedixNormalizedNameTests", async db =>
        {
            var migrator = db.GetService<IMigrator>();
            var names = db.GetService<IMigrationsAssembly>().Migrations.Keys.ToList();
            var target = names.Single(name => name.EndsWith("_SupplierNormalizedName"));
            await migrator.MigrateAsync(names[names.IndexOf(target) - 1]);
            var user = new User { Username = "approver", CreatedAt = factory.Time.GetUtcNow().UtcDateTime };
            db.Add(user);
            await db.SaveChangesAsync();
            var project = await OldProjectAsync(db, "غصون 6");
            const string name = "أَحْمَد إبراهيم مؤسسة آل مصطفى شئون";
            var id = Guid.CreateVersion7();
            await db.Database.ExecuteSqlAsync($"INSERT INTO Suppliers (Id, ProjectId, Name, VatNumber, Mobile, Status, IsActive, IsGeneralCash, CreatedByUserId) VALUES ({id}, {project}, {name}, '', '', 0, 1, 0, {user.Id})");

            await migrator.MigrateAsync(target);

            var kept = await db.Database.SqlQuery<string>($"SELECT NormalizedName AS Value FROM Suppliers WHERE Id = {id}").SingleAsync();
            Assert.Equal(ArabicSpelling.Normalize(name), kept);
        });
}
