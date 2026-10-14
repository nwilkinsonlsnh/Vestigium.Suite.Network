namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels.Rows;

public sealed class FirewallProfileRow
{
    public string Name { get; init; } = "";
    public string Enabled { get; init; } = "";
    public string DefaultInbound { get; init; } = "";
    public string DefaultOutbound { get; init; } = "";
    public string NotifyOnListen { get; init; } = "";
    public string LogAllowed { get; init; } = "";
    public string LogBlocked { get; init; } = "";
}

public sealed class FirewallRuleRow
{
    public string DisplayName { get; init; } = "";
    public string Enabled { get; init; } = "";
    public string Direction { get; init; } = "";
    public string Action { get; init; } = "";
    public string Profile { get; init; } = "";
    public string Protocol { get; init; } = "";
}

public sealed class CertificateRow
{
    public string Location { get; init; } = "";
    public string Store { get; init; } = "";
    public string Subject { get; init; } = "";
    public string Issuer { get; init; } = "";
    public string Thumbprint { get; init; } = "";
    public string NotAfter { get; init; } = "";
    public string HasPrivateKey { get; init; } = "";
}

public sealed class SignedDriverRow
{
    public string Source { get; init; } = "";
    public string DeviceName { get; init; } = "";
    public string DriverVersion { get; init; } = "";
    public string Manufacturer { get; init; } = "";
    public string IsSigned { get; init; } = "";
    public string Signer { get; init; } = "";
    public string InfName { get; init; } = "";
}

public sealed class SecuritySnapshot
{
    public const int RulesCap = 1000;
    public const int DriversCap = 5000;

    public List<FirewallProfileRow> Profiles { get; } = new();
    public List<FirewallRuleRow> Rules { get; } = new();
    public List<CertificateRow> Certificates { get; } = new();
    public List<SignedDriverRow> Drivers { get; } = new();
    public bool RulesTruncated { get; set; }
    public bool DriversTruncated { get; set; }
    public string? ProfilesError { get; set; }
    public string? RulesError { get; set; }
    public string? CertificatesError { get; set; }
    public string? DriversError { get; set; }
}
