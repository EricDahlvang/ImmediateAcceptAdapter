using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TestImmediateAcceptAdapter.CustomAction
{
    /// <summary>
    /// Custom command which executes a method, sending it turncontext and expecting a reply of DialogTrnResult.
    /// </summary>
    public class SleepAction : Dialog
    {
        [JsonConstructor]
        public SleepAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            // enable instances of this command as debug break point
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        [JsonProperty("$kind")]
        public const string Kind = "SleepAction";

        /// <summary>
        /// Gets or sets memory path to bind to for the amount of time to sleep.
        /// </summary>
        [JsonProperty("secondsToSleep")]
        public IntExpression SecondsToSleep { get; set; }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var conversationState = dc.Context.TurnState.Get<ConversationState>();
            var isSleepingProperty = conversationState.CreateProperty<bool>("conversationIsSleeping");
            if(await isSleepingProperty.GetAsync(dc.Context, ()=> false))
            {
                await dc.Context.SendActivityAsync("Already sleeping. Please wait...").ConfigureAwait(false);
                return new DialogTurnResult(DialogTurnStatus.CompleteAndWait);
            }

            await isSleepingProperty.SetAsync(dc.Context, true, cancellationToken).ConfigureAwait(false);
            await conversationState.SaveChangesAsync(dc.Context, true).ConfigureAwait(false);
            
            var sleepTime = SecondsToSleep.GetValue(dc.State);
            if(sleepTime <= 0)
            {
                await dc.Context.SendActivityAsync("SecondsToSleep must be a positive number.").ConfigureAwait(false);
            }
            else
            {
                await dc.Context.SendActivityAsync($"Sleeping thread for {sleepTime} seconds.").ConfigureAwait(false);
                Thread.Sleep(sleepTime * 1000);
                await dc.Context.SendActivityAsync($"Finished sleeping for {sleepTime} seconds.").ConfigureAwait(false);
            }

            await isSleepingProperty.SetAsync(dc.Context, false, cancellationToken).ConfigureAwait(false);

            return new DialogTurnResult(DialogTurnStatus.Complete);
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            await dc.Context.SendActivityAsync("Already sleeping. Please wait...").ConfigureAwait(false);
            return new DialogTurnResult(DialogTurnStatus.CompleteAndWait);
        }
    }
}
