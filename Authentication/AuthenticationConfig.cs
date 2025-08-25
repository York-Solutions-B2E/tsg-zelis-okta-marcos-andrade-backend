namespace SecurityAuditDashboard.Api.Authentication;

public class AuthenticationConfig
{
    public OktaSettings Okta { get; set; } = new();
    public GoogleSettings Google { get; set; } = new();
}

public class OktaSettings
{
    public string Domain { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Authority => $"https://{Domain}/oauth2/default";
}

public class GoogleSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}