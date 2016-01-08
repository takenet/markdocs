﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;

namespace Takenet.MarkDocs
{
    public class MarkDocsProvider
    {
        private class MarkDocsCache
        {
            public MarkDocsCache(ObjectCache objectCache)
            {
                ObjectCache = objectCache;
            }

            private ObjectCache ObjectCache { get; }

            internal bool TryGetCachedValue(string key, out object value, [CallerMemberName] string group = null)
            {
                key = $"{group}-{key}";
                if (!ObjectCache.Contains(key))
                {
                    value = null;
                    return false;
                }
                value = ObjectCache.Get(key);
                return true;
            }

            internal void SetCachedValue(string key, object value, [CallerMemberName] string group = null)
            {
                key = $"{group}-{key}";
                ObjectCache.Set(key, value, DateTimeOffset.Now.AddDays(1));
            }
        }

        private static readonly MarkDocsSection MarkDocsSettings = ConfigurationManager.GetSection("markdocs") as MarkDocsSection;

        public MarkDocsProvider(ObjectCache cache)
        {
            Cache = new MarkDocsCache(cache);
        }

        private readonly Logger Log = LogManager.GetCurrentClassLogger();

        private void WriteCurrentCultureInfoToLog([CallerMemberName] string methodName = null)
        {
            Log.Debug($"@{methodName}: CultureInfo.CurrentUICulture = {CultureInfo.CurrentUICulture}");
        }

        private static Assembly GetWebEntryAssembly()
        {
            if (Assembly.GetEntryAssembly() != null)
                return Assembly.GetEntryAssembly();

            if (System.Web.HttpContext.Current == null ||
                System.Web.HttpContext.Current.ApplicationInstance == null)
                return null;

            var type = System.Web.HttpContext.Current.ApplicationInstance.GetType();
            while (type != null && type.Namespace == "ASP")
            {
                type = type.BaseType;
            }

            return type?.Assembly;
        }

        private MarkDocsCache Cache { get; }

        public NodeElement Root => MarkDocsSettings.Items.Single();

        private IEnumerable<NodeElement> Flatten()
        {
            var nodes = new Stack<INode>(new[] { MarkDocsSettings });
            while (nodes.Any())
            {
                var node = nodes.Pop();
                var nodeElement = node as NodeElement;
                if (nodeElement != null)
                    yield return nodeElement;
                foreach (var n in node.Items) nodes.Push(n);
            }
        }

        private string BaseUrlForRawFiles(string nodeId)
        {
            WriteCurrentCultureInfoToLog();

            object value;
            if (Cache.TryGetCachedValue(nodeId, out value))
                return value as string;

            var allNodes = Flatten();
            var node = allNodes.SingleOrDefault(n => n.TargetFolder == nodeId);
            if (node == null)
                return null;

            var localizationPathPart = node.Localized ? $"{CultureInfo.CurrentUICulture.TwoLetterISOLanguageName}" : string.Empty;
            var result = $"https://raw.githubusercontent.com/{node.Owner}/{node.Repo}/{node.Branch}/{node.SourceFolder}/{localizationPathPart}";

            Cache.SetCachedValue(nodeId, result);

            return result;
        }

        public async Task<string> GetDocumentAsync(string folder, string document)
        {
            WriteCurrentCultureInfoToLog();

            var key = $"{folder}.{document}";
            object value;
            if (Cache.TryGetCachedValue(key, out value))
                return value as string;

            const string errorMessage = "Could not find the requested document!";

            var sourceUrl = BaseUrlForRawFiles(folder);

            if (sourceUrl == null)
                return errorMessage;

            var node = Flatten().SingleOrDefault(n => n.TargetFolder == folder);

            if (node == null)
                return errorMessage;

            document = await GetFileNameAsync(node, document);
            var resourceUri = $"{sourceUrl}/{document}";
            using (var webClient = new HttpClient())
            {
                var result = await webClient.GetStringAsync(resourceUri);

                Cache.SetCachedValue(key, result);

                return result;
            }
        }

        private async Task<string> GetFileNameAsync(NodeElement node, string document)
        {
            WriteCurrentCultureInfoToLog();

            var fileNames = await GetFileNamesAsync(node).ConfigureAwait(false); ;
            var fileName = fileNames.Single(fn => fn.EndsWith($"-{document}.md"));
            return fileName;
        }

        private async Task<IEnumerable<string>> GetFileNamesAsync(NodeElement node)
        {
            WriteCurrentCultureInfoToLog();

            var urls = await GetUrlsFromChildItemsAsync(node).ConfigureAwait(false); ;
            var docs = urls.Single(u => u.Key == node.SourceFolder).Value;
            var cultureCode = string.Empty;
            if (node.Localized)
            {
                cultureCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                urls = await GetUrlsFromChildItemsAsync(docs, node.Username, node.Password).ConfigureAwait(false);
                docs = urls.Single(u => u.Key == cultureCode).Value;
            }

            var key = $"{node.TargetFolder}-{cultureCode}";
            object value;
            if (Cache.TryGetCachedValue(key, out value))
                return value as IEnumerable<string>;

            var fileNames = await GetChildItemsFileNamesAsync(docs, node.Username, node.Password);

            Cache.SetCachedValue(key, fileNames);

            return fileNames;
        }

        internal async Task<IDictionary<string, string>> GetUrlsFromChildItemsAsync(NodeElement item)
        {
            WriteCurrentCultureInfoToLog();

            var parentUrl = $"https://api.github.com/repos/{item.Owner}/{item.Repo}/git/trees/{item.Branch}";
            return await GetUrlsFromChildItemsAsync(parentUrl, item.Username, item.Password).ConfigureAwait(false);
        }

        internal async Task<IDictionary<string, string>> GetUrlsFromChildItemsAsync(string parentUrl, string username, string password)
        {
            WriteCurrentCultureInfoToLog();

            object value;
            if (Cache.TryGetCachedValue(parentUrl, out value))
                return value as IDictionary<string, string>;

            var urls = new Dictionary<string, string>();
            var data = await GetStringAsync(parentUrl, username, password).ConfigureAwait(false);
            var json = JObject.Parse(data);
            var tree = json["tree"] as JArray;
            foreach (var url in tree)
            {
                urls[url["path"].Value<string>()] = url["url"].Value<string>();
            }

            Cache.SetCachedValue(parentUrl, urls);

            return urls;
        }

        internal async Task<IEnumerable<string>> GetChildItemsFileNamesAsync(string parentUrl, string username, string password)
        {
            WriteCurrentCultureInfoToLog();

            object value;
            if (Cache.TryGetCachedValue(parentUrl, out value))
                return value as IEnumerable<string>;

            var result = new List<string>();
            var data = await GetStringAsync(parentUrl, username, password).ConfigureAwait(false);
            var json = JObject.Parse(data);
            var tree = json["tree"] as JArray;
            result.AddRange(tree.Select(item => item["path"].Value<string>()));

            Cache.SetCachedValue(parentUrl, result);

            return result;
        }

        private async Task<string> GetStringAsync(string url, string username, string password)
        {
            WriteCurrentCultureInfoToLog();

            object value;
            if (Cache.TryGetCachedValue(url, out value))
                return value as string;

            using (var webClient = new HttpClient())
            {
                var uri = new Uri(url, UriKind.Absolute);
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.UserAgent.Add(new ProductInfoHeaderValue(username, ""));
                var authentication = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authentication);

                var response = await webClient.SendAsync(request).ConfigureAwait(false);

                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                Cache.SetCachedValue(url, result);

                return result;
            }
        }
    }
}
