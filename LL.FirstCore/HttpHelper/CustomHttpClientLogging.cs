using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LL.FirstCore.HttpHelper
{
    /// <summary>
    /// 解决HttpClientFactory日志无追踪机制的探索
    /// https://www.cnblogs.com/JulianHuang/p/11982021.html
    /// </summary>
    public class CustomHttpClientLogging
    {
        //P1 实现 IHttpMessageHandlerFilter接口，在接口中移除默认的两个日志处理器
        public class TraceIdLoggingMessageHandlerFilter : IHttpMessageHandlerBuilderFilter
        {
            private readonly ILoggerFactory _loggerFactory;

            public TraceIdLoggingMessageHandlerFilter(ILoggerFactory loggerFactory)
            {
                _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            }

            public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
            {
                if (next == null)
                {
                    throw new ArgumentNullException(nameof(next));
                }

                return (builder) =>
                {
                    // Run other configuration first, we want to decorate.
                    next(builder);

                    var outerLogger = _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.LogicalHandler");
                    //实现 IHttpMessageHandlerFilter接口，在接口中移除默认的两个日志处理器
                    builder.AdditionalHandlers.Clear();
                    builder.AdditionalHandlers.Insert(0, new CustomLoggingScopeHttpMessageHandler(outerLogger));
                };
            }
        }

        //P2 实现带有TraceId能力的HttpClient 日志处理器， 并加入到IHttpMessageHandlerFilter接口实现类
        public class CustomLoggingScopeHttpMessageHandler : DelegatingHandler
        {
            private readonly ILogger _logger;

            public CustomLoggingScopeHttpMessageHandler(ILogger logger)
            {
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                using (Log.BeginRequestPipelineScope(_logger, request))
                {
                    Log.RequestPipelineStart(_logger, request);
                    var response = await base.SendAsync(request, cancellationToken);
                    Log.RequestPipelineEnd(_logger, response);

                    return response;
                }
            }

            private static class Log
            {
                private static class EventIds
                {
                    public static readonly EventId PipelineStart = new EventId(100, "RequestPipelineStart");
                    public static readonly EventId PipelineEnd = new EventId(101, "RequestPipelineEnd");
                }

                private static readonly Func<ILogger, HttpMethod, Uri, string, IDisposable> _beginRequestPipelineScope =
                    LoggerMessage.DefineScope<HttpMethod, Uri, string>(
                        "HTTP {HttpMethod} {Uri} {CorrelationId}");

                private static readonly Action<ILogger, HttpMethod, Uri, string, Exception> _requestPipelineStart =
                    LoggerMessage.Define<HttpMethod, Uri, string>(
                        LogLevel.Information,
                        EventIds.PipelineStart,
                        "Start processing HTTP request {HttpMethod} {Uri} [Correlation: {CorrelationId}]");

                private static readonly Action<ILogger, HttpStatusCode, string, Exception> _requestPipelineEnd =
                    LoggerMessage.Define<HttpStatusCode, string>(
                        LogLevel.Information,
                        EventIds.PipelineEnd,
                        "End processing HTTP request - {StatusCode}, [Correlation: {CorrelationId}]");

                public static IDisposable BeginRequestPipelineScope(ILogger logger, HttpRequestMessage request)
                {
                    var correlationId = GetCorrelationIdFromRequest(request);
                    return _beginRequestPipelineScope(logger, request.Method, request.RequestUri, correlationId);
                }

                public static void RequestPipelineStart(ILogger logger, HttpRequestMessage request)
                {
                    var correlationId = GetCorrelationIdFromRequest(request);
                    _requestPipelineStart(logger, request.Method, request.RequestUri, correlationId, null);
                }

                public static void RequestPipelineEnd(ILogger logger, HttpResponseMessage response)
                {
                    var correlationId = GetCorrelationIdFromRequest(response.RequestMessage);
                    _requestPipelineEnd(logger, response.StatusCode, correlationId, null);
                }

                private static string GetCorrelationIdFromRequest(HttpRequestMessage request)
                {
                    string correlationId;
                    if (request.Headers.TryGetValues("X-Correlation-ID", out var values))
                        correlationId = values.First();
                    else
                    { correlationId = Guid.NewGuid().ToString(); request.Headers.Add("X-Correlation-ID", correlationId); }
                    return correlationId;
                }
            }
        }

        public class PrimaryHttpMessageHandler : DelegatingHandler
        {
            private IServiceProvider _provider;

            public PrimaryHttpMessageHandler(IServiceProvider provider)
            {
                _provider = provider;
                InnerHandler = new HttpClientHandler();
            }

            protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Console.WriteLine("PrimaryHttpMessageHandler Start Log");

                var response = await base.SendAsync(request, cancellationToken);
                Console.WriteLine("PrimaryHttpMessageHandler End Log");
                return response;
            }
        }

        public class Log2HttpMessageHandler : DelegatingHandler
        {
            private IServiceProvider _provider;

            public Log2HttpMessageHandler(IServiceProvider provider)
            {
                _provider = provider;
                //InnerHandler = new HttpClientHandler();
            }

            protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Console.WriteLine("LogHttpMessageHandler2 Start Log");
                var response = await base.SendAsync(request, cancellationToken);
                Console.WriteLine("LogHttpMessageHandler2 End Log");

                return response;
            }
        }

        public class LogHttpMessageHandler : DelegatingHandler
        {
            private IServiceProvider _provider;

            public LogHttpMessageHandler(IServiceProvider provider)
            {
                _provider = provider;
                //InnerHandler = new HttpClientHandler();
            }

            protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Console.WriteLine("LogHttpMessageHandler Start Log");
                var response = await base.SendAsync(request, cancellationToken);
                Console.WriteLine("LogHttpMessageHandler End Log");
                return response;
            }
        }
    }
}
