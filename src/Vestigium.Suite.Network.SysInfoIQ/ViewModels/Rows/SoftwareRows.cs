namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels.Rows;

public sealed class InstalledSoftwareRow
{
    public string Location { get; init; } = "";
    public string Arch { get; init; } = "";
    public string Name { get; init; } = "";
    public string Version { get; init; } = "";
    public string Publisher { get; init; } = "";
    public string InstallDate { get; init; } = "";
}

public sealed class HotfixRow
{
    public string HotFixId { get; init; } = "";
    public string Description { get; init; } = "";
    public string InstalledOn { get; init; } = "";
    public string InstalledBy { get; init; } = "";
}

public sealed class FeatureRow
{
    public string Name { get; init; } = "";
    public string State { get; init; } = "";
}

public sealed class OdbcRow
{
    public string Location { get; init; } = "";
    public string Arch { get; init; } = "";
    public string Kind { get; init; } = "";
    public string Name { get; init; } = "";
    public string Detail { get; init; } = "";
}

public sealed class EnvironmentRow
{
    public string Scope { get; init; } = "";
    public string Name { get; init; } = "";
    public string Value { get; init; } = "";
}

public sealed class SqlServiceRow
{
    public string Name { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string Status { get; init; } = "";
    public string StartType { get; init; } = "";
}

public sealed class SoftwareSnapshot
{
    public const int FeaturesCap = 500;

    public List<InstalledSoftwareRow> Installed { get; } = new();
    public List<HotfixRow> Hotfixes { get; } = new();
    public List<FeatureRow> Features { get; } = new();
    public List<OdbcRow> Odbc { get; } = new();
    public List<EnvironmentRow> Environment { get; } = new();
    public List<SqlServiceRow> Sql { get; } = new();
    public bool FeaturesTruncated { get; set; }
    public string? InstalledError { get; set; }
    public string? HotfixesError { get; set; }
    public string? FeaturesError { get; set; }
    public string? OdbcError { get; set; }
    public string? EnvironmentError { get; set; }
    public string? SqlError { get; set; }
}
