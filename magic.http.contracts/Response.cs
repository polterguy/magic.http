/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * Permission to use under the terms of the MIT license is hereby granted, see
 * the enclosed LICENSE file for details.
 */

using System.Net;
using System.Collections.Generic;

namespace magic.http.contracts
{
    /// <summary>
    /// Wraps an HTTP response, with headers, content and status code.
    /// </summary>
    public class Response<T>
    {
        /// <summary>
        /// Actual content object of the response.
        /// </summary>
        public T Content { get; set; }

        /// <summary>
        /// HTTP heders for the response.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Actual status response code for the request.
        /// </summary>
        public HttpStatusCode Status { get; set; }

        /// <summary>
        /// will only be populated if request is not successful, at which point the
        /// response content can be found in this property.
        /// </summary>
        public string Error { get; set; }
    }
}