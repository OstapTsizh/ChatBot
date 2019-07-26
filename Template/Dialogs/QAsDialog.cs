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
        private DialogInfo _DialogInfo;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        private Dictionary<string, List<string>> _QAs;


        public QAsDialog(IDecisionMaker decisionMaker, IEmailSender emailSender, ISubscriptionManager SubscriptionManager,
                             ThreadedLogger _myLogger, 
                             DialogInfo dialogInfo, 
                             ConcurrentDictionary<string, ConversationReference> conversationReferences, StuddyBotContext db)
            : base(nameof(QAsDialog))
        {
            
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            _DialogInfo = dialogInfo;
            _conversationReferences = conversationReferences;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new AddQuestionDialog(DecisionMaker,emailSender, _myLogger, dialogInfo, conversationReferences, db));
            AddDialog(new FinishDialog(DecisionMaker, emailSender, SubscriptionManager, _myLogger, dialogInfo, conversationReferences, db));
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
            _QAs = DecisionMaker.GetQAs(_DialogInfo.Language);

            var choices = new List<Choice>();

            foreach (var q in _QAs.Keys)
            {
                choices.Add(new Choice(q));
            }

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Choose a question:"),
                Choices = choices,
                RetryPrompt = MessageFactory.Text("Try one more time, please:"),
                Style = ListStyle.HeroCard
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
            var choiceValue = (string)(stepContext.Result as FoundChoice).Value;

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
