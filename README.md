# PassportCertificateStore

PassportCertificateStore is a .NET Standard 2.0 library that loads an ICAO Doc 9303 CSCA master list and returns the X.509 certificates inside it. Those certificates are the country signing trust anchors used when verifying electronic Machine Readable Travel Documents (eMRTDs) such as ePassports and ID cards.

## NuGet

[![NuGet](https://img.shields.io/nuget/v/PassportCertificateStore.svg)](https://www.nuget.org/packages/PassportCertificateStore)

Install via NuGet Package Manager:

```bash
dotnet add package PassportCertificateStore
```

[View on NuGet.org](https://www.nuget.org/packages/PassportCertificateStore)

**This library loads CSCA certificates from an ICAO master list.**

Master list signatures are left unverified, and eMRTD passive authentication is left to the caller.

## Features

- **Online Download**: Resolves the latest master list URL from a configured HTML page and downloads the file.
- **In-Memory Cache**: Keeps parsed certificates in memory and refreshes them when the downloaded file name changes.
- **CMS and ASN.1 Extraction**: Decodes the CMS SignedData container and reads the BER certificate set (`version` and `SET OF Certificate`).
- **Dependency Injection**: Registers the store from the `PassportCertificateStore` configuration section.
- **Cache Fallback**: Returns the cached certificates when a refresh or parse fails after a successful load.

## Example Usage

Published packages include a default `PageUrl`. You can omit `PageUrl` to use that default or override it as shown below. The URL must point to an HTML page that contains `var masterListFileURL = "<download url>"`.

```json
{
  "PassportCertificateStore": {
    "Provider": "Online",
    "PageUrl": "https://example.com/masterlist-page"
  }
}
```

Register the store on a generic host that already has configuration and logging:

```csharp
builder.Services.AddPassportCertificateStore(builder.Configuration);

var store = app.Services.GetRequiredService<IPassportCertificateStore>();

var certificates = await store.GetCertificates();
if (certificates is not null)
{
    foreach (var certificate in certificates)
    {
        Console.WriteLine(certificate.Subject);
    }
}

var collection = await store.GetCertificateCollection();
```

`GetCertificates()` and `GetCertificateCollection()` share the same load path. Both return `null` when the master list cannot be loaded and no cached certificates exist. A missing `PassportCertificateStore` section or `Provider` value throws `InvalidOperationException`. An unknown provider throws `NotSupportedException`.

## Providers

- **Online**: Downloads the master list from the URL published on `PageUrl`. A newer version is detected by comparing the downloaded file name. Published packages include a default page URL; when building locally, set `PageUrl` in configuration because the source default is the `__PAGE_URL__` placeholder.
- **Directory**: Registered when `Provider` is `Directory`. The `Directory` option defaults to `./Masterlist`. Loading from that folder is not implemented yet.

## Publishing

Add these repository secrets under **Settings → Secrets and variables → Actions**:

- `PAGE_URL`: The default master list page URL to embed in the package.
- `NUGET_API_KEY`: The API key used to publish to NuGet.org.

The publishing workflow runs on pushes to `main`, tags matching `v*`, and manual dispatches. A tag such as `v1.2.3` publishes version `1.2.3`. Other runs use the highest stable `v<major>.<minor>.<patch>` Git tag, increment its patch, and append `-preview.<run_number>`: after `v1.2.3`, build 125 publishes `1.2.4-preview.125`. Prerelease and unrelated tags are ignored when selecting the base version. If no stable tag exists, preview versions start at `0.1.0-preview.<run_number>`. Major and minor releases are selected explicitly through release tags.

Before the Release build, `qetza/replacetokens-action@v1` reads the URL from an environment variable populated from `PAGE_URL` and replaces `__PAGE_URL__` in `OnlineProviderOptions.cs`. The workflow stops if the secret is empty or exactly one token is not replaced. It then builds, packs, and publishes the package without committing the injected URL to the repository.

This keeps the URL out of public source code, but the URL is embedded in the published DLL and can be extracted from it. Consumers can override the embedded default through `PassportCertificateStore:PageUrl`.

## License

This project is licensed under the [MIT License](LICENSE.txt).

## Contributing

Contributions are welcome! Please open issues or pull requests with improvements.

## Authors

[W4TR1X](https://github.com/W4TR1X)  
[batuhanoztrk](https://github.com/batuhanoztrk)
