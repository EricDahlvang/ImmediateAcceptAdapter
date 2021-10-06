// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
using System.Security.Claims;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace ImmediateAcceptBot.BackgroundQueue
{
    /// <summary>
    /// Activity with Claims which should already have been authenticated.
    /// </summary>
    public class ActivityWithAuthenticateRequestResult
    {
        /// <summary>
        /// <see cref="AuthenticateRequestResult"/> retrieved from a call to BotFrameworkAuthentication.AuthenticateRequestAsync.
        /// <seealso cref="ImmediateAcceptAdapter"/>
        /// </summary>
        public AuthenticateRequestResult AuthenticateRequestResult { get; set; }

        /// <summary>
        /// <see cref="Activity"/> which is to be processed.
        /// </summary>
        public Activity Activity { get; set; }
    }
}
