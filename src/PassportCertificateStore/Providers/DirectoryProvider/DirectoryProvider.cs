namespace PassportCertificateStore.Providers.DirectoryProvider;

internal class DirectoryProvider(ILogger<DirectoryProvider> logger, DirectoryProviderOptions options) : IMasterlistProvider
{
    internal const string ProviderName = "Directory";

    private readonly ILogger<DirectoryProvider> _logger = logger;
    private readonly DirectoryProviderOptions _options = options;

    public Task<byte[]?> GetLatestVersionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool?> IsNewerVersionAvailableAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
