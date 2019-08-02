using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmailSender.Interfaces;
using LoggerService;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using StuddyBot.Core.DAL.Data;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

namespace StuddyBot.Dialogs
{
    /// <summary>
    /// This dialog is responsible for the communication about
    /// adding new question to the QA database.
    /// </summary>
    public class FinishDialog : ComponentDialog// CancelAndRestartDialog
    {
        private readonly IDecisionMaker DecisionMaker;
        private readonly ThreadedLogger _myLogger;
        private DialogInfo _DialogInfo;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        
        public FinishDialog(IDecisionMaker decisionMaker, IEmailSender emailSender, ISubscriptionManager SubscriptionManager,
                             ThreadedLogger _myLogger, 
                             DialogInfo dialogInfo, 
                             ConcurrentDictionary<string, ConversationReference> conversationReferences, StuddyBotContext db)
            : base(nameof(FinishDialog))
        {
            
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            _DialogInfo = dialogInfo;
            _conversationReferences = conversationReferences;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ChooseOptionDialog(DecisionMaker, emailSender, SubscriptionManager, _myLogger, dialogInfo, conversationReferences, db));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                DidWeFinishStepAsync,
                LeaveFeedbackStepAsync,
                FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Asks the user did we finish dialog.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> DidWeFinishStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choices = new List<Choice>();

            {
                choices.Add(new Choice("Так/Залишити відгук")); // Yes/Leave feedback
                choices.Add(new Choice("Ні")); // No
            }

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Чи я відповів на усі запитання?"), // Did I answer all your questions?
                Choices = choices,
                Style = ListStyle.HeroCard
            };

            var message = options.Prompt.Text;
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;

            _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        /// <summary>
        /// Asks the user to type a feedback.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> LeaveFeedbackStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choiceValue = (string)(stepContext.Result as FoundChoice).Value;

            if (choiceValue=="Ні") // No
            {
                return await stepContext.ReplaceDialogAsync(nameof(ChooseOptionDialog), "begin",
                    cancellationToken: cancellationToken);
            }

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Будь ласка, напишіть Ваш відгук:"), // Type Your feedback, please:
                Style = ListStyle.HeroCard
            };

            var message = options.Prompt.Text;
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;

            _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

            return await stepContext.PromptAsync(nameof(TextPrompt), options, cancellationToken);
        }

        /// <summary>
        /// Ends dialog(s) if a user has not any questions.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var feedback = stepContext.Result.ToString();

            if (!string.IsNullOrEmpty(feedback))
            {

                // ToDo write a feedback into a Database
            }

            return await stepContext.CancelAllDialogsAsync(cancellationToken);
        }
    }
}
