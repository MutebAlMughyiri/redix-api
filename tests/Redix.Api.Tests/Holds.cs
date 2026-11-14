using Redix.Api.Auth;

namespace Redix.Api.Tests;

/// <summary>
/// What a person held in a project under each of the five roles before they went (redix-api 89), plus Delete entries wherever Record entries was, as the move gave it, and the
/// report permissions as redix-api #131 D3's upgrade gave them: «يرى التقارير» wherever ReadProject was, «يُصدر التقارير» wherever MoveMoney or SeeAllCapital was. A test says
/// "a finance person" in one word; a theory names the set as a constant and reads it through <see cref="Of"/>.
/// </summary>
public static class Holds
{
    /// <summary>What the project manager role held.</summary>
    public static readonly Permission[] Manager = [Permission.ManageUsers, Permission.ReadProject, Permission.SeeAllCapital, Permission.SeeReports, Permission.GenerateReports];

    /// <summary>What the finance role held.</summary>
    public static readonly Permission[] Finance =
    [
        Permission.ReadProject, Permission.SeeAllCapital, Permission.ManageProject, Permission.ManageStructure, Permission.ManageSettings, Permission.ChargeAnyAccount,
        Permission.AddSupplier, Permission.RecordEntries, Permission.DeleteEntries, Permission.ManageAnyEntry, Permission.AttachFiles, Permission.ApproveEntries, Permission.MoveMoney,
        Permission.SeeReports, Permission.GenerateReports,
    ];

    /// <summary>What the field role held.</summary>
    public static readonly Permission[] Field = [Permission.ReadProject, Permission.AddSupplier, Permission.RecordEntries, Permission.DeleteEntries, Permission.AttachFiles, Permission.SeeReports];

    /// <summary>What the reader role held.</summary>
    public static readonly Permission[] Reader = [Permission.ReadProject, Permission.SeeReports];

    private static readonly Dictionary<string, Permission[]> Named = new()
    {
        [nameof(Manager)] = Manager,
        [nameof(Finance)] = Finance,
        [nameof(Field)] = Field,
        [nameof(Reader)] = Reader,
    };

    /// <summary>The set named <paramref name="name"/>: <c>Manager</c>, <c>Finance</c>, <c>Field</c> or <c>Reader</c>.</summary>
    public static Permission[] Of(string name) => Named[name];
}
