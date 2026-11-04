using PassportCertificateStore.Providers;

namespace PassportCertificateStore.Providers.OnlineProvider;

internal class OnlineProvider : IMasterlistProvider
{
    internal const string ProviderName = "Online";
    internal const string CLIENT_NAME = "ICAOMasterlistReader";

    private readonly ILogger<OnlineProvider> _logger;
    private readonly OnlineProviderOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly Regex MasterListUrlRegex
        = new(@"var masterListFileURL = ""([^""]+)"""
        , RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10));

    public OnlineProvider(ILogger<OnlineProvider> logger, OnlineProviderOptions options
        , IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _options = options;
    }

    private string? latestDownloadedFileName;
    private byte[]? latestDownloadedFileContent;

    public async Task<byte[]?> GetLatestVersionAsync(CancellationToken cancellationToken = default)
    {
        var downloadUrl = await GetDownloadUrl();
        if (downloadUrl is null) return null;

        var currentDownloadedFileName = Path.GetFileName(downloadUrl);
        if (!HasNewerVersion(currentDownloadedFileName))
        {
            return latestDownloadedFileContent;
        }

        using var client = _httpClientFactory.CreateClient(CLIENT_NAME);
        var masterListContent = await client.GetByteArrayAsync(downloadUrl);

        if (masterListContent is null || masterListContent.Length == 0)
        {
            _logger.LogError(
                $"Failed to download the latest masterlist from '{downloadUrl}'.");

            return null;
        }

        latestDownloadedFileName = Path.GetFileName(downloadUrl);
        latestDownloadedFileContent = masterListContent;

        return masterListContent;
    }

    public async Task<bool?> IsNewerVersionAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (latestDownloadedFileName is null) return true;

        var downloadUrl = await GetDownloadUrl();
        if (downloadUrl is null) return null;

        var currentDownloadedFileName = Path.GetFileName(downloadUrl);
        return HasNewerVersion(currentDownloadedFileName);
    }

    private bool HasNewerVersion(string currentDownloadedFileName)
    {
        if (latestDownloadedFileName != currentDownloadedFileName)
        {
            _logger.LogInformation($"Newer version available: {currentDownloadedFileName}");
            return true;
        }

        return false;
    }

    private async Task<string?> GetDownloadUrl()
    {
        using var client = _httpClientFactory.CreateClient(CLIENT_NAME);
        var content = await client.GetStringAsync(_options.PageUrl);

        var match = MasterListUrlRegex.Match(content);
        var url = match.Success ? match.Groups[1].Value : null;

        if (string.IsNullOrEmpty(url))
        {
            _logger.LogError(
                $"Failed to find the masterlist URL in the response from '{_options.PageUrl}'.");

            return null;
        }

        return url;
    }
}
