using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WorldDomination.Net.Http;

namespace Takenet.MarkDocs.Tests
{
    public abstract class TestingHttpMessageHandler : HttpMessageHandler
    {
        public abstract Task<HttpResponseMessage> OverrideSendAsync(HttpRequestMessage request, CancellationToken cancellationToken);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return OverrideSendAsync(request, cancellationToken);
        }
    }
}
