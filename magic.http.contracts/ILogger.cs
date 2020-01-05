/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * Permission to use under the terms of the MIT license is hereby granted, see
 * the enclosed LICENSE file for details.
 */

using System;

namespace magic.http.contracts
{
    /// <summary>
    /// Logging interface for logging usage of library.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a debug piece of information.
        /// </summary>
        /// <param name="message">Log entry.</param>
        void Debug(string message);

        /// <summary>
        /// Logs an informational piece of information.
        /// </summary>
        /// <param name="message">Log entry.</param>
        void Info(string message);

        /// <summary>
        /// Logs an error occurring.
        /// </summary>
        /// <param name="message">Log entry.</param>
        /// <param name="exception">Optional exception you want to log.</param>
        void Error(string message, Exception exception = null);

        /// <summary>
        /// Logs a fatal error occurring.
        /// </summary>
        /// <param name="message">Log entry.</param>
        /// <param name="exception">Optional exception you want to log.</param>
        void Fatal(string message, Exception exception = null);
    }
}