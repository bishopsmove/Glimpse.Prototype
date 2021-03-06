using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Glimpse.Agent.Configuration
{
    public class RequestIgnorerOptionsShouldIgnore : IRequestIgnorer
    {
        private readonly Func<HttpContext, bool> _shouldIgnore;

        public RequestIgnorerOptionsShouldIgnore(IOptions<GlimpseAgentOptions> optionsAccessor)
        {
            _shouldIgnore = optionsAccessor.Value.ShouldIgnore;
        }
        
        public bool ShouldIgnore(HttpContext context)
        {
            return _shouldIgnore != null ? _shouldIgnore(context) : false;
        }
    }
}
