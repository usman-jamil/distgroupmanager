using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Emirates.SharePoint.ADGroupHandler
{
    class ListsClient : IDisposable
    {
        public ListsClient(Uri webUri, ICredentials credentials)
        {
            _client = new WebClient();
            _client.Credentials = credentials;
            _client.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
            _client.Headers.Add(HttpRequestHeader.ContentType, "application/json;odata=verbose");
            _client.Headers.Add(HttpRequestHeader.Accept, "application/json;odata=verbose");
            WebUri = webUri;
        }


        /// <summary>
        /// Create List Item operation
        /// </summary>
        /// <param name="listTitle"></param>
        /// <param name="payload"></param>
        public void InsertListItem(string listTitle, object payload)
        {
            var formDigestValue = RequestFormDigest();
            _client.Headers.Add("X-RequestDigest", formDigestValue);
            var endpointUri = string.Format("{0}/_api/web/lists/getbytitle('{1}')/items", WebUri.AbsoluteUri, listTitle);
            var payloadString = JsonConvert.SerializeObject(payload);
            _client.Headers.Add(HttpRequestHeader.ContentType, "application/json;odata=verbose");
            _client.UploadString(endpointUri, "POST", payloadString);
        }

        /// <summary>
        /// Request Form Digest
        /// </summary>
        /// <returns></returns>
        private string RequestFormDigest()
        {
            var endpointUri = string.Format("{0}/_api/contextinfo", WebUri.AbsoluteUri);
            var result = _client.UploadString(endpointUri, "POST");
            JToken t = JToken.Parse(result);
            return t["d"]["GetContextWebInformation"]["FormDigestValue"].ToString();
        }


        public Uri WebUri { get; private set; }

        public void Dispose()
        {
            _client.Dispose();
            GC.SuppressFinalize(this);
        }

        private readonly WebClient _client;
    }
}
