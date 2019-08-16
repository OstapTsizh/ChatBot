using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
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
    public class EmailDialog : ComponentDialog
    {
        private DecisionModel _DecisionModel;
        private readonly IDecisionMaker DecisionMaker;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly ThreadedLogger _myLogger;
        private DialogInfo _DialogInfo;
        private StuddyBotContext _db;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private IEmailSender EmailSender;
        private string userEmail;
        private string validationCode;
        private IStatePropertyAccessor<DialogInfo> _dialogInfoStateProperty;

        private ICollection<UserCourse> userSubscription;

        public EmailDialog(IStatePropertyAccessor<DialogInfo> dialogInfoStateProperty, IDecisionMaker decisionMaker, ISubscriptionManager subscriptionManager, IEmailSender emailSender,
                             ThreadedLogger _myLogger,
                             DialogInfo dialogInfo,
                             ConcurrentDictionary<string, ConversationReference> conversationReferences, StuddyBotContext db)
            : base(nameof(EmailDialog))
        {
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            _subscriptionManager = subscriptionManager;
            EmailSender = emailSender;
            _DialogInfo = dialogInfo;
            _db = db;
            _dialogInfoStateProperty = dialogInfoStateProperty;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt("email", EmailFormValidator));
            AddDialog(new ChoicePrompt("validation", CodeValidator));
            AddDialog(new SubscriptionDialog(dialogInfoStateProperty, DecisionMaker, emailSender, subscriptionManager, _myLogger, dialogInfo, conversationReferences, db));
            AddDialog(new ChooseOptionDialog(dialogInfoStateProperty, DecisionMaker, emailSender, subscriptionManager, _myLogger, dialogInfo, conversationReferences, db));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                FirstStepAsync,
                ChoiceStepAsync,
                PreFinalStepAsync,
                FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> FirstStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            userEmail = _db.GetUserEmail(_DialogInfo);

            if (string.IsNullOrEmpty(userEmail))
            {
                var msg = DialogsMUI.EmailDictionary["msg"];
                var prompt = DialogsMUI.EmailDictionary["prompt"];//Хочете додати email?
                var reprompt = DialogsMUI.MainDictionary["reprompt"];
                var yes = DialogsMUI.MainDictionary["yes"];
                var no = DialogsMUI.MainDictionary["no"];

                var optionsAddEmail = new PromptOptions()
                {
                    Prompt = MessageFactory.Text(prompt),
                    RetryPrompt = MessageFactory.Text(reprompt),
                    Choices = new List<Choice> { new Choice(yes), new Choice(no) },
                };

                var messageAdd = msg + "\n" + optionsAddEmail.Prompt.Text;
                var senderAdd = "bot";
                var timeAdd = stepContext.Context.Activity.Timestamp.Value;

                _myLogger.LogMessage(messageAdd, senderAdd, timeAdd, _DialogInfo.DialogId);

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

                return await stepContext.PromptAsync(nameof(ChoicePrompt), optionsAddEmail, cancellationToken);
            }

            var currentEmail = DialogsMUI.EmailDictionary["currentEmail"];
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{currentEmail} **{userEmail}**"));

            var promptNext = DialogsMUI.EmailDictionary["promptNext"]; //Що Ви хочете зробити?
            var changeEmail = DialogsMUI.EmailDictionary["changeEmail"]; //Змінити адресу
            var deleteEmail = DialogsMUI.EmailDictionary["deleteEmail"]; //Видалити пошту
            var back = DialogsMUI.EmailDictionary["back"]; //Назад

            var optionsManageEmail = new PromptOptions()
            {
                Prompt = MessageFactory.Text(promptNext),
                RetryPrompt = MessageFactory.Text(DialogsMUI.MainDictionary["reprompt"]),
                Choices = new List<Choice>
                {
                    new Choice(changeEmail),
                    new Choice(deleteEmail),
                    new Choice(back)
                },
                Style = ListStyle.SuggestedAction
            };

            var message = optionsManageEmail.Prompt.Text;
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;

            _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), optionsManageEmail, cancellationToken);
        }

        private async Task<DialogTurnResult> ChoiceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var foundChoice = stepContext.Context.Activity.Text;

            var promptAddress = DialogsMUI.EmailDictionary["promptAddress"]; //Введіть адресу:
            var promptDelete = DialogsMUI.EmailDictionary["promptDelete"]; //Ви справді хочете видалити?
            var reprompt = DialogsMUI.MainDictionary["reprompt"];
            var yes = DialogsMUI.MainDictionary["yes"];
            var no = DialogsMUI.MainDictionary["no"];

            if (foundChoice == DialogsMUI.MainDictionary["no"] || foundChoice == DialogsMUI.EmailDictionary["back"])
            {
                return await stepContext.ReplaceDialogAsync(nameof(ChooseOptionDialog), "begin", cancellationToken);
            }
            else if (foundChoice == DialogsMUI.EmailDictionary["changeEmail"] || foundChoice == DialogsMUI.MainDictionary["yes"])
            {
                validationCode = string.Empty;
                var options = new PromptOptions()
                {
                    Prompt = MessageFactory.Text(promptAddress),
                    RetryPrompt = MessageFactory.Text(reprompt),
                    Style = ListStyle.SuggestedAction
                };

                var message = options.Prompt.Text;
                var sender = "bot";
                var time = stepContext.Context.Activity.Timestamp.Value;

                _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

                return await stepContext.PromptAsync("email", options, cancellationToken);
            }
            else if (foundChoice == DialogsMUI.EmailDictionary["deleteEmail"])
            {
                var optionsDeleteEmail = new PromptOptions()
                {
                    Prompt = MessageFactory.Text(promptDelete),
                    RetryPrompt = MessageFactory.Text(reprompt),
                    Choices = new List<Choice> {new Choice(yes), new Choice(no)},
                };

                var messageDelete = optionsDeleteEmail.Prompt.Text;
                var senderDelete = "bot";
                var timeDelete = stepContext.Context.Activity.Timestamp.Value;

                _myLogger.LogMessage(messageDelete, senderDelete, timeDelete, _DialogInfo.DialogId);

                return await stepContext.PromptAsync(nameof(ChoicePrompt), optionsDeleteEmail, cancellationToken);
            }

            return await stepContext.ReplaceDialogAsync(nameof(ChooseOptionDialog), "begin", cancellationToken);
        }

        private async Task<DialogTurnResult> PreFinalStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var foundChoice = stepContext.Context.Activity.Text;

            var promptCode = DialogsMUI.EmailDictionary["promptCode"];//Будь ласка, введіть код який я відправив Вам на пошту

            switch (foundChoice)
            {
                case "ні":
                    return await stepContext.ReplaceDialogAsync(nameof(EmailDialog),
                        cancellationToken: cancellationToken);

                case "так":
                    _db.DeleteUserEmail(_DialogInfo);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(DialogsMUI.EmailDictionary["deleted"]));
                    return await stepContext.ReplaceDialogAsync(nameof(EmailDialog),
                        cancellationToken: cancellationToken);

                default:
                    var message = GenerateCode();
                    userEmail = foundChoice;
                    await EmailSender.SendEmailAsync(foundChoice, DialogsMUI.EmailDictionary["subjectValidationCode"], $"{DialogsMUI.EmailDictionary["emailMessage"]} <b>{message}</b>.");


                    var options = new PromptOptions()
                    {
                        Prompt = MessageFactory.Text(promptCode),
                        RetryPrompt = MessageFactory.Text(DialogsMUI.MainDictionary["reprompt"]),
                        Choices = new List<Choice> { new Choice(DialogsMUI.EmailDictionary["back"]) }
                    };


                    return await stepContext.PromptAsync("validation", options, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if(stepContext.Context.Activity.Text == DialogsMUI.EmailDictionary["back"])
            {
                return await stepContext.ReplaceDialogAsync(nameof(EmailDialog), cancellationToken: cancellationToken);
            }

            _db.EditUserEmail(_DialogInfo, userEmail);
            return await stepContext.ReplaceDialogAsync(nameof(EmailDialog),
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
                promptcontext.Context.SendActivityAsync(MessageFactory.Text(DialogsMUI.EmailDictionary["wrongFormat"]), cancellationtoken);
                return Task.FromResult(false);
            }
        }

        private Task<bool> CodeValidator(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Context.Activity.Text == validationCode 
                || promptContext.Context.Activity.Text == DialogsMUI.EmailDictionary["back"]
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