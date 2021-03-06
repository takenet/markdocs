﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Takenet.MarkDocs
{
    public class MarkDocsProvider
    {
        static readonly MarkDocsSection markDocsSettings = ConfigurationManager.GetSection("markdocs") as MarkDocsSection;

        public NodeElement Root => Settings.Items.Single();

        protected virtual MarkDocsSection Settings => markDocsSettings;

        protected virtual HttpClient GetWebClient() => new HttpClient();
        
        public async Task<string> GetDocumentAsync(string folder, string document)
        {
            const string errorMessage = "Could not find the requested document!";

            var sourceUrl = BaseUrlForRawFiles(folder);

            if (sourceUrl == null)
                return errorMessage;

            var node = Flatten().SingleOrDefault(n => n.TargetFolder == folder);

            if (node == null)
                return errorMessage;

            document = await GetFileNameAsync(node, document).ConfigureAwait(false);
            if (document == null)
                return errorMessage;

            var resourceUri = $"{sourceUrl}/{document}";
            using (var webClient = GetWebClient())
            {
                var httpResponse = await webClient.GetAsync(resourceUri).ConfigureAwait(false);
                if (httpResponse.StatusCode == HttpStatusCode.NotFound && !string.IsNullOrWhiteSpace(Settings.DefaultLanguage))
                {
                    sourceUrl = BaseUrlForRawFiles(folder, Settings.DefaultLanguage);
                    resourceUri = $"{sourceUrl}/{document}";
                    httpResponse = await webClient.GetAsync(resourceUri).ConfigureAwait(false);
                }

                httpResponse.EnsureSuccessStatusCode();
                return await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        private IEnumerable<NodeElement> Flatten()
        {
            var nodes = new Stack<INode>(new[] { Settings });
            while (nodes.Any())
            {
                var node = nodes.Pop();
                var nodeElement = node as NodeElement;
                if (nodeElement != null)
                    yield return nodeElement;
                foreach (var n in node.Items) nodes.Push(n);
            }
        }

        private string BaseUrlForRawFiles(string nodeId, string language = null)
        {
            var allNodes = Flatten();
            var node = allNodes.SingleOrDefault(n => n.TargetFolder == nodeId);
            if (node == null)
                return null;

            var localizationPathPart = language;
            if (localizationPathPart == null)
            {
                localizationPathPart = node.Localized ? $"{CultureInfo.CurrentUICulture.TwoLetterISOLanguageName}" : string.Empty;
            }
            var result = $"https://raw.githubusercontent.com/{node.Owner}/{node.Repo}/{node.Branch}/{node.SourceFolder}/{localizationPathPart}";

            return result;
        }
        
        private async Task<string> GetFileNameAsync(NodeElement node, string document)
        {
            var fileNames = await GetFileNamesAsync(node).ConfigureAwait(false);
            var fileName = fileNames.SingleOrDefault(fn => fn.EndsWith($"-{document}.md"));
            return fileName;
        }

        private async Task<IEnumerable<string>> GetFileNamesAsync(NodeElement node)
        {
            var urls = await GetUrlsFromChildItemsAsync(node).ConfigureAwait(false);
            var docs = urls.Single(u => u.Key == node.SourceFolder).Value;
            string cultureCode;
            if (node.Localized)
            {
                cultureCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                urls = await GetUrlsFromChildItemsAsync(docs, node.Username, node.Password).ConfigureAwait(false);
                docs = urls.SingleOrDefault(u => u.Key == cultureCode).Value ??
                       urls.SingleOrDefault(u => u.Key == Settings.DefaultLanguage).Value;
                if (docs == null)
                    return Enumerable.Empty<string>();
            }

            var fileNames = await GetChildItemsFileNamesAsync(docs, node.Username, node.Password).ConfigureAwait(false);

            return fileNames;
        }

        internal async Task<IDictionary<string, string>> GetUrlsFromChildItemsAsync(NodeElement item)
        {
            var parentUrl = $"https://api.github.com/repos/{item.Owner}/{item.Repo}/git/trees/{item.Branch}";
            return await GetUrlsFromChildItemsAsync(parentUrl, item.Username, item.Password).ConfigureAwait(false);
        }

        internal async Task<IDictionary<string, string>> GetUrlsFromChildItemsAsync(string parentUrl, string username, string password)
        {
            var urls = new Dictionary<string, string>();
            var data = await GetStringAsync(parentUrl, username, password).ConfigureAwait(false);
            var json = JObject.Parse(data);
            var tree = json["tree"] as JArray;
            foreach (var url in tree)
            {
                urls[url["path"].Value<string>()] = url["url"].Value<string>();
            }
            return urls;
        }

        internal async Task<IEnumerable<string>> GetChildItemsFileNamesAsync(string parentUrl, string username, string password)
        {
            var result = new List<string>();
            var data = await GetStringAsync(parentUrl, username, password).ConfigureAwait(false);
            var json = JObject.Parse(data);
            var tree = json["tree"] as JArray;
            result.AddRange(tree.Select(item => item["path"].Value<string>()));
            return result;
        }

        private async Task<string> GetStringAsync(string url, string username, string password)
        {
            using (var webClient = GetWebClient())
            {
                var uri = new Uri(url, UriKind.Absolute);
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    request.Headers.UserAgent.Add(new ProductInfoHeaderValue(username, ""));
                    var authentication = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authentication);

                    var response = await webClient.SendAsync(request).ConfigureAwait(false);

                    var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    return result;
                }
            }
        }
    }
}
