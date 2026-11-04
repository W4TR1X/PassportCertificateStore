using System.Collections.ObjectModel;

namespace PassportCertificateStore;

public interface IPassportCertificateStore
{
    Task<ReadOnlyCollection<X509Certificate2>?> GetCertificates();
    Task<X509Certificate2Collection?> GetCertificateCollection();
}
