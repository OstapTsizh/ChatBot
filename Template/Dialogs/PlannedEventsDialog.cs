using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LoggerService;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

namespace StuddyBot.Dialogs
{
    /// <summary>
    /// This dialog is responsible for the communication about planned events.
    /// information to the user's email.
    /// </summary>
    public class PlannedEventsDialog : ComponentDialog// CancelAndRestartDialog
    {
        private readonly IDecisionMaker DecisionMaker;
        private readonly ThreadedLogger _myLogger;
        private DialogInfo _DialogInfo;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        private Dictionary<string, List<string>> _events;


        public PlannedEventsDialog(IDecisionMaker decisionMaker, 
                             ThreadedLogger _myLogger, 
                             DialogInfo dialogInfo, 
                             ConcurrentDictionary<string, ConversationReference> conversationReferences,
                             DialogsMUI dialogsMui)
            : base(nameof(PlannedEventsDialog))
        {
            
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            _DialogInfo = dialogInfo;
            _conversationReferences = conversationReferences;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AskSelectEventStepAsync,
                FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Asks the user to select question/answer.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> AskSelectEventStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _events = DecisionMaker.GetPlannedEvents(_DialogInfo.Language);

            var choices = new List<Choice>();

            foreach (var q in _events.Keys)
            {
                choices.Add(new Choice(q));
            }

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Виберіть питання:"), // Choose a question:
                Choices = choices,
                RetryPrompt = MessageFactory.Text("Будь ласка, спробуйте ще раз:"), // Try one more time, please
                Style = ListStyle.HeroCard
            };

            var message = options.Prompt.Text;
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;

            _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        /// <summary>
        /// Shows selected event.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var choiceValue = (string)(stepContext.Result as FoundChoice).Value;

            var msg = MessageFactory.Text(string.Join("\n", _events[choiceValue]));
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;

            _myLogger.LogMessage(msg.Text, sender, time, _DialogInfo.DialogId);

            await stepContext.Context.SendActivityAsync(msg, cancellationToken);
            
            return await stepContext.ReplaceDialogAsync(nameof(ChooseOptionDialog), "begin",
                cancellationToken: cancellationToken);
        }
    }
}
