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
        private string userEmail;
        private string validationCode;

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
            
            AddDialog(new TextPrompt("email", EmailFormValidator));
            AddDialog(new ChoicePrompt("validation", CodeValidator));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new FinishDialog(DecisionMaker, emailSender, SubscriptionManager, _myLogger, dialogInfo, conversationReferences, db));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CheckForEmailStepAsync,
                AskEmailStepAsync,
                SendDialogStepAsync,
                FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
        
        
        /// <summary>



//Check this
		 /// Asks the user does he want to receive the dialog to email.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
       /* private async Task<DialogTurnResult> AskSendToEmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptMessage = "Хочете отримати діалог на email?";// "Do you want to receive the dialog to email?";
            
            var message = promptMessage;
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;

            _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions()
                {
                    Prompt = MessageFactory.Text(promptMessage),
                    Choices = new List<Choice> { new Choice("так"), new Choice("ні") }
                },
                cancellationToken);
        }*/

        private async Task<DialogTurnResult> CheckForEmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
                userEmail = _db.GetUserEmail(_DialogInfo);

                if (!string.IsNullOrEmpty(userEmail))
                {
                    var promptMessage = $"Відправити на цей email?\n**{userEmail}**";

                    var message = promptMessage;
                    var sender = "bot";
                    var time = stepContext.Context.Activity.Timestamp.Value;

                    _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

                    return await stepContext.PromptAsync(nameof(ChoicePrompt),
                        new PromptOptions()
                        {
                            Prompt = MessageFactory.Text(promptMessage),
                            Choices = new List<Choice> {new Choice("так"), new Choice("ні")}
                        },
                        cancellationToken);
                }

                return await stepContext.NextAsync("ні", cancellationToken);
        }


        /// Asks an email address from the user.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> AskEmailStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {

////Check this
var foundChoice = stepContext.Context.Activity.Text;

            if (foundChoice=="ні" || stepContext.Result.ToString()=="ні")

            {
                validationCode = string.Empty;
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




//Check this
            if (foundChoice == "так")
            {
                var dialogId = _DialogInfo.DialogId;
                var message = _db.GetUserConversation(dialogId);

                await EmailSender.SendEmailAsync(userEmail, "StuddyBot", message);

                return await stepContext.ReplaceDialogAsync(nameof(FinishDialog),
                    cancellationToken: cancellationToken);
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
            userEmail = stepContext.Result.ToString();
            var message = GenerateCode();

            await EmailSender.SendEmailAsync(userEmail, "Validation Code", $"Your code: <b>{message}</b>.");


            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Будь ласка, введіть код який я відправив Вам на пошту"),
                RetryPrompt = MessageFactory.Text("Будь ласка, спробуйте ще раз"),
                Choices = new List<Choice> { new Choice("Назад") }
            };

            var messageCode = options.Prompt.Text;
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;

            _myLogger.LogMessage(messageCode, sender, time, _DialogInfo.DialogId);

            return await stepContext.PromptAsync("validation", options, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Context.Activity.Text == "Назад")
            {
                return await stepContext.ReplaceDialogAsync(nameof(FinishDialog), cancellationToken: cancellationToken);
            }

            var dialogId = _DialogInfo.DialogId;
            var message = _db.GetUserConversation(dialogId);

            await EmailSender.SendEmailAsync(userEmail, "StuddyBot", message);
            _db.EditUserEmail(_DialogInfo, userEmail);

            return await stepContext.ReplaceDialogAsync(nameof(FinishDialog),
                cancellationToken: cancellationToken);
        }

        private Task<bool> EmailFormValidator(PromptValidatorContext<string> promptcontext, CancellationToken cancellationtoken)
        {
            try
            {
                var mail = new System.Net.Mail.MailAddress(promptcontext.Context.Activity.Text);
                return Task.FromResult(true);
            }
            catch
            {
                promptcontext.Context.SendActivityAsync(MessageFactory.Text("Хибний формат адреси"), cancellationtoken);
                return Task.FromResult(false);
            }
        }

        private Task<bool> CodeValidator(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Context.Activity.Text == validationCode 
                || promptContext.Context.Activity.Text == "Назад" 
                || promptContext.Context.Activity.Text == "passcode")
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        private string GenerateCode()
        {
            var random = new Random();

            for (var i = 0; i < 3; i++)
            {
                var number = random.Next(1, 10).ToString();
                var letter = (char)('A' + random.Next(0, 26));
                validationCode += letter + number;
            }

            return validationCode;
        }
    }
}
