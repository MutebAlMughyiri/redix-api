---
name: using-never-qualify
description: In the REDIX API's C#, a type is written by its simple name with a using directive - never written out in full as System.Text.Json.JsonSerializer or Microsoft.Data.SqlClient.SqlException in the middle of a line. Use when writing or editing any C# class or test, especially one-line fixes and test helpers where a fully qualified name is the quick way to avoid adding a using.
---

# Using, never qualify

> Brought from the Testin project, 12 November 2026, where Muteb wrote: *"why did you write java.util.Optional, just write Optional, make sure our classes not have like this."*

A type is written by its simple name, and the `using` at the top says where it comes from:

```csharp
// no
CustomValues = System.Text.Json.JsonSerializer.Serialize(request.CustomValuesKept()),
```

```csharp
// yes
using System.Text.Json;

CustomValues = JsonSerializer.Serialize(request.CustomValuesKept()),
```

A qualified name in the middle of a line is a namespace path the reader has to skip to find the word that matters, and it hides a dependency the `using` list would have shown. It is almost always the quick way out of adding a `using` during a small edit, which is exactly when it slips in.

## The one exception: the simple name already means something else

Qualify only when the simple name already means another type or member in that file. For a type, C# has a using alias (`using SqlException = Microsoft.Data.SqlClient.SqlException;`); prefer the alias to a qualified name repeated in the code.

Three places stay qualified. In `RedixDbContext`, `global::Redix.Api.Entries.Entry.DescriptionMaxLength`: inside a `DbContext`, `Entry` is its method. In `TransfersEndpoints`, `Accounts.Transfer.RefusalOf`: the class's own route method is named `Transfer`. In `ReportFormats`, `QuestPDF.Settings.License`: `Settings` is REDIX's own namespace `Redix.Api.Settings`.

## Fixed, 13 November 2026

The 23 lines measured on 12 November (findings row 212), in `EntriesEndpoints`, `EntryTable`, `RedixDbContext` and 8 test files, now use a `using`. In a type argument, `DbSet<PeriodicReport>`, the simple name is safe even beside a property of the same name: C# looks only for types there.

## Fixed again, 16 November 2026

The first check matched only `System` and `Microsoft`, so a path through REDIX's own namespaces or a package's (`Partners.PartnerSight`, `Config.ConfigValue`, `ClosedXML.Excel.XLDataType`) slipped by (findings row 220). Those lines now use a `using`, and the check below matches any namespace.

## Checking

Namespace paths in code, outside `using` and `namespace` lines, doc comments and generated folders: REDIX's own namespaces, then the packages'.

```bash
ns=$(grep -rhoE "^namespace Redix\.Api\.[A-Za-z]+" src --include=*.cs | sed 's/namespace Redix\.Api\.//' | sort -u | paste -sd'|')
grep -rnP "(?<![\w.\"<>])(System|Microsoft|ClosedXML|QuestPDF|Redix|$ns)\.[A-Z]\w*\.\w+" src tests --include=*.cs \
  | grep -vE "/(Migrations|obj|bin)/" | grep -vE ":[0-9]+:\s*(using|namespace|global using) " | grep -v '///'
```

Only the three places above should answer. It also matches strings; a hit inside a string literal stays.
