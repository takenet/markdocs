using NSubstitute;
using System.Net.Http;

namespace Takenet.MarkDocs.Tests
{
    class TestingMarkDocsProvider : MarkDocsProvider
    {
        public MarkDocsSection MarkDocsSettings { get; }
        public HttpMessageHandler HttpMessageHandler { get; }

        public TestingMarkDocsProvider(MarkDocsSection settings = null, HttpMessageHandler handler = null)
        {
            MarkDocsSettings = settings ?? new MarkDocsSection();
            HttpMessageHandler = handler ?? Substitute.For<HttpMessageHandler>();
        }

        protected override MarkDocsSection GetSettings() => MarkDocsSettings;

        protected override HttpClient GetWebClient() => new HttpClient(HttpMessageHandler);
    }
}
