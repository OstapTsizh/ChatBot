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
using Microsoft.VisualBasic;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

namespace StuddyBot.Dialogs
{
    /// <summary>
    /// This dialog is responsible for the communication about mailing
    /// information to the user's email.
    /// </summary>
    public class MailingDialog : ComponentDialog// CancelAndRestartDialog
    {
        private readonly IDecisionMaker DecisionMaker;
        private readonly ThreadedLogger _myLogger;
        private DialogInfo _DialogInfo;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        



        public MailingDialog(IDecisionMaker decisionMaker, 
                             ThreadedLogger _myLogger, 
                             DialogInfo dialogInfo, 
                             ConcurrentDictionary<string, ConversationReference> conversationReferences)
            : base(nameof(MailingDialog))
        {
            
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            _DialogInfo = dialogInfo;
            _conversationReferences = conversationReferences;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            
            // ToDo Email validator to check email
            AddDialog(new TextPrompt("email"));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new FinishDialog(DecisionMaker, _myLogger, dialogInfo, conversationReferences));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AskSendToEmailStepAsync,
                AskEmailStepAsync,
                SendDialogStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Asks the user does he want to receive the dialog to email.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> AskSendToEmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var msg = "Хочете отримати діалог на email?";// "Do you want to receive the dialog to email?";
            
            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text(msg),
                Style = ListStyle.HeroCard
            };

            var message = options.Prompt.Text;
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;

            _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), options, cancellationToken);
        }

        /// <summary>
        /// Asks an email address from the user.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> AskEmailStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var msg = "Будь ласка, введіть свій email:";// "Enter Your email, please:";
                var retryMsg = "Будь ласка, спробуйте ще раз:";// "Try one more time, please:";

                var options = new PromptOptions()
                {
                    Prompt = MessageFactory.Text(msg),
                    RetryPrompt = MessageFactory.Text(retryMsg),
                    Style = ListStyle.HeroCard
                };

                var message = options.Prompt.Text;
                var sender = "bot";
                var time = stepContext.Context.Activity.Timestamp.Value;

                _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);
                return await stepContext.PromptAsync("email", options, cancellationToken);
            }

            return await stepContext.ReplaceDialogAsync(nameof(FinishDialog),
                cancellationToken: cancellationToken);
        }


        /// <summary>
        /// Sends the dialog if the user wants.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> SendDialogStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var userEmail = stepContext.Result.ToString();

            // ToDo the method that sends dialog
            
            return await stepContext.ReplaceDialogAsync(nameof(FinishDialog),
                cancellationToken: cancellationToken);
        }
    }
}
