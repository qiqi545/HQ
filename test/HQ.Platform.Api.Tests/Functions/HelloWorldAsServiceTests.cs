using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HQ.Platform.Api.Functions.Azure;
using HQ.Test.Sdk;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace HQ.Platform.Api.Tests.Functions
{
    public class HelloWorldAsServiceTests : ServiceUnderTest
    {
        [Test]
        public async Task With_query_string_and_body()
        {
            var query = new Dictionary<string, StringValues>();
            query.TryAdd("name", "HQ");
            const string body = "";

            var functions = new HelloWorld();
            var result = await functions.Run(CreateHttpRequest(query, body), this);
            var resultObject = (OkObjectResult)result;
            Assert.Equal("Hello, HQ", resultObject.Value);
        }

        [Test]
        public async Task With_body()
        {
            var query = new Dictionary<string, StringValues>();
            const string body = "{\"name\":\"HQ\"}";

            var functions = new HelloWorld();
            var result = await functions.Run(CreateHttpRequest(query, body), this);
            var resultObject = (OkObjectResult)result;
            Assert.Equal("Hello, HQ", resultObject.Value);
        }

        [Test]
        public async Task With_no_query_and_body()
        {
            var query = new Dictionary<string, StringValues>();
            const string body = "";

            var functions = new HelloWorld();
            var result = await functions.Run(CreateHttpRequest(query, body), this);
            var resultObject = (BadRequestObjectResult)result;
            Assert.Equal("Please pass a name on the query string or in the request body", resultObject.Value);
        }
        
        public static DefaultHttpRequest CreateHttpRequest(Dictionary<string, StringValues> query, string body)
        {
            var context = new DefaultHttpContext();
            var request = new DefaultHttpRequest(context)
            {
                Query = new QueryCollection(query),
                Body = new MemoryStream(Encoding.UTF8.GetBytes(body))
            };
            return request;
        }
    }
}
