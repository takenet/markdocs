using MvcSiteMapProvider;
using System.Collections.Generic;
using System.Linq;

namespace $rootnamespace$
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
