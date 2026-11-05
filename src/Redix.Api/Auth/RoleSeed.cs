namespace Redix.Api.Auth;

/// <summary>The roles every database starts with (BRS section 2). After the first migration they are data: changing a row needs no release (BR-32).</summary>
public static class RoleSeed
{
    private const string AdminWord = "users.roleAdmin";

    private static readonly (string WordKey, RoleScope Scope, int SortOrder, Permission[] Permissions)[] Seeded =
    [
        (AdminWord, RoleScope.System, 1, [Permission.ApproveProjects]),
        ("users.roleManager", RoleScope.Project, 1, [Permission.ManageUsers, Permission.ReadProject, Permission.SeeAllCapital]),
        ("users.roleFinance", RoleScope.Project, 2, [Permission.ApproveEntries, Permission.ManageAnyEntry, Permission.ManageProject, Permission.ManageStructure, Permission.ManageSettings, Permission.ChargeAnyAccount, Permission.AddSupplier, Permission.RecordEntries, Permission.AttachFiles, Permission.ReadProject, Permission.SeeAllCapital]),
        ("users.roleField", RoleScope.Project, 3, [Permission.AddSupplier, Permission.RecordEntries, Permission.AttachFiles, Permission.ReadProject]),
        ("users.roleReader", RoleScope.Project, 4, [Permission.ReadProject]),
    ];

    /// <summary>The admin's role, which the first user is given.</summary>
    public static Guid AdminId => IdOf(AdminWord);

    /// <summary>The seeded roles.</summary>
    public static IEnumerable<Role> Roles => Seeded.Select(role => new Role { Id = IdOf(role.WordKey), WordKey = role.WordKey, Scope = role.Scope, SortOrder = role.SortOrder });

    /// <summary>What each seeded role may do.</summary>
    public static IEnumerable<RolePermission> RolePermissions => Seeded.SelectMany(role => role.Permissions.Select(permission =>
        new RolePermission { Id = StableIds.Of($"{role.WordKey}:{permission}"), RoleId = IdOf(role.WordKey), Permission = permission }));

    /// <summary>The id of the seeded role named by <paramref name="wordKey"/>, the same in every database.</summary>
    public static Guid IdOf(string wordKey) => StableIds.Of(wordKey + ":role");
}
