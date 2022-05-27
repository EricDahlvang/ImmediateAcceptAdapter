using ImmediateAcceptBot.BackgroundQueue;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace TestImmediateAcceptAdapter
{
    public class ImmediateAcceptAdapter : CloudAdapter
    {
        private readonly IActivityTaskQueue _activityTaskQueue;

        public ImmediateAcceptAdapter(
            BotFrameworkAuthentication botFrameworkAuthentication,
            IEnumerable<Microsoft.Bot.Builder.IMiddleware> middlewares,
            IActivityTaskQueue activityTaskQueue,
            ILogger logger = null)
            : base(botFrameworkAuthentication, logger)
        {
            _activityTaskQueue = activityTaskQueue;

            // Pick up feature based middlewares such as telemetry or transcripts
            foreach (Microsoft.Bot.Builder.IMiddleware middleware in middlewares)
            {
                Use(middleware);
            }

            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                Logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

                // Send the exception message to the user. Since the default behavior does not
                // send logs or trace activities, the bot appears hanging without any activity
                // to the user.
                await turnContext.SendActivityAsync(exception.Message).ConfigureAwait(false);

                var conversationState = turnContext.TurnState.Get<ConversationState>();

                if (conversationState != null)
                {
                    // Delete the conversationState for the current conversation to prevent the
                    // bot from getting stuck in a error-loop caused by being in a bad state.
                    await conversationState.DeleteAsync(turnContext).ConfigureAwait(false);
                }
            };
        }

        public Task<InvokeResponse> ProcessAuthenticatedActivity(AuthenticateRequestResult authenticateRequestResult, Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            return base.ProcessActivityAsync(authenticateRequestResult, activity, callback, cancellationToken);
        }

        public async Task ImmediateAcceptBotRequest(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken)
        {
            _ = httpRequest ?? throw new ArgumentNullException(nameof(httpRequest));
            _ = httpResponse ?? throw new ArgumentNullException(nameof(httpResponse));
            _ = bot ?? throw new ArgumentNullException(nameof(bot));

            try
            {
                // Only GET requests for web socket connects are allowed
                if (httpRequest.Method == HttpMethods.Get && httpRequest.HttpContext.WebSockets.IsWebSocketRequest)
                {
                    // All socket communication will be handled by the internal streaming-specific BotAdapter
                    await base.ProcessAsync(httpRequest, httpResponse, bot, cancellationToken);
                }
                else if (httpRequest.Method == HttpMethods.Post)
                {
                    // Deserialize the incoming Activity
                    var activity = await HttpHelper.ReadRequestAsync<Activity>(httpRequest);

                    // A POST request must contain an Activity 
                    if (string.IsNullOrEmpty(activity?.Type))
                    {
                        httpResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                        return;
                    }
                    
                    if (activity.Type == ActivityTypes.Invoke || activity.DeliveryMode == DeliveryModes.ExpectReplies)
                    {
                        request.Body.Seek(0, SeekOrigin.Begin);
                        
                        // NOTE: Invoke and ExpectReplies cannot be performed async, the response must be written before the calling thread is released.
                        await base.ProcessAsync(httpRequest, httpResponse, bot, cancellationToken);
                    }
                    else
                    {
                        // Grab the auth header from the inbound http request
                        var authHeader = httpRequest.Headers["Authorization"];
                        var authenticateRequestResult = await BotFrameworkAuthentication.AuthenticateRequestAsync(activity, authHeader, cancellationToken).ConfigureAwait(false);

                        // Queue the activity to be processed by the ActivityBackgroundService
                        _activityTaskQueue.QueueBackgroundActivity(authenticateRequestResult, activity);
                    }

                    // Activity has been queued to process, so return Ok immediately
                    httpResponse.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    httpResponse.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // handle unauthorized here as this layer creates the http response
                httpResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
        }
    }
}
