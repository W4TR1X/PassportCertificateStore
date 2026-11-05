using System.Collections.ObjectModel;
using System.Formats.Asn1;
using System.Security.Cryptography;

namespace PassportCertificateStore;

internal sealed class PassportCertificateStore(ILogger<PassportCertificateStore> logger, IMasterlistProvider provider) : IPassportCertificateStore
{
    private readonly ILogger<PassportCertificateStore> _logger = logger;
    private readonly IMasterlistProvider _provider = provider;
    private X509Certificate2Collection? collection;
    private List<X509Certificate2>? list;

    private async Task<bool> LoadCertificates()
    {
        if (list?.Count > 0)
        {
            var status = await _provider.IsNewerVersionAvailableAsync();
            if (status is not true) return true;
        }

        var masterlistContent = await _provider.GetLatestVersionAsync();
        if (masterlistContent is null || masterlistContent.Length == 0)
        {
            if (list?.Count > 0)
            {
                _logger.LogWarning("Failed to retrieve the latest masterlist version. Using cached certificates.");
                return true;
            }
            else
            {
                return false;
            }
        }

        try
        {
            var cms = new SignedCms();
            cms.Decode(masterlistContent);

            var asnReader = new AsnReader(cms.ContentInfo.Content, AsnEncodingRules.BER);

            var sequenceReader = asnReader.ReadSequence();
            _ = sequenceReader.ReadInteger();

            var certificateSet = sequenceReader.ReadSetOf();

            var currentList = new List<X509Certificate2>();
            var currentCollection = new X509Certificate2Collection();
            var certificateIndex = 0;

            while (certificateSet.HasData)
            {
                var encodedValue = certificateSet.ReadEncodedValue();
                certificateIndex++;

                try
                {
                    var cert = X509CertificateLoader.LoadCertificate(encodedValue.ToArray());

                    currentList.Add(cert);
                    currentCollection.Add(cert);
                }
                catch (CryptographicException ex)
                {
                    _logger.LogWarning(ex,
                        "Unable to import masterlist certificate {CertificateIndex}. Skipping this certificate.",
                        certificateIndex);
                }
            }

            sequenceReader.ThrowIfNotEmpty();
            asnReader.ThrowIfNotEmpty();

            if (currentList.Count == 0)
            {
                _logger.LogError("No masterlist certificates could be imported.");

                if (list?.Count > 0)
                {
                    _logger.LogWarning("Using cached certificates because no new certificates could be imported.");
                    return true;
                }

                return false;
            }

            _logger.LogInformation("Imported {ImportedCount} of {TotalCount} masterlist certificates.",
                currentList.Count, certificateIndex);

            list?.Clear();
            collection?.Clear();

            list = currentList;
            collection = currentCollection;
        }
        catch (AsnContentException ex)
        {
            _logger.LogError(ex
                , "ASN content is malformed or does not conform to the expected format.");

            if (list?.Count > 0)
            {
                _logger.LogWarning("Using cached certificates due to ASN content error.");
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            if (list?.Count > 0)
            {
                _logger.LogWarning("Using cached certificates due to an error.");
                return true;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public async Task<X509Certificate2Collection?> GetCertificateCollection()
    {
        var status = await LoadCertificates();
        if (!status) return null;

        return collection;
    }

    public async Task<ReadOnlyCollection<X509Certificate2>?> GetCertificates()
    {
        var status = await LoadCertificates();
        if (!status) return null;

        return list?.AsReadOnly();
    }
}
