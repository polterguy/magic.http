/*
 * Magic, Copyright(c) Thomas Hansen 2019 - thomas@gaiasoul.com
 * Permission to use under the terms of the MIT license is hereby granted, see
 * the enclosed LICENSE file for details.
 */

using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using net = System.Net.Http;
using Newtonsoft.Json.Linq;
using magic.http.contracts;

namespace magic.http.services
{
    /// <summary>
    /// Service implementation for IHttpClient. Uses System.Net.Http.HttpClient
    /// to invoke your HTTP requests.
    /// </summary>
    public class HttpClient : IHttpClient
    {
        static readonly net.HttpClient _client = new net.HttpClient();

        #region [ -- Interface implementation -- ]

        /// <summary>
        /// Posts an object asynchronously to the specified URL. Notice, you can supply a Stream as your request,
        /// and the service will intelligently determine it's a stream, and serialize it directly on to the HTTP request stream.
        /// </summary>
        /// <typeparam name="Request">Type of request.</typeparam>
        /// <typeparam name="Response">Type of response.</typeparam>
        /// <param name="url">URL of your request.</param>
        /// <param name="request">Payload of your request.</param>
        /// <param name="contentType">Optional Content-Type for your request. Defaults to "application/json" if omitted.</param>
        /// <param name="token">Optional Bearer token for your request.</param>
        /// <returns>Object returned from your request.</returns>
        public async Task<Response> PostAsync<Request, Response>(
            string url,
            Request request,
            string contentType = "application/json",
            string token = null)
        {
            return await CreateRequest<Response>(
                url,
                net.HttpMethod.Post,
                request,
                contentType,
                token);
        }

        /// <summary>
        /// Puts an object asynchronously to the specified URL. Notice, you can supply a Stream as your request,
        /// and the service will intelligently determine it's a stream, and serialize it directly on to the HTTP request stream.
        /// </summary>
        /// <typeparam name="Request">Type of request.</typeparam>
        /// <typeparam name="Response">Type of response.</typeparam>
        /// <param name="url">URL of your request.</param>
        /// <param name="request">Payload of your request.</param>
        /// <param name="contentType">Optional Content-Type for your request. Defaults to "application/json" if omitted.</param>
        /// <param name="token">Optional Bearer token for your request.</param>
        /// <returns>Object returned from your request.</returns>
        public async Task<Response> PutAsync<Request, Response>(
            string url,
            Request request,
            string contentType = "application/json",
            string token = null)
        {
            return await CreateRequest<Response>(
                url,
                net.HttpMethod.Put,
                request,
                contentType,
                token);
        }

        /// <summary>
        /// Gets a resource from some URL.
        /// </summary>
        /// <typeparam name="Response">Type of response.</typeparam>
        /// <param name="url">URL of your request.</param>
        /// <param name="token">Optional Bearer token for your request.</param>
        /// <returns>Object returned from your request.</returns>
        public async Task<Response> GetAsync<Response>(
            string url,
            string token = null)
        {
            return await CreateRequest<Response>(
                url,
                net.HttpMethod.Get,
                token);
        }

        /// <summary>
        /// Gets a resource from some URL. Notice, this overload requires you to supply
        /// an Action taking a Stream as its input, from where you can directly access the response content,
        /// without having to load it into memory. This i suseful for downloading larger documents from some URL.
        /// </summary>
        /// <param name="url">URL of your request.</param>
        /// <param name="functor">Action lambda function given the response Stream for you to do whatever you wish with once the request returns.</param>
        /// <param name="token">Optional Bearer token for your request.</param>
        /// <returns>Async void Task</returns>
        public async Task GetAsync(
            string url,
            Action<Stream> functor,
            string token = null)
        {
            await CreateRequest(
                url,
                net.HttpMethod.Get,
                functor,
                token);
        }

        /// <summary>
        /// Deletes some resource.
        /// </summary>
        /// <typeparam name="Response">Type of response.</typeparam>
        /// <param name="url">URL of your request.</param>
        /// <param name="token">Optional Bearer token for your request.</param>
        /// <returns>Result of your request.</returns>
        public async Task<Response> DeleteAsync<Response>(
            string url,
            string token = null)
        {
            return await CreateRequest<Response>(
                url,
                net.HttpMethod.Delete,
                token);
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
        /// <param name="token">Bearer token for your request.</param>
        /// <returns>Object returned from your request.</returns>
        virtual protected async Task<Response> CreateRequest<Response>(
            string url,
            net.HttpMethod method,
            string token)
        {
            return await CreateRequestMessage(url, method, token, async (msg) =>
            {
                return await GetResult<Response>(msg);
            });
        }

        /// <summary>
        /// Responsible for creating a request of the specified type. Used
        /// only during POST and PUT since it requires a payload to be provided.
        /// </summary>
        /// <typeparam name="Response">Type of response.</typeparam>
        /// <param name="url">URL of your request.</param>
        /// <param name="method">HTTP method or verb to create your request as.</param>
        /// <param name="input">Payload for your request.</param>
        /// <param name="contentType">Content-Type for your request.</param>
        /// <param name="token">Bearer token for your request.</param>
        /// <returns>Object returned from your request.</returns>
        virtual protected async Task<Response> CreateRequest<Response>(
            string url,
            net.HttpMethod method,
            object input,
            string contentType,
            string token)
        {
            return await CreateRequestMessage(url, method, token, async (msg) =>
            {
                if (input is Stream stream)
                {
                    using (var content = new net.StreamContent(stream))
                    {
                        if (contentType != null)
                            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                        msg.Content = content;
                        return await GetResult<Response>(msg);
                    }
                }

                var stringContent = input is string strInput ? strInput : JObject.FromObject(input).ToString();
                using (var content = new net.StringContent(stringContent))
                {
                    if (contentType != null)
                        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                    msg.Content = content;
                    return await GetResult<Response>(msg);
                }
            });
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
        /// <param name="token">Optional Bearer token for your request.</param>
        /// <returns>An async Task</returns>
        virtual protected async Task CreateRequest(
            string url,
            net.HttpMethod method,
            Action<Stream> functor,
            string token)
        {
            using(var msg = new net.HttpRequestMessage())
            {
                msg.RequestUri = new Uri(url);
                msg.Method = method;

                if (token != null)
                    msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using (var response = await _client.SendAsync(msg))
                {
                    using (var content = response.Content)
                    {
                        // Checking is request was successful, and if not, throwing an exception.
                        if (!response.IsSuccessStatusCode)
                            throw new HttpException(await content.ReadAsStringAsync(), response.StatusCode);

                        functor(await content.ReadAsStreamAsync());
                    }
                }
            }
        }

        /// <summary>
        /// Responsible for creating an HTTP request message for you, which
        /// by default is only interested in JSON and plain text result.
        /// </summary>
        /// <typeparam name="Response">Type of response.</typeparam>
        /// <param name="url">URL of your request.</param>
        /// <param name="method">HTTP method or verb to create your request as.</param>
        /// <param name="token">Optional Bearer token for your request.</param>
        /// <param name="functor">Callback function that will be invoked with
        /// your message when your request has been created.</param>
        /// <returns></returns>
        virtual protected async Task<Response> CreateRequestMessage<Response>(
            string url,
            net.HttpMethod method,
            string token,
            Func<net.HttpRequestMessage, Task<Response>> functor)
        {
            using(var msg = new net.HttpRequestMessage())
            {
                msg.RequestUri = new Uri(url);
                msg.Method = method;

                if (token != null)
                    msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                return await functor(msg);
            }
        }

        /// <summary>
        /// Responsible for sending and retrieving your HTTP request and response.
        /// Only invoked if you are requesting a non Stream result.
        /// </summary>
        /// <typeparam name="Response"></typeparam>
        /// <param name="msg"></param>
        /// <returns></returns>
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
    }
}