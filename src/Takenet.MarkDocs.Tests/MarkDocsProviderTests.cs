using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using WorldDomination.Net.Http;

namespace Takenet.MarkDocs.Tests
{
    [TestFixture]
    public class MarkDocsProviderTests
    {
        TestingMarkDocsProvider provider;
        TestingHttpMessageHandler messageHandler;
        MarkDocsSection settings;

        [SetUp]
        public void SetUp()
        {
            messageHandler = Substitute.ForPartsOf<TestingHttpMessageHandler>();
            settings = new MarkDocsSection { Items = new NodeCollection() };
            provider = new TestingMarkDocsProvider(settings, messageHandler);
        }

        [Test]
        public async Task WhenADocumentIsRequestThenItIsRetrievedFromGitHubRepo()
        {
            // Arrange
            var documentName = "document";
            var folderName = "folder";

            var repoFolder = "docs";
            var rootJson = $@"{{ 'tree': [
    {{
      'path': '{repoFolder}',
      'url': 'http://content.com/{repoFolder}'
    }}] 
}}";
            var docsJson = $@"{{ 'tree': [
    {{
      'path': '1-{documentName}.md',
      'url': 'http://content.com/{documentName}'
    }}] 
}}";
            var repoContent = "Test";

            settings.Items = new TestingNodeCollection
            {
                new NodeElement { TargetFolder = folderName, Localized = false, Username = "user", Password = "pwd", Repo = "reponame", Owner = "owner", Branch = "branch" }
            };
            messageHandler.OverrideSendAsync(Arg.Is<HttpRequestMessage>(r => r.RequestUri.Host == "api.github.com"), Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(FakeHttpMessageHandler.GetStringHttpResponseMessage(rootJson));
            messageHandler.OverrideSendAsync(Arg.Is<HttpRequestMessage>(r => r.RequestUri.PathAndQuery.EndsWith(repoFolder)), Arg.Any<CancellationToken>())
                .Returns(FakeHttpMessageHandler.GetStringHttpResponseMessage(docsJson));
            messageHandler.OverrideSendAsync(Arg.Is<HttpRequestMessage>(r => r.RequestUri.Host == "raw.githubusercontent.com"), Arg.Any<CancellationToken>())
                .Returns(FakeHttpMessageHandler.GetStringHttpResponseMessage(repoContent));

            // Act
            var content = await provider.GetDocumentAsync(folderName, documentName);

            // Assert
            content.ShouldBe(repoContent);
        }
    }
}
