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
    /// This dialog is responsible for the communication about questions/answers.
    /// </summary>
    public class QAsDialog : ComponentDialog// CancelAndRestartDialog
    {
        private readonly IDecisionMaker DecisionMaker;
        private readonly ThreadedLogger _myLogger;
        private IStatePropertyAccessor<DialogInfo> _dialogInfoStateProperty;


        public QAsDialog(IStatePropertyAccessor<DialogInfo> dialogInfoStateProperty, IDecisionMaker decisionMaker, IEmailSender emailSender, ISubscriptionManager SubscriptionManager,
                             ThreadedLogger _myLogger, 
                             //DialogInfo dialogInfo, 
                             //ConcurrentDictionary<string, ConversationReference> conversationReferences, 
                             StuddyBotContext db
                             )
            : base(nameof(QAsDialog))
        {
            
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            
            _dialogInfoStateProperty = dialogInfoStateProperty;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new AddQuestionDialog(dialogInfoStateProperty, DecisionMaker, emailSender, _myLogger, 
                db
                ));
            AddDialog(new FinishDialog(dialogInfoStateProperty, DecisionMaker, emailSender, SubscriptionManager,
                _myLogger,
                db
                ));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AskSelectQAStepAsync,
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
        private async Task<DialogTurnResult> AskSelectQAStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);
            _DialogInfo.LastDialogName = this.Id;

            var _QAs = DecisionMaker.GetQAs(_DialogInfo.Language);
            var dialogsMUI = DecisionMaker.GetDialogsMui(_DialogInfo.Language);
            var prompt = dialogsMUI.QAsDictionary["prompt"];
            var reprompt = dialogsMUI.QAsDictionary["reprompt"];

            var choices = new List<Choice>();

            foreach (var q in _QAs.Keys)
            {
                choices.Add(new Choice(q));
            }

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text(prompt), // Choose a question:
                Choices = choices,
                RetryPrompt = MessageFactory.Text(reprompt), // Try one more time, please:
                Style = ListStyle.SuggestedAction
            };

            var message = options.Prompt.Text;
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;

            _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        /// <summary>
        /// Shows an answer.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);
            var choiceValue = (string)(stepContext.Result as FoundChoice).Value;
            var _QAs = DecisionMaker.GetQAs(_DialogInfo.Language);
            var msg = MessageFactory.Text(string.Join("\n" ,_QAs[choiceValue]));
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;

            _myLogger.LogMessage(msg.Text, sender, time, _DialogInfo.DialogId);

            await stepContext.Context.SendActivityAsync(msg, cancellationToken);
            
            return await stepContext.ReplaceDialogAsync(nameof(ChooseOptionDialog), "qa",
                cancellationToken: cancellationToken);
        }
    }
}
