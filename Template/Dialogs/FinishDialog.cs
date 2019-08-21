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
        //private DialogInfo _DialogInfo;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private StuddyBotContext _db;
        private IStatePropertyAccessor<DialogInfo> _dialogInfoStateProperty;


        public FinishDialog(IStatePropertyAccessor<DialogInfo> dialogInfoStateProperty, IDecisionMaker decisionMaker, IEmailSender emailSender, ISubscriptionManager SubscriptionManager,
                             ThreadedLogger _myLogger, 
                             //DialogInfo dialogInfo, 
                             ConcurrentDictionary<string, ConversationReference> conversationReferences, StuddyBotContext db)
            : base(nameof(FinishDialog))
        {
            
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            //_DialogInfo = dialogInfo;
            _conversationReferences = conversationReferences;
            _db = db;
            _dialogInfoStateProperty = dialogInfoStateProperty;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ChooseOptionDialog(dialogInfoStateProperty, DecisionMaker, emailSender, SubscriptionManager, _myLogger,
                //dialogInfo,
                conversationReferences, db));
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
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);
            
            var dialogsMUI = DecisionMaker.GetDialogsMui(_DialogInfo.Language);
            var choices = new List<Choice>();

            var leaveFeedback = dialogsMUI.FinishDictionary["leave"];// Leave feedback
            var answered = dialogsMUI.FinishDictionary["answered"]; // Did I answer all your questions?

            {
                choices.Add(new Choice(dialogsMUI.MainDictionary["yes"]));
                choices.Add(new Choice(leaveFeedback)); 
                choices.Add(new Choice(dialogsMUI.MainDictionary["no"]));
            }

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text(answered),
                Choices = choices,
                Style = ListStyle.SuggestedAction
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
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);
            var dialogsMUI = DecisionMaker.GetDialogsMui(_DialogInfo.Language);
            var choiceValue = (string)(stepContext.Result as FoundChoice).Value;
            
            if (choiceValue==dialogsMUI.MainDictionary["no"]) 
            {
                return await stepContext.ReplaceDialogAsync(nameof(ChooseOptionDialog), "begin",
                    cancellationToken: cancellationToken);
            }
            else if (choiceValue == dialogsMUI.MainDictionary["yes"])
            {
                return await stepContext.CancelAllDialogsAsync(cancellationToken);
            }

            var feedback = dialogsMUI.FinishDictionary["feedback"]; // Type Your feedback, please:

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text(feedback), 
                Style = ListStyle.SuggestedAction
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
                _db.AddFeedback(feedback);
                _db.SaveChanges();
            }

            return await stepContext.CancelAllDialogsAsync(cancellationToken);
        }
    }
}
