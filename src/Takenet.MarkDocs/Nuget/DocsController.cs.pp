using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Takenet.MarkDocs;
using System.Web.Mvc;

namespace $rootnamespace$
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
