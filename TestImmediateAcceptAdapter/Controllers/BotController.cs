using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Settings;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TestImmediateAcceptAdapter.Controllers
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly ImmediateAcceptAdapter _adapter;
        private readonly IBot _bot;
        private readonly ILogger<BotController> _logger;

        public BotController(
            IConfiguration configuration,
            ImmediateAcceptAdapter adapter,
            IBot bot,
            ILogger<BotController> logger)
        {
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _logger = logger;
            _adapter = adapter;
        }

        [HttpPost]
        [HttpGet]
        [Route("api/{route}")]
        public async Task PostAsync(string route)
        {
            if (string.IsNullOrEmpty(route))
            {
                _logger.LogError($"PostAsync: No route provided.");
                throw new ArgumentNullException(nameof(route));
            }

            await _adapter.ImmediateAcceptBotRequest(Request, Response, _bot, CancellationToken.None).ConfigureAwait(false);
        }
    }
}
