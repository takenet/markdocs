using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Takenet.MarkDocs
{
    public class DynamicNodeProvider
    {
        public DynamicNodeProvider(MarkDocsProvider markDocs)
        {
            MarkDocs = markDocs;
        }

        private MarkDocsProvider MarkDocs { get; }

        public IEnumerable<DynamicNode> GetDynamicNodeCollection()
        {
            if (MarkDocs == null) throw new ArgumentNullException(nameof(MarkDocs));
            if (MarkDocs.Root == null) throw new ArgumentNullException(nameof(MarkDocs.Root));

            return CreateSiteMapNodesAsync(MarkDocs.Root).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private async Task<IEnumerable<DynamicNode>> CreateSiteMapNodesAsync(NodeElement item, string parentKey = null)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (MarkDocs == null) throw new ArgumentNullException(nameof(MarkDocs));
            if (MarkDocs.CultureInfo == null) throw new ArgumentNullException(nameof(MarkDocs.CultureInfo));

            var result = new List<DynamicNode>();

            var parentSiteMapNode = new DynamicNode
            {
                Key = Guid.NewGuid().ToString(),
                ParentKey = parentKey,
                Title = DisplayFor(item.Display, item.Localized),
                Action = $"{item.TargetFolder}"
            };
            result.Add(parentSiteMapNode);

            // Determine remote folder to load the document files from
            var urls = await MarkDocs.GetUrlsFromChildItemsAsync(item).ConfigureAwait(false);
            var docs = urls.Single(u => u.Key == item.SourceFolder).Value;
            if (item.Localized)
            {
                var cultureCode = MarkDocs.CultureInfo.TwoLetterISOLanguageName;
                urls = await MarkDocs.GetUrlsFromChildItemsAsync(docs, item.Username, item.Password).ConfigureAwait(false);
                docs = urls.Single(u => u.Key == cultureCode).Value;
            }

            // Get remote documents and generate siteMapItems for them
            var files = await MarkDocs.GetChildItemsFileNamesAsync(docs, item.Username, item.Password).ConfigureAwait(false);
            var siteMapNodes = CreateSiteMapNodes(item.Localized, item.TargetFolder, files);
            foreach (var siteMapNode in siteMapNodes)
            {
                siteMapNode.ParentKey = parentSiteMapNode.Key;
                result.Add(siteMapNode);
            }

            // Do the same as above, recursively, for each child NodeElement
            foreach (var child in item.Items)
            {
                var childItemSiteMapNodes = await CreateSiteMapNodesAsync(child, parentSiteMapNode.Key).ConfigureAwait(false);
                result.AddRange(childItemSiteMapNodes);
            }
            return result;
        }

        private IEnumerable<DynamicNode> CreateSiteMapNodes(bool isLocalized, string folder, IEnumerable<string> documents)
        {
            if (documents == null) throw new ArgumentNullException(nameof(documents));
            if (folder == null) throw new ArgumentNullException(nameof(folder));

            return documents.Select(document =>
            {
                document = document.Split('-', '.').Skip(1).First();
                return new DynamicNode
                {
                    Key = Guid.NewGuid().ToString(),
                    Title = DisplayFor(document, isLocalized),
                    Action = $"{folder}/{document}"
                };
            });
        }

        private string DisplayFor(string document, bool isLocalized)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            return isLocalized ? (string)HttpContext.GetGlobalResourceObject("SiteMapResources", document, MarkDocs.CultureInfo) ?? document : document;
        }
    }
}