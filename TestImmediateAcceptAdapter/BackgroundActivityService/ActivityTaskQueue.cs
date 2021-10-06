// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace ImmediateAcceptBot.BackgroundQueue
{
    /// <summary>
    /// Singleton queue, used to transfer an ActivityWithAuthenticateRequestResult to the <see cref="HostedActivityService"/>.
    /// </summary>
    public class ActivityTaskQueue : IActivityTaskQueue
    {
        private SemaphoreSlim _signal = new SemaphoreSlim(0);
        private ConcurrentQueue<ActivityWithAuthenticateRequestResult> _activities = new ConcurrentQueue<ActivityWithAuthenticateRequestResult>();

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
        public void QueueBackgroundActivity(AuthenticateRequestResult authenticateResult, Activity activity)
        {
            if (authenticateResult == null)
            {
                throw new ArgumentNullException(nameof(authenticateResult));
            }

            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            _activities.Enqueue(new ActivityWithAuthenticateRequestResult { AuthenticateRequestResult = authenticateResult, Activity = activity});
            _signal.Release();
        }

        /// <summary>
        /// Wait for a signal of an enqueued Activity with Claims to be processed.
        /// </summary>
        /// <param name="cancellationToken">CancellationToken used to cancel the wait.</param>
        /// <returns>An ActivityWithAuthenticateRequestResult to be processed.
        /// </returns>
        /// <remarks>It is assumed these claims have already been authenticated.</remarks>
        public async Task<ActivityWithAuthenticateRequestResult> WaitForActivityAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);

            ActivityWithAuthenticateRequestResult dequeued;
            _activities.TryDequeue(out dequeued);

            return dequeued;
        }
    }
}
