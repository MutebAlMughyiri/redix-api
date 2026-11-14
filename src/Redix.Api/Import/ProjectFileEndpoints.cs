using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Accounts;
using Redix.Api.Auth;
using Redix.Api.Contracts;
using Redix.Api.Entries;
using Redix.Api.Partners;
using Redix.Api.Projects;
using Redix.Api.Reports;
using Redix.Api.Settings;
using Redix.Api.Structure;
using Redix.Api.Suppliers;

namespace Redix.Api.Import;

/// <summary>
/// Loads a project's file into REDIX (UC-071, FR-53, BR-43): a project that lived in Excel comes in at once, every part of it through the owner that takes it by hand, inside one
/// transaction. The load holds no rule of its own: it reads the file, finds or adds each part, and asks the owners; one refusal anywhere saves nothing.
/// </summary>
public static class ProjectFileEndpoints
{
    /// <summary>What a contract's payments table is called before its contractor's name, as the workbook names it.</summary>
    private const string ContractTablePrefix = "دفعات ";

    /// <summary>Maps the route that loads a project file, for the manager who created the project, who holds <see cref="Permission.ManageProject"/> (D1).</summary>
    /// <param name="app">The app.</param>
    public static void MapProjectFile(this WebApplication app) => app.MapProjectChanges(Permission.ManageProject).MapPost("/project-file", Load).DisableAntiforgery();

    /// <summary>
    /// Loads a project file into an approved project that holds no entry yet (FR-53, D4). A file that is not a project file answers <c>notAProjectFile</c>, a newer one
    /// <c>newerVersion</c> (AC-53.9), and a project holding an entry <c>projectHasEntries</c> (AC-53.3), each before anything changes. Then its missing categories, items, tables,
    /// contracts, partners, suppliers and payment methods are added (D5, D6, D13), every row is recorded through Add entry's own checks and approved in the caller's name (D7,
    /// AC-53.4), counting from the file's report time (D12), what each account really held becomes one reconciliation (D8, AC-53.8), and the file's report is issued with its number
    /// and time (D9, AC-53.7). Any refusal, a row's or a part's, answers <c>rowsRefused</c> with every one of them under <c>rows</c>, and nothing is saved (D3, D14, AC-53.5).
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="file">The file, sent as every file the app sends, so its upload waits as long as a file may (UC-070).</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the project lives in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Load(Guid projectId, IFormFile file, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        var (projectFile, refusal) = ProjectFile.Read(await reader.ReadToEndAsync());
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }
        if ((await Project.HoldingEntriesAsync([projectId], db)).Count > 0)
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "ProjectHasEntries");
        }

        var userId = AccessTokens.UserIdOf(principal);
        var at = time.GetUtcNow().UtcDateTime;
        var refused = new List<ProjectFileRefusal>();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var registers = await RegistersOfAsync(projectId, projectFile, userId, db, refused);
        var tables = await TablesOfAsync(projectId, projectFile, userId, at, db, refused);
        var balances = BalancesOf(projectFile, registers, refused);
        var held = (await UserAccess.OfAsync(userId, db)).In(projectId);
        var kept = await RecordAsync(projectId, tables, projectFile, registers, userId, held, db, time, refused);
        if (refused.Count > 0)
        {
            return Refused(refused);
        }

        foreach (var entry in kept.Select(row => row.Entry))
        {
            await ApprovalsEndpoints.ApproveAsync(entry, userId, at, projectFile.Report.Number > 0 ? projectFile.Report.At : at, db);
        }
        await db.SaveChangesAsync();
        refusal = projectFile.Accounts.Count > 0 ? await ReconciliationsEndpoints.EnterAsync(projectId, balances, userId, at, db) : "";
        if (refusal.Length > 0)
        {
            return Refused([ProjectFileRefusal.Of(ProjectFileOwner.Reconciliation, refusal)]);
        }
        if (projectFile.Report.Number > 0)
        {
            await PeriodicReportsEndpoints.IssueAsync(projectId, projectFile.Report.Number, projectFile.Report.At, userId, db);
        }
        await transaction.CommitAsync();
        return Results.Ok(new ProjectFileLoadedView(kept.Count, kept.Select(row => row.TableId).Distinct().Count(), projectFile.Report.Number));
    }

    private static IResult Refused(List<ProjectFileRefusal> refused) => Refusal.Of(StatusCodes.Status400BadRequest, "RowsRefused", "rows", refused);

    /// <summary>
    /// The project's accounts, suppliers and payment methods by name, once the suppliers and payment methods of the file's lists it lacks are added through their owners (D6). An
    /// account is never added: the file carries no type or custodian.
    /// </summary>
    private static async Task<ProjectFileRegisters> RegistersOfAsync(Guid projectId, ProjectFile file, Guid userId, RedixDbContext db, List<ProjectFileRefusal> refused)
    {
        var suppliers = await db.Suppliers.Where(supplier => supplier.ProjectId == projectId).Select(supplier => supplier.Name).ToListAsync();
        foreach (var name in LabelsOf(file, "supplier").Except(suppliers))
        {
            var (_, refusal, _) = await SuppliersEndpoints.AddAsync(projectId, new SupplierRequest(name), userId, db);
            refused.AddRange(refusal.Length > 0 ? [ProjectFileRefusal.Of(ProjectFileOwner.Supplier, refusal) with { Value = name }] : []);
        }
        var methods = await db.PaymentMethods.Where(method => method.ProjectId == projectId).Select(method => method.Name).ToListAsync();
        foreach (var name in LabelsOf(file, "paymentMethod").Except(methods))
        {
            if (Project.IsAcceptableName(name))
            {
                await PaymentMethodsEndpoints.AddAsync(projectId, name, db);
            }
            else
            {
                refused.Add(ProjectFileRefusal.Of(ProjectFileOwner.PaymentMethod, nameof(ProjectChangeOutcome.InvalidName)) with { Value = name });
            }
        }
        return new ProjectFileRegisters(
            ByName(await db.Accounts.Where(account => account.ProjectId == projectId).Select(account => new { account.Name, account.Id }).ToListAsync(), row => row.Name, row => row.Id),
            ByName(await db.Suppliers.Where(supplier => supplier.ProjectId == projectId).Select(supplier => new { supplier.Name, supplier.Id }).ToListAsync(), row => row.Name, row => row.Id),
            ByName(await db.PaymentMethods.Where(method => method.ProjectId == projectId).Select(method => new { method.Name, method.Id }).ToListAsync(), row => row.Name, row => row.Id));
    }

    private static List<string> LabelsOf(ProjectFile file, string listKey) =>
        [.. file.Lists.Where(list => list.Key == listKey).SelectMany(list => list.Options).Select(option => option.Label.Trim()).Where(label => label.Length > 0).Distinct()];

    private static Dictionary<string, Guid> ByName<T>(List<T> rows, Func<T, string> nameOf, Func<T, Guid> idOf) => rows.DistinctBy(nameOf).ToDictionary(nameOf, idOf);

    /// <summary>
    /// Every table of the file with the item and the table of the project it goes into: a category and an item found by name, or added at the end (D5, D13); a table found by name
    /// within its item, or added through its owner: a subgroup, a contract with the contractor its name gives after «دفعات», or a partner (D6). A part its owner refuses is listed,
    /// and its rows with it.
    /// </summary>
    private static async Task<List<ProjectFileTablePlace>> TablesOfAsync(Guid projectId, ProjectFile file, Guid userId, DateTime at, RedixDbContext db, List<ProjectFileRefusal> refused)
    {
        var places = new List<ProjectFileTablePlace>();
        foreach (var fileCategory in file.Categories)
        {
            var categoryName = fileCategory.Name.Trim();
            if (!Project.IsAcceptableName(categoryName))
            {
                refused.Add(ProjectFileRefusal.Of(ProjectFileOwner.Structure, nameof(StructureChangeOutcome.InvalidName)) with { Category = fileCategory.Name });
                continue;
            }
            var categoryId = await db.Categories.Where(category => category.ProjectId == projectId && category.Name == categoryName).Select(category => category.Id).FirstOrDefaultAsync();
            categoryId = categoryId != Guid.Empty ? categoryId : (await CategoriesEndpoints.AddCategoryAsync(projectId, categoryName, db)).Id;
            foreach (var fileItem in fileCategory.Items)
            {
                var item = await ItemOfAsync(projectId, categoryId, fileItem, db);
                if (item.Refusal.Length > 0)
                {
                    refused.Add(ProjectFileRefusal.Of(ProjectFileOwner.Structure, item.Refusal) with { Category = categoryName, Item = fileItem.Name });
                    continue;
                }
                foreach (var fileTable in fileItem.Tables)
                {
                    var (owner, tableId, refusal) = await TableOfAsync(projectId, item.Item, fileTable, userId, at, db);
                    var place = new ProjectFileTablePlace(categoryName, item.Item, fileTable, tableId);
                    if (refusal.Length > 0)
                    {
                        refused.Add(place.Refusal(0, owner, refusal));
                    }
                    else
                    {
                        places.Add(place);
                    }
                }
            }
        }
        return places;
    }

    /// <summary>The item of <paramref name="fileItem"/>'s name in the category <paramref name="categoryId"/>, with its subgroups, or one added with its type; the owner's refusal otherwise.</summary>
    private static async Task<(Item Item, string Refusal)> ItemOfAsync(Guid projectId, Guid categoryId, ProjectFileItem fileItem, RedixDbContext db)
    {
        var name = fileItem.Name.Trim();
        if (!Project.IsAcceptableName(name))
        {
            return (Item.None, nameof(StructureChangeOutcome.InvalidName));
        }
        var item = await db.Items.Include(row => row.Subgroups).FirstOrDefaultAsync(row => row.CategoryId == categoryId && row.Name == name);
        return item is null ? await CategoriesEndpoints.AddItemAsync(projectId, categoryId, name, fileItem.Type, db) : (item, "");
    }

    /// <summary>The id of the table of <paramref name="item"/> that <paramref name="fileTable"/> is, found by name or added through its owner; that owner and its refusal otherwise.</summary>
    private static async Task<(ProjectFileOwner Owner, Guid TableId, string Refusal)> TableOfAsync(Guid projectId, Item item, ProjectFileTable fileTable, Guid userId, DateTime at, RedixDbContext db)
    {
        var name = fileTable.Name.Trim();
        if (!Project.IsAcceptableName(name))
        {
            return (ProjectFileOwner.Structure, Guid.Empty, nameof(StructureChangeOutcome.InvalidName));
        }
        return fileTable.Kind switch
        {
            ItemType.ContractorPayments => await ContractOfAsync(projectId, item, ContractorOf(name), fileTable.ContractAmount, userId, at, db),
            ItemType.Capital => await PartnerOfAsync(projectId, item, name, userId, at, db),
            _ => await SubgroupOfAsync(item, name, db),
        };
    }

    private static string ContractorOf(string tableName) => tableName.StartsWith(ContractTablePrefix) ? tableName[ContractTablePrefix.Length..].Trim() : tableName;

    private static async Task<(ProjectFileOwner Owner, Guid TableId, string Refusal)> SubgroupOfAsync(Item item, string name, RedixDbContext db)
    {
        var subgroupName = name == item.Name ? Subgroup.GeneralName : name;
        var found = item.Subgroups.FirstOrDefault(subgroup => subgroup.Name == subgroupName);
        if (found is not null)
        {
            return (ProjectFileOwner.Structure, found.Id, "");
        }
        var outcome = await CategoriesEndpoints.AddSubgroupAsync(item, name, db);
        return outcome == StructureChangeOutcome.Done
            ? (ProjectFileOwner.Structure, await db.Subgroups.Where(subgroup => subgroup.ItemId == item.Id && subgroup.Name == name).Select(subgroup => subgroup.Id).FirstAsync(), "")
            : (ProjectFileOwner.Structure, Guid.Empty, outcome.ToString());
    }

    /// <summary>The contract of <paramref name="item"/> with <paramref name="contractor"/>, or one added for the value agreed, an open value when the file holds none (D6).</summary>
    private static async Task<(ProjectFileOwner Owner, Guid TableId, string Refusal)> ContractOfAsync(Guid projectId, Item item, string contractor, JsonElement contractAmount, Guid userId, DateTime at, RedixDbContext db)
    {
        var contracts = from contract in db.Contracts join supplier in db.Suppliers on contract.SupplierId equals supplier.Id where contract.ItemId == item.Id && supplier.Name == contractor select contract.Id;
        var contractId = await contracts.FirstOrDefaultAsync();
        if (contractId != Guid.Empty)
        {
            return (ProjectFileOwner.Contract, contractId, "");
        }
        var supplierId = await db.Suppliers.Where(supplier => supplier.ProjectId == projectId && supplier.Name == contractor).Select(supplier => supplier.Id).FirstOrDefaultAsync();
        if (supplierId == Guid.Empty)
        {
            var (added, supplierRefusal, _) = await SuppliersEndpoints.AddAsync(projectId, new SupplierRequest(contractor), userId, db);
            if (supplierRefusal.Length > 0)
            {
                return (ProjectFileOwner.Supplier, Guid.Empty, supplierRefusal);
            }
            supplierId = added.Id;
        }
        var isOpenValue = contractAmount.ValueKind != JsonValueKind.Number;
        var refusal = await ContractsEndpoints.AddAsync(projectId, new AddContractRequest(supplierId, item.Id, isOpenValue ? 0 : contractAmount.GetDecimal(), isOpenValue), userId, at, db);
        return refusal.Length > 0 ? (ProjectFileOwner.Contract, Guid.Empty, refusal) : (ProjectFileOwner.Contract, await contracts.FirstAsync(), "");
    }

    /// <summary>The partner <paramref name="name"/> of the capital item <paramref name="item"/>, or one added with no login (D6).</summary>
    private static async Task<(ProjectFileOwner Owner, Guid TableId, string Refusal)> PartnerOfAsync(Guid projectId, Item item, string name, Guid userId, DateTime at, RedixDbContext db)
    {
        var partners = db.Partners.Where(Partner.Kept).Where(partner => partner.ProjectId == projectId && partner.ItemId == item.Id && partner.Name == name).Select(partner => partner.Id);
        var partnerId = await partners.FirstOrDefaultAsync();
        if (partnerId != Guid.Empty)
        {
            return (ProjectFileOwner.Partner, partnerId, "");
        }
        var refusal = await PartnersEndpoints.AddAsync(projectId, name, Guid.Empty, userId, at, db);
        return refusal.Length > 0 ? (ProjectFileOwner.Partner, Guid.Empty, refusal) : (ProjectFileOwner.Partner, await partners.FirstAsync(), "");
    }

    /// <summary>What each account of the file really held, by the project's account of its name (D8); an account the project lacks is listed as <c>accountNotInProject</c> (D6).</summary>
    private static List<ActualBalanceRequest> BalancesOf(ProjectFile file, ProjectFileRegisters registers, List<ProjectFileRefusal> refused)
    {
        var balances = new List<ActualBalanceRequest>();
        foreach (var account in file.Accounts)
        {
            if (registers.Accounts.TryGetValue(account.Name.Trim(), out var accountId))
            {
                balances.Add(new ActualBalanceRequest(accountId, account.ActualBalance));
            }
            else
            {
                refused.Add(ProjectFileRefusal.Of(ProjectFileOwner.File, "AccountNotInProject") with { Value = account.Name });
            }
        }
        return balances;
    }

    /// <summary>
    /// Records every row of every table through <see cref="EntryRecording.RecordAsync"/>, as Add entry does, telling no decider (D7): a table not offered to the caller is listed once,
    /// and each row refused is listed with its table, its place counted from 1 as the sheet's «#» counts it, and its code. Answers the entries kept, each with its table.
    /// </summary>
    private static async Task<List<(Entry Entry, Guid TableId)>> RecordAsync(Guid projectId, List<ProjectFileTablePlace> places, ProjectFile file, ProjectFileRegisters registers, Guid userId, List<Permission> held, RedixDbContext db, TimeProvider time, List<ProjectFileRefusal> refused)
    {
        var offered = await EntryTable.OfferedAsync(projectId, held, db);
        var options = file.Lists.SelectMany(list => list.Options.Select(option => (list.Key, Option: option))).DistinctBy(row => row.Option.Id).ToDictionary(row => row.Option.Id, row => (List: row.Key, Label: row.Option.Label.Trim()));
        var kept = new List<(Entry Entry, Guid TableId)>();
        foreach (var place in places)
        {
            var table = offered.SingleOrDefault(row => row.Item.Id == place.Item.Id && row.Id == place.TableId);
            if (table is null)
            {
                refused.Add(place.Refusal(0, ProjectFileOwner.Entry, "UnknownTable"));
                continue;
            }
            foreach (var (row, number) in place.FileTable.Rows.Select((row, index) => (row, index + 1)))
            {
                var read = RequestOf(row, place.FileTable, table, options, registers);
                if (read.Code.Length > 0)
                {
                    refused.Add(place.Refusal(number, ProjectFileOwner.File, read.Code) with { ColumnKey = read.ColumnKey, Value = read.Value, Column = read.Column });
                    continue;
                }
                var (_, code, columnKey, entry) = await EntryRecording.RecordAsync(projectId, read.Request, table, userId, held, Entry.None, tellDeciders: false, db, time);
                if (code.Length > 0)
                {
                    refused.Add(place.Refusal(number, ProjectFileOwner.Entry, code) with { ColumnKey = columnKey });
                }
                else
                {
                    kept.Add((entry, table.Id));
                }
            }
        }
        return kept;
    }

    /// <summary>
    /// What <paramref name="row"/> asks Add entry to record, sent for approval, in <paramref name="table"/>: each value under its column's key, a list's option by its label. An option
    /// the file's lists do not hold, or a choice the table's list does not have, answers <c>unknownChoice</c>, naming the value and the column (G4); an account the project lacks
    /// <c>accountNotInProject</c> (D6); a file named <c>attachmentNotInFile</c>, since a file carries none (G3). Everything else is Add entry's to check.
    /// </summary>
    private static ProjectFileRow RequestOf(Dictionary<string, JsonElement> row, ProjectFileTable fileTable, EntryTable table, Dictionary<string, (string List, string Label)> options, ProjectFileRegisters registers)
    {
        var request = new EntryRequest(Guid.CreateVersion7()) { ItemId = table.Item.Id, TableId = table.Id, Submit = true };
        var columnValues = new Dictionary<string, string>();
        foreach (var (key, element) in row)
        {
            var column = fileTable.Columns.FirstOrDefault(column => column.Key == key, new ProjectFileColumn { Key = key });
            var value = TextOf(element);
            if (column.Type == ColumnDataType.List && value.Length > 0)
            {
                if (!options.TryGetValue(value, out var option) || option.List != column.List)
                {
                    return ProjectFileRow.Refused("UnknownChoice", key, value, column.Name);
                }
                value = option.Label;
            }
            switch (key)
            {
                case "description":
                    request = request with { Description = value };
                    break;
                case "date":
                    request = request with { Date = DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : DateOnly.MinValue };
                    break;
                case "total" or "amount":
                    request = request with { Amount = decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ? amount : 0 };
                    break;
                case "account" when value.Length > 0:
                    if (!registers.Accounts.TryGetValue(value, out var accountId))
                    {
                        return ProjectFileRow.Refused("AccountNotInProject", key, value, column.Name);
                    }
                    request = request with { AccountId = accountId };
                    break;
                case "supplier":
                    request = request with { SupplierId = registers.Suppliers.GetValueOrDefault(value) };
                    break;
                case "paymentMethod":
                    request = request with { PaymentMethodId = registers.PaymentMethods.GetValueOrDefault(value) };
                    break;
                case "outsideContract" when value.Length > 0:
                    if (value != ReportColumns.PlaceInContract(true) && value != ReportColumns.PlaceInContract(false))
                    {
                        return ProjectFileRow.Refused("UnknownChoice", key, value, column.Name);
                    }
                    request = request with { IsOutsideContract = value == ReportColumns.PlaceInContract(true) };
                    break;
                case "attachment" when value.Length > 0:
                    return ProjectFileRow.Refused("AttachmentNotInFile", key, value, column.Name);
                case "account" or "outsideContract" or "attachment":
                    break;
                default:
                    var choices = table.Columns.Where(asked => asked.Key == key && asked.DataType == ColumnDataType.List && asked.Choices.Count > 0).SelectMany(asked => asked.Choices).ToList();
                    if (choices.Count > 0 && value.Length > 0 && choices.All(choice => choice.Name != value))
                    {
                        return ProjectFileRow.Refused("UnknownChoice", key, value, column.Name);
                    }
                    columnValues[key] = choices.Count > 0 && value.Length > 0 ? choices.First(choice => choice.Name == value).Id.ToString() : value;
                    break;
            }
        }
        return new ProjectFileRow(request with { ColumnValues = columnValues }, "", "", "", "");
    }

    /// <summary>A row's value as the file wrote it: a text as it is, a number in the invariant culture, anything else empty.</summary>
    private static string TextOf(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString()!.Trim(),
        JsonValueKind.Number => value.GetRawText(),
        _ => "",
    };
}

/// <summary>What a project file's load did (FR-53), for the message the app shows (say only what happened).</summary>
/// <param name="Entries">How many entries were recorded and approved.</param>
/// <param name="Tables">How many tables they went into.</param>
/// <param name="Report">The number of the report issued; 0 when the file held none.</param>
public record ProjectFileLoadedView(int Entries, int Tables, int Report);

/// <summary>Whose rule refused a part of a project file, which says whose words the app shows for its code (D14).</summary>
public enum ProjectFileOwner
{
    /// <summary>The file itself, read as a project: <c>accountNotInProject</c>, <c>attachmentNotInFile</c>, <c>unknownChoice</c>.</summary>
    File,

    /// <summary>Add entry, which every row goes through (AC-53.4).</summary>
    Entry,

    /// <summary>The structure: a category, an item or a subgroup.</summary>
    Structure,

    /// <summary>A contract.</summary>
    Contract,

    /// <summary>A partner.</summary>
    Partner,

    /// <summary>A supplier of the register.</summary>
    Supplier,

    /// <summary>A payment method.</summary>
    PaymentMethod,

    /// <summary>The reconciliation of the actual balances.</summary>
    Reconciliation,
}

/// <summary>
/// One refusal of a project file's load (AC-53.5, D14): where it is, by the file's category, item and table and the row's place counted from 1 (0 for the part itself), whose rule
/// refused it and its code, with the column, the value and the column's name a code names.
/// </summary>
/// <param name="Category">The file's category; empty for a part outside the structure.</param>
/// <param name="Item">The file's item; empty when the refusal is not inside one.</param>
/// <param name="Table">The file's table; empty when the refusal is not inside one.</param>
/// <param name="Row">The row's place in its table, counted from 1; 0 for the part itself.</param>
/// <param name="Owner">Whose rule refused it.</param>
/// <param name="Code">Its code, in camelCase as every refusal's.</param>
public record ProjectFileRefusal(string Category, string Item, string Table, int Row, ProjectFileOwner Owner, string Code)
{
    /// <summary>The key of the column it names; empty when it names none.</summary>
    public string ColumnKey { get; init; } = "";

    /// <summary>The value it names: an account's or a choice's; empty when it names none.</summary>
    public string Value { get; init; } = "";

    /// <summary>The name of the column it names; empty when it names none.</summary>
    public string Column { get; init; } = "";

    /// <summary>A refusal by <paramref name="owner"/> with the code <paramref name="reason"/>, outside any table.</summary>
    public static ProjectFileRefusal Of(ProjectFileOwner owner, string reason) => new("", "", "", 0, owner, JsonNamingPolicy.CamelCase.ConvertName(reason));
}

/// <summary>The project's accounts, suppliers and payment methods by name, which a row's list values name.</summary>
/// <param name="Accounts">The accounts.</param>
/// <param name="Suppliers">The suppliers.</param>
/// <param name="PaymentMethods">The payment methods.</param>
internal sealed record ProjectFileRegisters(Dictionary<string, Guid> Accounts, Dictionary<string, Guid> Suppliers, Dictionary<string, Guid> PaymentMethods);

/// <summary>A table of the file with the item and the table of the project it goes into.</summary>
/// <param name="Category">The category's name.</param>
/// <param name="Item">The project's item.</param>
/// <param name="FileTable">The file's table.</param>
/// <param name="TableId">The project's table: a subgroup, a contract or a partner.</param>
internal sealed record ProjectFileTablePlace(string Category, Item Item, ProjectFileTable FileTable, Guid TableId)
{
    /// <summary>A refusal of the row <paramref name="row"/> of this table, or of the table itself for 0.</summary>
    public ProjectFileRefusal Refusal(int row, ProjectFileOwner owner, string reason) => ProjectFileRefusal.Of(owner, reason) with { Category = Category, Item = Item.Name, Table = FileTable.Name, Row = row };
}

/// <summary>A row of the file as Add entry is asked to record it, or why it cannot be asked at all.</summary>
/// <param name="Request">What Add entry is asked.</param>
/// <param name="Code">Why not; empty when it can be asked.</param>
/// <param name="ColumnKey">The column the refusal names.</param>
/// <param name="Value">The value the refusal names.</param>
/// <param name="Column">The name of the column the refusal names.</param>
internal sealed record ProjectFileRow(EntryRequest Request, string Code, string ColumnKey, string Value, string Column)
{
    /// <summary>A row refused with <paramref name="code"/>, over <paramref name="value"/> in the column <paramref name="columnKey"/> named <paramref name="column"/>.</summary>
    public static ProjectFileRow Refused(string code, string columnKey, string value, string column) => new(new EntryRequest(Guid.Empty), code, columnKey, value, column);
}
