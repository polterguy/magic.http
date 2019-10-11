/*
 * Magic, Copyright(c) Thomas Hansen 2019, thomas@gaiasoul.com, all rights reserved.
 * Permission to use under the terms of the MIT license is hereby granted, see
 * the enclosed LICENSE file for details.
 */

using System;
using System.Net;

namespace magic.http.contracts
{
    /// <summary>
    /// Exception thrown by Magic HTTP when your endpoint returns a non-successful
    /// status code.
    /// </summary>
    public class HttpException : Exception
    {
        /// <summary>
        /// Creates a new exception.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="statusCode">HTTP response status code.</param>
        public HttpException(string message, HttpStatusCode statusCode)
            : base (message)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// The HTTP response status code of your request.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }
    }
}