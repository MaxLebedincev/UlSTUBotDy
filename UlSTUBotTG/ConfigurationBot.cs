using HandlerUlSTU.DTO;

public class ConfigurationBot
{
    public string Token { get; set; } = "";

    public EntryPoint? EntryPointUlSTU { get; set; }

    public Credential? CredentialUlSTU { get; set; }
}