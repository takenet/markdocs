# Takenet.MarkDocs

Takenet.MarkDocs is a ASP.NET MVC library that provides markdown documentation.

![TC](https://take-teamcity1.azurewebsites.net/app/rest/builds/buildType:(id:MarkDocs_Master)/statusIcon)

<a href="https://www.nuget.org/packages/Takenet.MarkDocs" rel="NuGet">![NuGet](https://img.shields.io/nuget/dt/Takenet.MarkDocs.svg)</a>

<a href="https://www.nuget.org/packages/Takenet.MarkDocs" rel="NuGet">![NuGet](https://img.shields.io/nuget/v/Takenet.MarkDocs.svg)</a>

## Dependencies

- .NET Framework 4.5.2
- ASP.NET MVC 5
- MVC SiteMap
- Newtonsoft.Json

## Configuring the Project

### Changes to Web.Config

Register the session `markdocs` in the `Web.Config` file, like so:

```
<configuration>
  <configSections>
    <section name="markdocs" type="Takenet.MarkDocs.MarkDocsSection, Takenet.MarkDocs, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null" requirePermission="false" />
  </configSections>
```

Register the GitHub project documentation sources

```
  <markdocs>
    <node display="documentation"
          targetFolder="home"
          username="yourgithubuser"
          password="yourpersonalaccesstoken"
          owner="takenet" 
          repo="markdocs" 
          branch="master" 
          sourceFolder="docs" 
          localized="true">
      <node display="sdks"
            targetFolder="sdks" 
            username="yourgithubuser"
            password="yourpersonalaccesstoken"
            owner="takenet" 
            repo="markdocs-sdks" 
            branch="master" 
            sourceFolder="docs" 
            localized="true" />
    </node>
  </markdocs>
```

### Changes to mvc.sitemap

Create a SiteMap node to host the documentation links

```
<mvcSiteMapNode title="Documentation" controller="Docs" action="Root/Introduction" fa-icon="wrench"
                dynamicNodeProvider="YourNamespace.DocsDynamicNodeProvider, YourAssembly">
</mvcSiteMapNode>
```

### DocsDynamicNodeProvider

Create a `DocsDynamicNodeProvider` reading the `Takenet.MarkDocs.DynamicNodeProvider` in your project

```
using System.Collections.Generic;
using MvcSiteMapProvider;
using Takenet.MarkDocs;

namespace YourNamespace
{
    public class DocsDynamicNodeProvider : DynamicNodeProviderBase
    {
        private Takenet.MarkDocs.DynamicNodeProvider DynamicNodeProvider { get; }

        public DocsDynamicNodeProvider(Takenet.MarkDocs.DynamicNodeProvider dynamicNodeProvider) 
        {
            DynamicNodeProvider = dynamicNodeProvider;
        }

        public override IEnumerable<DynamicNode> GetDynamicNodeCollection(ISiteMapNode node)
        {
            return DynamicNodeProvider.GetDynamicNodeCollection().Select(n => new DynamicNode
            {
                Key = n.Key,
                ParentKey = n.ParentKey,
                Title = n.Title,
                Action = n.Action
            });
        }
    }
}
```

### Dependency Injection

You need to configure your DI to inject the `MarkDocsProvider` on your controllers and on your `DynamicNodeProvider`

Make sure the instantiation is by web request, otherwise cached values will be reused for different users

```
// DI configuration for Simple injector
container.RegisterPerWebRequest<MarkDocsProvider>();
```

### DocsController

Create a ASP.NET MVC Controller to handle the documentation requests

```
namespace YourNamespace.Controllers
{
    public partial class DocsController : Controller
    {
        // You need to configure your DI to inject the MarkDocsProvider on your controllers
        public DocsController(MarkDocsProvider markDocs)
        {
            MarkDocs = markDocs;
        }

        private MarkDocsProvider MarkDocs { get; }

        public virtual async Task<ActionResult> Show(string folder, string document)
        {
            // Load the markdown document
            var markdown = await MarkDocs.GetDocumentAsync(folder, document);

            // Return the view with the markdown document to be loaded
            return View(markdown);
        }
    }
}
```

### Docs Views

You need to create a view inside the folder `Views/Docs` named `Show.cshtml` to display your markdown document

## Markdown File Structure

### Folder and file names

On the GitHub project that will host your documentation filed, create a `docs` folder in the root folder.

If you want to localize your docs, create a folder for each culture.
At the moment only TweLetterCultureCodes are supported as culture options.
Name your folder accordingly. Ex:

```
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
The localization resources must match the part of the file name discarding the sort index and the extension and should be available in your AppGlobal_Resources.

## License

Apache License 2.0
