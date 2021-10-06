// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace ImmediateAcceptBot.BackgroundQueue
{
    /// <summary>
    /// Interface for a class used to transfer an activityWithAuthenticateRequestResult to the <see cref="HostedActivityService"/>.
    /// </summary>
    public interface IActivityTaskQueue
    {
        /// <summary>
        /// Enqueue an Activity, with AuthenticateRequestResult, to be processed on a background thread.
        /// </summary>
        /// <remarks>
        /// It is assumed these AuthenticateRequestResult aws been authenticated via BotFrameworkAuthentication.AuthenticateRequestAsync 
        /// before enqueueing.
        /// </remarks>
        /// <param name="authenticateResult">Authenticated <see cref="AuthenticateRequestResult"/> used to process the 
        /// activity.</param>
        /// <param name="activity"><see cref="Activity"/> to be processed.</param>
        void QueueBackgroundActivity(AuthenticateRequestResult authenticateResult, Activity activity);

        /// <summary>
        /// Wait for a signal of an enqueued Activity with Claims to be processed.
        /// </summary>
        /// <param name="cancellationToken">CancellationToken used to cancel the wait.</param>
        /// <returns>An activityWithAuthenticateRequestResult to be processed.</returns>
        /// <remarks>It is assumed these claims have already been authenticated.</remarks>
        Task<ActivityWithAuthenticateRequestResult> WaitForActivityAsync(CancellationToken cancellationToken);
    }
}
