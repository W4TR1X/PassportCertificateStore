namespace PassportCertificateStore.Providers;

public interface IMasterlistProvider
{
    Task<bool?> IsNewerVersionAvailableAsync(CancellationToken cancellationToken = default);
    Task<byte[]?> GetLatestVersionAsync(CancellationToken cancellationToken = default);
}
