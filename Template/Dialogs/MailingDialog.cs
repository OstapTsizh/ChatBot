using System;
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
using Microsoft.VisualBasic;
using StuddyBot.Core.DAL.Data;
using StuddyBot.Core.DAL.Entities;
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
        private readonly IEmailSender EmailSender;
        private readonly ThreadedLogger _myLogger;
        private StuddyBotContext _db;
        private DialogInfo _DialogInfo;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;




        public MailingDialog(IDecisionMaker decisionMaker, IEmailSender emailSender, ISubscriptionManager SubscriptionManager,
                             ThreadedLogger _myLogger, 
                             DialogInfo dialogInfo, 
                             ConcurrentDictionary<string, ConversationReference> conversationReferences, StuddyBotContext db)
            : base(nameof(MailingDialog))
        {
            
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            EmailSender = emailSender;
            _DialogInfo = dialogInfo;
            _conversationReferences = conversationReferences;
            _db = db;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            
            // ToDo Email validator to check email
            AddDialog(new TextPrompt("email"));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new FinishDialog(DecisionMaker, emailSender, SubscriptionManager, _myLogger, dialogInfo, conversationReferences, db));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AskEmailStepAsync,
                SendDialogStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
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
            //if ((bool)stepContext.Result)
            {
                var msg = "Ѕудь ласка, введ≥ть св≥й email:";// "Enter Your email, please:";
                var retryMsg = "Ѕудь ласка, спробуйте ще раз:";// "Try one more time, please:";

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

            var dialogId = _DialogInfo.DialogId;
            var message = GetUserConversation(dialogId);

            await EmailSender.SendEmailAsync(userEmail, "StuddyBot", message);

            return await stepContext.ReplaceDialogAsync(nameof(FinishDialog),
                cancellationToken: cancellationToken);
        }

        private string GetUserConversation(int dialogId)
        {
            var text = "Hi, your conversation with StuddyBot";
            var conversation = _db.Dialog.Where(d => d.DialogsId == dialogId).ToList();          
            foreach (var message in conversation)
            {
                text += $"<br/>From: {message.Sender} <br/> &nbsp; {message.Message} &emsp; {message.Time.DateTime}<br/><hr/>";
            }

            return text;
        }
    }
}
