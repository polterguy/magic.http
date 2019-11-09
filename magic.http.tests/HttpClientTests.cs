/*
 * Magic, Copyright(c) Thomas Hansen 2019, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Newtonsoft.Json.Linq;
using magic.http.services;
using magic.http.contracts;

namespace magic.http.tests
{
    public class HttpClientTests
    {
        #region [ -- Unit tests -- ]

        /*
         * Checks that we can return a simple string from an HTTP GET request.
         */
        [Fact]
        public async Task GetString()
        {
            var kernel = Initialize();
            var client = kernel.GetService(typeof(IHttpClient)) as IHttpClient;
            var result = await client.GetAsync<string>("https://my-json-server.typicode.com/typicode/demo/posts");
            Assert.NotNull(result);
            Assert.NotNull(result?.Content);
        }

        /*
         * Checks that we can return a JArray from a simple HTTP GET request.
         */
        [Fact]
        public async Task GetJArray()
        {
            var kernel = Initialize();
            var client = kernel.GetService(typeof(IHttpClient)) as IHttpClient;
            var result = await client.GetAsync<JArray>("https://my-json-server.typicode.com/typicode/demo/posts");
            Assert.NotNull(result);
            Assert.Equal(3, result.Content.Count);
        }

        /*
         * Verifies that we can return an array of objects from an HTTP GET request.
         */
        [Fact]
        public async Task GetArrayOfObjects()
        {
            var kernel = Initialize();
            var client = kernel.GetService(typeof(IHttpClient)) as IHttpClient;
            var result = await client.GetAsync<Blog[]>(
                "https://my-json-server.typicode.com/typicode/demo/posts",
                new Dictionary<string, string> { { "Accept", "application/json" } });
            Assert.NotNull(result);
            Assert.Equal(3, result.Content.Length);
        }

        /*
         * Verifies that we can return an array of objects from an HTTP GET request.
         */
        [Fact]
        public async Task GetWithToken()
        {
            var kernel = Initialize();
            var client = kernel.GetService(typeof(IHttpClient)) as IHttpClient;
            var result = await client.GetAsync<Blog[]>(
                "https://my-json-server.typicode.com/typicode/demo/posts",
                "foo");
            Assert.NotNull(result);
            Assert.Equal(3, result.Content.Length);
        }

        /*
         * Verifies that we can return an array of objects from an HTTP GET request.
         */
        [Fact]
        public async Task GetWithNullToken_Throws()
        {
            var kernel = Initialize();
            var client = kernel.GetService(typeof(IHttpClient)) as IHttpClient;
            string token = null;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => {
                await client.GetAsync<Blog[]>(
                    "https://my-json-server.typicode.com/typicode/demo/posts",
                    token /* Throws */);
                });
        }

        /*
         * Verifies that we can return an IEnumerable of objects from an HTTP GET request.
         */
        [Fact]
        public async Task GetEnumerable()
        {
            var kernel = Initialize();
            var client = kernel.GetService(typeof(IHttpClient)) as IHttpClient;
            var result = await client.GetAsync<IEnumerable<Blog>>("https://my-json-server.typicode.com/typicode/demo/posts");
            Assert.NotNull(result);
            Assert.Equal(3, result.Content.Count());
        }

        /*
         * Verifies that we can send and return a typed object using HTTP POST
         * as both request payload and response payload.
         */
        [Fact]
        public async Task PostObject()
        {
            var kernel = Initialize();
            var client = kernel.GetService(typeof(IHttpClient)) as IHttpClient;
            var user = new User
            {
                Name = "John Doe"
            };
            var result = await client.PostAsync<User, UserWithId>(
                "https://my-json-server.typicode.com/typicode/demo/posts",
                user);
            Assert.Equal("John Doe", result.Content.Name);
            Assert.True(result.Content.Id > 0);
        }

        /*
         * Verifies that we can return the HTTP response stream directly using
         * a lambda callback for HTTP GET requests.
         */
        [Fact]
        public async Task GetStream()
        {
            var kernel = Initialize();
            var client = kernel.GetService(typeof(IHttpClient)) as IHttpClient;
            Blog[] blogs = null;
            await client.GetAsync(
                "https://my-json-server.typicode.com/typicode/demo/posts",
                (stream, status, headers) =>
                {
                    using (var memory = new MemoryStream())
                    {
                        stream.CopyTo(memory);
                        memory.Position = 0;
                        using (var reader = new StreamReader(memory))
                        {
                            var stringContent = reader.ReadToEnd();
                            blogs = JArray.Parse(stringContent).ToObject<Blog[]>();
                        }
                    }
                });
            Assert.Equal(3, blogs.Length);
        }

        /*
         * Verifies that we can send a stream directly using HTTP POST.
         */
        [Fact]
        public async Task PostStream()
        {
            var kernel = Initialize();
            var client = kernel.GetService(typeof(IHttpClient)) as IHttpClient;
            using (var stream = new MemoryStream())
            {
                var jObject = JObject.FromObject(new User
                {
                    Name = "John Doe"
                });
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write (jObject);
                    writer.Flush();
                    stream.Position = 0;
                    var result = await client.PostAsync<Stream, UserWithId>(
                        "https://my-json-server.typicode.com/typicode/demo/posts",
                        stream);
                    Assert.Equal("John Doe", result.Content.Name);
                    Assert.True(result.Content.Id > 0);
                }
            }
        }

        #endregion

        #region [ -- Private helper methods and types -- ]

        class Blog
        {
            public int Id { get; set; }

            public string Title { get; set; }
        }

        class User
        {
            public string Name { get; set; }
        }

        class UserWithId
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        IServiceProvider Initialize()
        {
            var configuration = new ConfigurationBuilder().Build();
            var services = new ServiceCollection();
            services.AddTransient<IConfiguration>((svc) => configuration);
            services.AddTransient<IHttpClient, HttpClient>();
            var provider = services.BuildServiceProvider();
            return provider;
        }

        #endregion
    }
}
