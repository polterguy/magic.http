/*
 * Magic, Copyright(c) Thomas Hansen 2019 - thomas@gaiasoul.com
 * Permission to use under the terms of the MIT license is hereby granted, see
 * the enclosed LICENSE file for details.
 */

using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using net = System.Net.Http;
using Newtonsoft.Json.Linq;
using magic.http.contracts;

namespace magic.http.services
{
    /// <summary>
    /// Service implementation for IHttpClient. Uses System.Net.Http.HttpClient
    /// internally to invoke your HTTP requests.
    /// </summary>
    public class HttpClient : IHttpClient
    {
        static readonly net.HttpClient _client = new net.HttpClient();

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

        #region [ -- Interface implementation -- ]

        /// <summary>
        /// Posts an object asynchronously to the specified URL. Notice, you can
        /// supply a Stream as your request, and the service will intelligently
        /// determine it's a stream, and serialize it directly on to the HTTP
        /// request stream.
        /// </summary>
        /// <typeparam name="Request">Type of request.</typeparam>
        /// <typeparam name="Response">Type of response.</typeparam>
        /// <param name="url">URL of your request.</param>
        /// <param name="request">Payload of your request.</param>
        /// <param name="headers">HTTP headers for your request.</param>
        /// <returns>Object returned from your request.</returns>
        public async Task<Response> PostAsync<Request, Response>(
            string url,
            Request request,
            Dictionary<string, string> headers = null)
        {
            return await CreateContentRequest<Response>(
                url,
                net.HttpMethod.Post,
                request,
                headers ?? DEFAULT_HEADERS_REQUEST);
        }

        /// <summary>
        /// Posts an object asynchronously to the specified URL with the specified Bearer token.
        /// Notice, you can supply a Stream as your request, and the service will intelligently
        /// determine it's a stream, and serialize it directly on to the HTTP request stream.
        /// </summary>
        /// <typeparam name="Request">Type of request.</typeparam>
        /// <typeparam name="Response">Type of response.</typeparam>
        /// <param name="url">URL of your request.</param>
        /// <param name="request">Payload of your request.</param>
        /// <param name="token">Bearer token for your request.</param>
        /// <returns>Object returned from your request.</returns>
        public async Task<Response> PostAsync<Request, Response>(
            string url,
            Request request,
            string token)
        {
            return await CreateContentRequest<Response>(
                url,
                net.HttpMethod.Post,
                request,
                GetDefaultBearerTokenHeaders(token));
        }

        /// <summary>
        /// Puts an object asynchronously to the specified URL. Notice, you can
        /// supply a Stream as your request, and the service will intelligently
        /// determine it's a stream, and serialize it directly on to the HTTP
        /// request stream.
        /// </summary>
        /// <typeparam name="Request">Type of request.</typeparam>
        /// <typeparam name="Response">Type of response.</typeparam>
        /// <param name="url">URL of your request.</param>
        /// <param name="request">Payload of your request.</param>
        /// <param name="headers">HTTP headers for your request.</param>
        /// <returns>Object returned from your request.</returns>
        public async Task<Response> PutAsync<Request, Response>(
            string url,
            Request request,
            Dictionary<string, string> headers = null)
        {
            return await CreateContentRequest<Response>(
                url,
                net.HttpMethod.Put,
                request,
                headers ?? DEFAULT_HEADERS_REQUEST);
        }

        /// <summary>
        /// Puts an object asynchronously to the specified URL with the specified Bearer token.
        /// Notice, you can supply a Stream as your request, and the service will intelligently
        /// determine it's a stream, and serialize it directly on to the HTTP request stream.
        /// </summary>
        /// <typeparam name="Request">Type of request.</typeparam>
        /// <typeparam name="Response">Type of response.</typeparam>
        /// <param name="url">URL of your request.</param>
        /// <param name="request">Payload of your request.</param>
        /// <param name="token">Bearer token for your request.</param>
        /// <returns>Object returned from your request.</returns>
        public async Task<Response> PutAsync<Request, Response>(
            string url,
            Request request,
            string token)
        {
            return await CreateContentRequest<Response>(
                url,
                net.HttpMethod.Put,
                request,
                GetDefaultBearerTokenHeaders(token));
        }

        /// <summary>
        /// Gets a resource from some URL.
        /// </summary>
        /// <typeparam name="Response">Type of response.</typeparam>
        /// <param name="url">URL of your request.</param>
        /// <param name="headers">HTTP headers for your request.</param>
        /// <returns>Object returned from your request.</returns>
        public async Task<Response> GetAsync<Response>(
            string url,
            Dictionary<string, string> headers = null)
        {
            return await CreateEmptyRequest<Response>(
                url,
                net.HttpMethod.Get,
                headers ?? DEFAULT_HEADERS_EMPTY_REQUEST);
        }

        /// <summary>
        /// Gets a resource from some URL with the specified Bearer token.
        /// </summary>
        /// <typeparam name="Response">Type of response.</typeparam>
        /// <param name="url">URL of your request.</param>
        /// <param name="token">Bearer token for your request.</param>
        /// <returns>Object returned from your request.</returns>
        public async Task<Response> GetAsync<Response>(
            string url,
            string token)
        {
            return await CreateEmptyRequest<Response>(
                url,
                net.HttpMethod.Get,
                GetDefaultBearerTokenHeaders(token));
        }

        /// <summary>
        /// Gets a resource from some URL. Notice, this overload requires you to supply
        /// an Action taking a Stream as its input, from where you can directly access the response content,
        /// without having to load it into memory. This i suseful for downloading larger documents from some URL.
        /// </summary>
        /// <param name="url">URL of your request.</param>
        /// <param name="functor">Action lambda function given the response Stream for you to do whatever you wish with once the request returns.</param>
        /// <param name="headers">HTTP headers for your request.</param>
        /// <returns>Async void Task</returns>
        public async Task GetAsync(
            string url,
            Action<Stream> functor,
            Dictionary<string, string> headers = null)
        {
            await CreateEmptyRequestStreamResponse(
                url,
                net.HttpMethod.Get,
                functor,
                headers ?? DEFAULT_HEADERS_EMPTY_REQUEST);
        }

        /// <summary>
        /// Gets a resource from some URL with the specified Bearer token.
        /// Notice, this overload requires you to supply an Action taking a Stream
        /// as its input, from where you can directly access the response content,
        /// without having to load it into memory. This i suseful for downloading
        /// larger documents from some URL.
        /// </summary>
        /// <param name="url">URL of your request.</param>
        /// <param name="functor">Action lambda function given the response Stream for you to do whatever you wish with once the request returns.</param>
        /// <param name="token">Bearer token for your request.</param>
        /// <returns>Async void Task</returns>
        public async Task GetAsync(
            string url,
            Action<Stream> functor,
            string token)
        {
            await CreateEmptyRequestStreamResponse(
                url,
                net.HttpMethod.Get,
                functor,
                GetDefaultBearerTokenHeaders(token));
        }

        /// <summary>
        /// Deletes some resource.
        /// </summary>
        /// <typeparam name="Response">Type of response.</typeparam>
        /// <param name="url">URL of your request.</param>
        /// <param name="headers">HTTP headers for your request.</param>
        /// <returns>Result of your request.</returns>
        public async Task<Response> DeleteAsync<Response>(
            string url,
            Dictionary<string, string> headers = null)
        {
            return await CreateEmptyRequest<Response>(
                url,
                net.HttpMethod.Delete,
                headers ?? DEFAULT_HEADERS_EMPTY_REQUEST);
        }

        /// <summary>
        /// Deletes some resource with the specified Bearer token.
        /// </summary>
        /// <typeparam name="Response">Type of response.</typeparam>
        /// <param name="url">URL of your request.</param>
        /// <param name="token">Bearer token for your request.</param>
        /// <returns>Result of your request.</returns>
        public async Task<Response> DeleteAsync<Response>(
            string url,
            string token)
        {
            return await CreateEmptyRequest<Response>(
                url,
                net.HttpMethod.Delete,
                GetDefaultBearerTokenHeaders(token));
        }

        #endregion

        #region [ -- Protected virtual methods -- ]

        /// <summary>
        /// Responsible for creating an HTTP request of specified type. Only used
        /// during GET and DELETE requests, since you cannot apply a payload to
        /// your request.
        /// </summary>
        /// <typeparam name="Response">Type of response.</typeparam>
        /// <param name="url">URL of your request.</param>
        /// <param name="method">HTTP method or verb to create your request as.</param>
        /// <param name="headers">HTTP headers for your request.</param>
        /// <returns>Object returned from your request.</returns>
        virtual protected async Task<Response> CreateEmptyRequest<Response>(
            string url,
            net.HttpMethod method,
            Dictionary<string, string> headers)
        {
            using (var msg = CreateRequestMessage(method, url, headers))
            {
                return await GetResult<Response>(msg);
            }
        }

        /// <summary>
        /// Responsible for creating a request of the specified type. Used
        /// only during POST and PUT since it requires a payload to be provided.
        /// </summary>
        /// <typeparam name="Response">Type of response.</typeparam>
        /// <param name="url">URL of your request.</param>
        /// <param name="method">HTTP method or verb to create your request as.</param>
        /// <param name="input">Payload for your request.</param>
        /// <param name="headers">HTTP headers for your request.</param>
        /// <returns>Object returned from your request.</returns>
        virtual protected async Task<Response> CreateContentRequest<Response>(
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
                        return await GetResult<Response>(msg);
                    }
                }

                var stringContent = input is string strInput ?
                    strInput :
                    JObject.FromObject(input).ToString();

                using (var content = new net.StringContent(stringContent))
                {
                    AddContentHeaders(content, headers);
                    msg.Content = content;
                    return await GetResult<Response>(msg);
                }
            }
        }

        /// <summary>
        /// Responsible for creating requests of type GET where the caller wants
        /// to directly access the HTTP response stream, instead of having a typed
        /// callback returned to him.
        /// </summary>
        /// <param name="url">URL of your request.</param>
        /// <param name="method">HTTP method or verb to create your request as.</param>
        /// <param name="functor">Callback function that will be invoked with the response
        /// stream when it is ready.</param>
        /// <param name="headers">HTTP headers for your request.</param>
        /// <returns>An async Task</returns>
        virtual protected async Task CreateEmptyRequestStreamResponse(
            string url,
            net.HttpMethod method,
            Action<Stream> functor,
            Dictionary<string, string> headers)
        {
            using (var msg = CreateRequestMessage(method, url, headers))
            {
                using (var response = await _client.SendAsync(msg))
                {
                    using (var content = response.Content)
                    {
                        // Checking if request was successful, and if not, throwing an exception.
                        if (!response.IsSuccessStatusCode)
                            throw new HttpException(await content.ReadAsStringAsync(), response.StatusCode);

                        functor(await content.ReadAsStreamAsync());
                    }
                }
            }
        }

        /// <summary>
        /// Responsible for sending and retrieving your HTTP request and response.
        /// Only invoked if you are requesting a non Stream result.
        /// </summary>
        /// <typeparam name="Response">Response type from endpoint.</typeparam>
        /// <param name="msg">HTTP request message.</param>
        /// <returns>Object returned from your request.</returns>
        virtual protected async Task<Response> GetResult<Response>(net.HttpRequestMessage msg)
        {
            using (var response = await _client.SendAsync(msg))
            {
                using (var content = response.Content)
                {
                    var responseContent = await content.ReadAsStringAsync();

                    // Checking is request was successful, and if not, throwing an exception.
                    if (!response.IsSuccessStatusCode)
                        throw new HttpException(responseContent, response.StatusCode);

                    // Checking if caller wants a string type of return
                    if (typeof(Response) == typeof(string))
                        return (Response)(object)responseContent;

                    /*
                     * Checking if Response type implements IConvertible, at which point
                     * we simply converts the response instead of parsing it using
                     * JSON conversion.
                     *
                     * This might be used if caller is requesting for instance
                     * an integer, or some other object that has automatic conversion
                     * from string to itself.
                     */
                    if (typeof(IConvertible).IsAssignableFrom(typeof(Response)))
                        return (Response)Convert.ChangeType(responseContent, typeof(Response));

                    /*
                     * Checking if caller is interested in some sort of JContainer,
                     * such as a JArray or JObject, at which point we simply return
                     * the above object immediately as such.
                     */
                    var objResult = JToken.Parse(responseContent);
                    if (typeof(Response) == typeof(JContainer))
                        return (Response)(object)objResult;

                    /*
                     * Converting above JContainer to instance of requested type,
                     * and returns object to caller.
                     */
                    return objResult.ToObject<Response>();
                }
            }
        }

        #endregion

        #region [ -- Private helper methods -- ]

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
        void AddContentHeaders(net.HttpContent content, Dictionary<string, string> headers)
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

        #endregion
    }
}