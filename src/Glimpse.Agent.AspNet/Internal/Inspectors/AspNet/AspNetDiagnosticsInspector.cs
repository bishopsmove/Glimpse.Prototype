﻿using System;
using System.Collections.Generic;
using System.Linq;
using Glimpse.Agent.Messages;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.TelemetryAdapter;

namespace Glimpse.Agent.Internal.Inspectors.Mvc
{
    public partial class WebDiagnosticsInspector
    {
        [TelemetryName("Microsoft.AspNet.Hosting.BeginRequest")]
        public void OnBeginRequest(HttpContext httpContext)
        {
            // TODO: Not sure if this is where this should live but it's the earlist hook point we have
            _contextData.Value = new MessageContext { Id = Guid.NewGuid(), Type = "Request" };

            var request = httpContext.Request;

            var beginMessage = new BeginRequestMessage
            {
                // TODO: check if there is a better way of doing this
                // TODO: should there be a StartTime property here?
                Url = $"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}{request.QueryString}",
                Path = request.Path,
                QueryString = request.QueryString.Value,
                Method = request.Method,
                Headers = request.Headers,
                ContentLength = request.ContentLength,
                ContentType = request.ContentType
            };

            _broker.BeginLogicalOperation(beginMessage);
        }

        [TelemetryName("Microsoft.AspNet.Hosting.EndRequest")]
        public void OnEndRequest(HttpContext httpContext)
        {
            var timing = _broker.EndLogicalOperation<BeginRequestMessage>().Timing;

            var request = httpContext.Request;
            var response = httpContext.Response;

            var endMessage = new EndRequestMessage
            {
                // TODO: check if there is a better way of doing this
                Url = $"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}{request.QueryString}",
                Path = request.Path,
                QueryString = request.QueryString.Value,
                Duration = Math.Round(timing.Elapsed.TotalMilliseconds, 2),
                Headers = response.Headers,
                ContentLength = response.ContentLength,
                ContentType = response.ContentType,
                StatusCode = response.StatusCode,
                StartTime = timing.Start.ToUniversalTime(),
                EndTime = timing.End.ToUniversalTime()
            };

            _broker.SendMessage(endMessage);
        }
    }
}
