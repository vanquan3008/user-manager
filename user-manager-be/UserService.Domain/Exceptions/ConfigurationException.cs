namespace UserService.Domain.Exceptions;

public sealed class ConfigurationException : DomainException
{
    public string ConfigurationKey { get; }

    public ConfigurationException(string configurationKey, string message)
        : base(message)
    {
        ConfigurationKey = configurationKey;
    }
}
