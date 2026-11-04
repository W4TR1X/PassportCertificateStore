using PassportCertificateStore.Providers;

namespace PassportCertificateStore.Providers.DirectoryProvider;

internal class DirectoryProvider : IMasterlistProvider
{
    internal const string ProviderName = "Directory";

    private readonly ILogger<DirectoryProvider> _logger;
    private readonly DirectoryProviderOptions _options;

    public DirectoryProvider(ILogger<DirectoryProvider> logger, DirectoryProviderOptions options)
    {
        _logger = logger;
        _options = options;
    }

    public Task<byte[]> GetLatestVersionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool?> IsNewerVersionAvailableAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
