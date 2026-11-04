using PassportCertificateStore.Providers;
using PassportCertificateStore.Providers.DirectoryProvider;
using PassportCertificateStore.Providers.OnlineProvider;

namespace PassportCertificateStore;

public static class ConfigureServices
{
    public static IServiceCollection AddPassportCertificateStore(this IServiceCollection services, IConfiguration configuration)
    {
        const string CONFIGURATION_SECTION = "PassportCertificateStore";
        var section = configuration.GetSection(CONFIGURATION_SECTION);
        if (!section.Exists())
        {
            throw new InvalidOperationException($"The '{CONFIGURATION_SECTION}' configuration section is missing.");
        }

        var typeValue = section.GetValue("Provider", string.Empty);
        if (string.IsNullOrEmpty(typeValue))
        {
            throw new InvalidOperationException($"The '{CONFIGURATION_SECTION}:Provider' configuration value is missing.");
        }

        if (typeValue == DirectoryProvider.ProviderName)
        {
            var options = section.Get<DirectoryProviderOptions>()
                ?? new DirectoryProviderOptions();

            services.AddSingleton(options);
            services.AddSingleton<IMasterlistProvider, DirectoryProvider>();
        }
        else if (typeValue == OnlineProvider.ProviderName)
        {
            var options = section.Get<OnlineProviderOptions>()
                ?? new OnlineProviderOptions();

            services.AddSingleton(options);
            services.AddSingleton<IMasterlistProvider, OnlineProvider>();

            services.AddHttpClient(OnlineProvider.CLIENT_NAME, client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent"
                    , "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
                    + "AppleWebKit/537.36 (KHTML, like Gecko) "
                    + "Chrome/156.0.0.0 Safari/537.36");
            });
        }
        else
        {
            throw new NotSupportedException($"The provider '{typeValue}' is not supported.");
        }

        services.AddSingleton<IPassportCertificateStore, PassportCertificateStore>();

        return services;
    }
}
