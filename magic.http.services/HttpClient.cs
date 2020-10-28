/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * Permission to use under the terms of the LGPL license is hereby granted, see
 * the enclosed LICENSE file for details.
 */

using System;
using System.IO;
using System.Net;
using System.Text;
using net = System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using magic.http.contracts;

namespace magic.http.services
{
    /// <summary>
    /// Service implementation for IHttpClient. Uses System.Net.Http.HttpClient
    /// internally to invoke your HTTP requests.
    /// </summary>
    public sealed class HttpClient : IHttpClient, IDisposable
    {
        readonly net.HttpClient _client;

        // Default HTTP headers for an empty HTTP request.
        static readonly Dictionary<string, string> DEFAULT_HEADERS_EMPTY_REQUEST =
            new Dictionary<string, string> {
                { "Accept", "application/json" },
            };

        // Default HTTP headers for an HTTP request with a payload.
        static readonly Dictionary<string, string> DEFAULT_HEADERS_REQUEST =
            new Dictionary<string, string> {
                { "Content-Type", "application/json" },
                { "Accept", "application/json" },
            };

        /// <summary>
        /// Creates a new instance of class.
        /// </summary>
        /// <param name="factory">The factory to create the internally kept HttpClient</param>
        public HttpClient(net.IHttpClientFactory factory)
        {
            _client = factory.CreateClient("magic");
        }

        #region [ -- Interface implementation -- ]

        /// <inheritdoc />
        public async Task<Response<TOut>> PostAsync<TIn, TOut>(
            string url,
            TIn request,
            Dictionary<string, string> headers = null)
        {
            return await CreateContentRequest<TOut>(
                url,
                net.HttpMethod.Post,
                request,
                headers ?? DEFAULT_HEADERS_REQUEST);
        }

        /// <inheritdoc />
        public async Task<Response<TOut>> PostAsync<TIn, TOut>(
            string url,
            TIn request,
            string token)
        {
            return await CreateContentRequest<TOut>(
                url,
                net.HttpMethod.Post,
                request,
                GetDefaultBearerTokenHeaders(token));
        }

        /// <inheritdoc />
        public async Task<Response<TOut>> PatchAsync<TIn, TOut>(
            string url,
            TIn request,
            Dictionary<string, string> headers = null)
        {
            return await CreateContentRequest<TOut>(
                url,
                new net.HttpMethod("PATCH"),
                request,
                headers ?? DEFAULT_HEADERS_REQUEST);
        }

        /// <inheritdoc />
        public async Task<Response<TOut>> PatchAsync<TIn, TOut>(
            string url,
            TIn request,
            string token)
        {
            return await CreateContentRequest<TOut>(
                url,
                new net.HttpMethod("PATCH"),
                request,
                GetDefaultBearerTokenHeaders(token));
        }

        /// <inheritdoc />
        public async Task<Response<TOut>> PutAsync<TIn, TOut>(
            string url,
            TIn request,
            Dictionary<string, string> headers = null)
        {
            return await CreateContentRequest<TOut>(
                url,
                net.HttpMethod.Put,
                request,
                headers ?? DEFAULT_HEADERS_REQUEST);
        }

        /// <inheritdoc />
        public async Task<Response<TOut>> PutAsync<TIn, TOut>(
            string url,
            TIn request,
            string token)
        {
            return await CreateContentRequest<TOut>(
                url,
                net.HttpMethod.Put,
                request,
                GetDefaultBearerTokenHeaders(token));
        }

        /// <inheritdoc />
        public async Task<Response<TOut>> GetAsync<TOut>(
            string url,
            Dictionary<string, string> headers = null)
        {
            return await CreateEmptyRequest<TOut>(
                url,
                net.HttpMethod.Get,
                headers ?? DEFAULT_HEADERS_EMPTY_REQUEST);
        }

        /// <inheritdoc />
        public async Task<Response<TOut>> GetAsync<TOut>(
            string url,
            string token)
        {
            return await CreateEmptyRequest<TOut>(
                url,
                net.HttpMethod.Get,
                GetDefaultBearerTokenHeaders(token));
        }

        /// <inheritdoc />
        public async Task GetAsync(
            string url,
            Action<Stream, HttpStatusCode, Dictionary<string, string>> functor,
            Dictionary<string, string> headers = null)
        {
            await CreateEmptyRequestStreamResponse(
                url,
                net.HttpMethod.Get,
                functor,
                headers ?? DEFAULT_HEADERS_EMPTY_REQUEST);
        }

        /// <inheritdoc />
        public async Task GetAsync(
            string url,
            Action<Stream, HttpStatusCode, Dictionary<string, string>> functor,
            string token)
        {
            await CreateEmptyRequestStreamResponse(
                url,
                net.HttpMethod.Get,
                functor,
                GetDefaultBearerTokenHeaders(token));
        }

        /// <inheritdoc />
        public async Task<Response<TOut>> DeleteAsync<TOut>(
            string url,
            Dictionary<string, string> headers = null)
        {
            return await CreateEmptyRequest<TOut>(
                url,
                net.HttpMethod.Delete,
                headers ?? DEFAULT_HEADERS_EMPTY_REQUEST);
        }

        /// <inheritdoc />
        public async Task<Response<TOut>> DeleteAsync<TOut>(
            string url,
            string token)
        {
            return await CreateEmptyRequest<TOut>(
                url,
                net.HttpMethod.Delete,
                GetDefaultBearerTokenHeaders(token));
        }

        /// <summary>
        /// Necessary to dispose HttpClient used internally
        /// </summary>
        public void Dispose()
        {
            _client.Dispose();
        }

        #endregion

        #region [ -- Private helper methods -- ]

        /* Responsible for creating an HTTP request of specified type. Only used
         * during GET and DELETE requests, since you cannot apply a payload to
         * your request.
         */
        async Task<Response<TOut>> CreateEmptyRequest<TOut>(
            string url,
            net.HttpMethod method,
            Dictionary<string, string> headers)
        {
            using (var msg = CreateRequestMessage(method, url, headers))
            {
                return await GetResult<TOut>(msg);
            }
        }

        /* Responsible for creating a request of the specified type. Used
         * only during POST and PUT since it requires a payload to be provided.
         */
        async Task<Response<TOut>> CreateContentRequest<TOut>(
            string url,
            net.HttpMethod method,
            object input,
            Dictionary<string, string> headers)
        {
            using (var msg = CreateRequestMessage(method, url, headers))
            {
                if (input is Stream stream)
                {
                    using (var content = new net.StreamContent(stream))
                    {
                        AddContentHeaders(content, headers);
                        msg.Content = content;
                        return await GetResult<TOut>(msg);
                    }
                }
                else if (input is byte[] bytes)
                {
                    using (var content = new net.ByteArrayContent(bytes))
                    {
                        AddContentHeaders(content, headers);
                        msg.Content = content;
                        return await GetResult<TOut>(msg);
                    }
                }

                var stringContent = input is string strInput ?
                    strInput :
                    JObject.FromObject(input).ToString();

                using (var content = new net.StringContent(stringContent))
                {
                    AddContentHeaders(content, headers);
                    msg.Content = content;
                    return await GetResult<TOut>(msg);
                }
            }
        }

        /* Responsible for creating requests of type GET where the caller wants
         * to directly access the HTTP response stream, instead of having a typed
         * callback returned to him.
         */
        async Task CreateEmptyRequestStreamResponse(
            string url,
            net.HttpMethod method,
            Action<Stream, HttpStatusCode, Dictionary<string, string>> functor,
            Dictionary<string, string> headers)
        {
            using (var msg = CreateRequestMessage(method, url, headers))
            {
                using (var response = await _client.SendAsync(msg))
                {
                    using (var content = response.Content)
                    {
                        // Retrieving HTTP headers, both response and content headers.
                        var responseHeaders = GetHeaders(response, content);

                        // Checking if request was successful, and if not, throwing an exception.
                        functor(await content.ReadAsStreamAsync(), response.StatusCode, responseHeaders);
                    }
                }
            }
        }

        /* Responsible for sending and retrieving your HTTP request and response.
         * Only invoked if you are requesting a non Stream result.
         */
        async Task<Response<TOut>> GetResult<TOut>(
            net.HttpRequestMessage msg)
        {
            using (var response = await _client.SendAsync(msg))
            {
                using (var content = response.Content)
                {
                    // Retrieving HTTP headers, both response and content headers.
                    var responseHeaders = GetHeaders(response, content);

                    // Retrieving actual content.
                    var byteArray = await content.ReadAsByteArrayAsync();

                    // Checking is request was successful, and if not, throwing an exception.
                    if (!response.IsSuccessStatusCode)
                    {
                        var responseResult = new Response<TOut>
                        {
                            Error = Encoding.UTF8.GetString(byteArray),
                            Status = response.StatusCode,
                            Headers = responseHeaders,
                        };
                        return responseResult;
                    }
                    else
                    {
                        var responseResult = new Response<TOut>
                        {
                            Status = response.StatusCode,
                            Headers = responseHeaders,
                        };

                        if (typeof(TOut) == typeof(byte[]))
                        {
                            responseResult.Content = (TOut)(object)byteArray;
                        }
                        else if (typeof(TOut) == typeof(string))
                        {
                            responseResult.Content = (TOut)(object)Encoding.UTF8.GetString(byteArray);
                        }
                        else if (typeof(IConvertible).IsAssignableFrom(typeof(TOut)))
                        {
                            /*
                             * Checking if Response type implements IConvertible, at which point
                             * we simply converts the response instead of parsing it using
                             * JSON conversion.
                             *
                             * This might be used if caller is requesting for instance
                             * an integer, or some other object that has automatic conversion
                             * from string to its own type.
                             */
                            responseResult.Content = (TOut)Convert.ChangeType(
                                Encoding.UTF8.GetString(byteArray),
                                typeof(TOut));
                        }
                        else
                        {

                            /*
                             * Checking if caller is interested in some sort of JContainer,
                             * such as a JArray or JObject, at which point we simply return
                             * the above object immediately as such.
                             */
                            var objResult = JToken.Parse(Encoding.UTF8.GetString(byteArray));
                            if (typeof(TOut) == typeof(JContainer))
                                responseResult.Content = (TOut)(object)objResult;

                            /*
                             * Converting above JContainer to instance of requested type,
                             * and returns object to caller.
                             */
                            responseResult.Content = objResult.ToObject<TOut>();
                        }

                        // Finally, we can return result to caller.
                        return responseResult;
                    }
                }
            }
        }

        /*
         * Creates a new request message, and decorates it with the relevant
         * HTTP headers.
         */
        net.HttpRequestMessage CreateRequestMessage(
            net.HttpMethod method,
            string url,
            Dictionary<string, string> headers)
        {
            var msg = new net.HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = method
            };

            foreach (var idx in headers.Keys)
            {
                /*
                 * Notice, we're simply ignoring all headers that belongs to the content,
                 * and adding all other headers to the request.
                 * 
                 * This is done since all HttpContent headers are added later, but
                 * only if content is being transmitted.
                 *
                 * This allows us to support any HTTP headers, including any custom
                 * headers.
                 */
                switch (idx)
                {
                    case "Allow":
                    case "Content-Disposition":
                    case "Content-Encoding":
                    case "Content-Language":
                    case "Content-Length":
                    case "Content-Location":
                    case "Content-MD5":
                    case "Content-Range":
                    case "Content-Type":
                    case "Expires":
                    case "Last-Modified":
                        break;
                    default:
                        msg.Headers.Add(idx, headers[idx]);
                        break;
                }
            }
            return msg;
        }

        /*
         * Decorates the HTTP content with the relevant HTTP headers from the
         * specified dictionary.
         */
        void AddContentHeaders(
            net.HttpContent content,
            Dictionary<string, string> headers)
        {
            foreach (var idx in headers.Keys)
            {
                /*
                 * Adding all Content HTTP headers, and ignoring the rest
                 */
                switch (idx)
                {
                    case "Allow":
                    case "Content-Disposition":
                    case "Content-Encoding":
                    case "Content-Language":
                    case "Content-Length":
                    case "Content-Location":
                    case "Content-MD5":
                    case "Content-Range":
                    case "Content-Type":
                    case "Expires":
                    case "Last-Modified":
                        if (content.Headers.Contains(idx))
                            content.Headers.Remove(idx);
                        content.Headers.Add(idx, headers[idx]);
                        break;
                }
            }
        }

        Dictionary<string, string> GetDefaultBearerTokenHeaders(string token)
        {
            return new Dictionary<string, string> {
                    { "Accept", "application/json" },
                    { "Content-Type", "application/json" },
                    { "Authorization", "Bearer " + (token ?? throw new ArgumentNullException(nameof(token))) },
            };
        }

        Dictionary<string, string> GetHeaders(
            net.HttpResponseMessage response,
            net.HttpContent content)
        {
            var headers = new Dictionary<string, string>();
            foreach (var idx in response.Headers)
            {
                headers.Add(idx.Key, string.Join(";", idx.Value));
            }
            foreach (var idx in content.Headers)
            {
                headers.Add(idx.Key, string.Join(";", idx.Value));
            }

            return headers;
        }

        #endregion
    }
}