using ContainerNinja.Contracts.Services;
using ContainerNinja.Contracts.Walmart;
using ContainerNinja.Core.Walmart;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System.Collections.Specialized;

namespace ContainerNinja.Core.Services
{
    public class WalmartService : IWalmartService
    {
        private readonly string _walmartId;
        private readonly string _keyVersion;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public WalmartService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _walmartId = Environment.GetEnvironmentVariable("WalmartServiceId");
            _keyVersion = Environment.GetEnvironmentVariable("WalmartServiceIdVersion");
        }

        public async Task<Search> Search(string query)
        {
            var searchRequest = new SearchRequest
            {
                query = query
            };

            using (var client = new WalmartWebClient())
            {
                var t1 = client.GetTimestampInJavaMillis();
                var requiredHeaders = new NameValueCollection
                {
                    { "WM_CONSUMER.ID", _walmartId },
                    { "WM_CONSUMER.INTIMESTAMP", t1 },
                    { "WM_SEC.KEY_VERSION", _keyVersion },
                };
                client.Headers.Add(requiredHeaders);
                client.Headers.Add("WM_SEC.AUTH_SIGNATURE", client.GetWalmartSignature(requiredHeaders[0], requiredHeaders[1], requiredHeaders[2]));

                client.BaseAddress = "https://developer.api.walmart.com";
                var jsonResponse = await client.DownloadStringTaskAsync(string.Format("/api-proxy/service/affil/product/v2/search?query={0}", query));
                return JsonConvert.DeserializeObject<Search>(jsonResponse);
            }
        }

        public async Task<Item> GetItem(string id)
        {
            var itemRequest = new ItemRequest
            {
                id = id
            };

            using (var client = new WalmartWebClient())
            {
                var t1 = client.GetTimestampInJavaMillis();
                var requiredHeaders = new NameValueCollection
                {
                    { "WM_CONSUMER.ID", _walmartId },
                    { "WM_CONSUMER.INTIMESTAMP", t1 },
                    { "WM_SEC.KEY_VERSION", _keyVersion },
                };
                client.Headers.Add(requiredHeaders);
                client.Headers.Add("WM_SEC.AUTH_SIGNATURE", client.GetWalmartSignature(requiredHeaders[0], requiredHeaders[1], requiredHeaders[2]));

                client.BaseAddress = "https://developer.api.walmart.com";
                var jsonResponse = await client.DownloadStringTaskAsync(string.Format("/api-proxy/service/affil/product/v2/items/{0}", id));
                return JsonConvert.DeserializeObject<Item>(jsonResponse);
            }
        }

        public async Task<Item> GetItem(long? id)
        {
            return await GetItem(id.ToString());
        }

        public async Task<MultipleItems> GetMultipleItems(string[] ids)
        {
            if (ids.Length > 10)
            {
                throw new ArgumentException("Ids length must be less than 10", nameof(ids));
            }
            var multipleItemsRequest = new MultipleItemsRequest
            {
                ids = string.Join(',', ids)
            };
            using (var client = new WalmartWebClient())
            {
                var t1 = client.GetTimestampInJavaMillis();
                var requiredHeaders = new NameValueCollection
                {
                    { "WM_CONSUMER.ID", _walmartId },
                    { "WM_CONSUMER.INTIMESTAMP", t1 },
                    { "WM_SEC.KEY_VERSION", _keyVersion },
                };
                client.Headers.Add(requiredHeaders);
                client.Headers.Add("WM_SEC.AUTH_SIGNATURE", client.GetWalmartSignature(requiredHeaders[0], requiredHeaders[1], requiredHeaders[2]));

                client.BaseAddress = "https://developer.api.walmart.com";
                var jsonResponse = await client.DownloadStringTaskAsync(string.Format("/api-proxy/service/affil/product/v2/items?ids{0}", string.Join(',', ids)));
                return JsonConvert.DeserializeObject<MultipleItems>(jsonResponse);
            }
        }
    }
}
