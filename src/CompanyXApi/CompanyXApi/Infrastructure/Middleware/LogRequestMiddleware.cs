﻿using System;
using System.IO;
using System.Threading.Tasks;
using CompanyX.Api.Infrastructure.Helpers;
using CompanyX.Base.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace CompanyX.Api.Infrastructure.Middleware
{
    /// <summary>
    /// Log request
    /// Credits: https://github.com/sulhome/log-request-response-middleware
    /// </summary>
    public class LogRequestMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<LogRequestMiddleware> _logger;
        private readonly Func<string, Exception, string> _defaultFormatter = (state, exception) => state;

        /// <summary>
        /// LogRequestMiddleware constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public LogRequestMiddleware(RequestDelegate next, ILogger<LogRequestMiddleware> logger)
        {
            Guard.IsNotNull(logger, () => logger);

            _logger = logger;
            this.next = next;
        }

        /// <summary>
        ///  Log http request and pass http context to next pipeline middle ware
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            var requestBodyStream = new MemoryStream();
            var originalRequestBody = context.Request.Body;

            await context.Request.Body.CopyToAsync(requestBodyStream);
            requestBodyStream.Seek(0, SeekOrigin.Begin);

            var url = context.Request.GetDisplayUrl();
            var requestBodyText = new StreamReader(requestBodyStream).ReadToEnd();
            _logger.Log(LogLevel.Information, Const.LogRequestEventId, $"REQUEST METHOD: {context.Request.Method}, REQUEST BODY: {requestBodyText}, REQUEST URL: {url}", null, _defaultFormatter);

            requestBodyStream.Seek(0, SeekOrigin.Begin);
            context.Request.Body = requestBodyStream;

            await next(context);
            context.Request.Body = originalRequestBody;
        }
    }
}
