# Takenet.MarkDocs

![TC](https://take-teamcity1.azurewebsites.net/app/rest/builds/buildType:(id:MarkDocs_Master)/statusIcon) [![NuGet](https://img.shields.io/nuget/dt/Takenet.MarkDocs.svg?style=flat-square)](https://www.nuget.org/packages/Takenet.MarkDocs) [![NuGet](https://img.shields.io/nuget/v/Takenet.MarkDocs.svg?style=flat-square)](https://www.nuget.org/packages/Takenet.MarkDocs)

> Takenet.MarkDocs is a ASP.NET MVC library that provides markdown documentation.

## How to use

 - Change the `markdocs` section in `Web.Config` with GitHub project documentation sources.

>  * *You can generate your access token on [this link](https://github.com/settings/tokens)*
>  * *An optional default language could be defined on top level element* `<markdocs defaultLanguage="en">`

The `markdocs` section will be created during the NuGet package installation.

 - Create/Change your `mvc.sitemap` node to host the documentation links

```xml
<mvcSiteMapNode title="Documentation" controller="Docs" action="Show"
                dynamicNodeProvider="YourNamespace.DocsDynamicNodeProvider, YourAssembly">
</mvcSiteMapNode>
```

The type `YourNamespace.DocsDynamicNodeProvider` referenced above will be created during the NuGet package installation.

### Dependency Injection

You need to configure your DI to inject the `MarkDocsProvider` on your `DocsController` and `DocsDynamicNodeProvider`. This example uses [SimpleInjector](https://github.com/simpleinjector/SimpleInjector):

```csharp
container.RegisterSingleton<MarkDocsProvider>();
```

### Docs Controller

The NuGet package installer will create a `DocsController` with a `Show` action to load your markdown documents and display them in a view.

### Docs Views

You need to create a view inside the folder `Views/Docs` named `Show.cshtml` to display your markdown document.

## Folder and file names

On the GitHub project that will host your documentation files, create a `docs` folder in the root folder.

If you want to localize your docs, create a folder for each culture.
At the moment, only TweLetterCultureCodes are supported as culture options.
Name your folder accordingly. Ex:

```yaml
- docs
    - en
        - 1-index.md
        - 2-about.md
    - pt
        - 1-index.md
        - 2-about.md
```

### File name sorting

You need to prefix your markdown files with a number and a dash. The number will be used to sort the documentation files and the equivalent SiteMap items.

## SiteMap Localization

The SiteMap itens can be localized if your enable localization in the `Web.Config` file.
The localization resources must match the part of the file name discarding the sort index and the extension and should be available in your `AppGlobal_Resources`.

## License

Apache License 2.0
