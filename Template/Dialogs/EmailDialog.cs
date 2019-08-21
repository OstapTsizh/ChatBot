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
        //private DialogInfo _DialogInfo;
        private StuddyBotContext _db;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private IEmailSender EmailSender;
        private string userEmail;
        private string validationCode;
        private IStatePropertyAccessor<DialogInfo> _dialogInfoStateProperty;

        private ICollection<UserCourse> userSubscription;

        public EmailDialog(IStatePropertyAccessor<DialogInfo> dialogInfoStateProperty, IDecisionMaker decisionMaker, ISubscriptionManager subscriptionManager, IEmailSender emailSender,
                             ThreadedLogger _myLogger,
                             //DialogInfo dialogInfo,
                             ConcurrentDictionary<string, ConversationReference> conversationReferences, StuddyBotContext db)
            : base(nameof(EmailDialog))
        {
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            _subscriptionManager = subscriptionManager;
            EmailSender = emailSender;
            //_DialogInfo = dialogInfo;
            _db = db;
            _dialogInfoStateProperty = dialogInfoStateProperty;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt("email", EmailFormValidator));
            AddDialog(new ChoicePrompt("validation", CodeValidator));
            AddDialog(new SubscriptionDialog(dialogInfoStateProperty, DecisionMaker, emailSender, subscriptionManager, _myLogger,
                //dialogInfo,
                conversationReferences, db));
            AddDialog(new ChooseOptionDialog(dialogInfoStateProperty, DecisionMaker, emailSender, subscriptionManager, _myLogger,
                //dialogInfo,
                conversationReferences, db));
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
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);
            _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);

            userEmail = _db.GetUserEmail(_DialogInfo);
            var dialogsMUI = DecisionMaker.GetDialogsMui(_DialogInfo.Language);
            if (string.IsNullOrEmpty(userEmail))
            {
                var msg = dialogsMUI.EmailDictionary["msg"];
                var prompt = dialogsMUI.EmailDictionary["prompt"];//Хочете додати email?
                var reprompt = dialogsMUI.MainDictionary["reprompt"];
                var yes = dialogsMUI.MainDictionary["yes"];
                var no = dialogsMUI.MainDictionary["no"];

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

            var currentEmail = dialogsMUI.EmailDictionary["currentEmail"];
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{currentEmail} **{userEmail}**"));

            var promptNext = dialogsMUI.EmailDictionary["promptNext"]; //Що Ви хочете зробити?
            var changeEmail = dialogsMUI.EmailDictionary["changeEmail"]; //Змінити адресу
            var deleteEmail = dialogsMUI.EmailDictionary["deleteEmail"]; //Видалити пошту
            var back = dialogsMUI.EmailDictionary["back"]; //Назад

            var optionsManageEmail = new PromptOptions()
            {
                Prompt = MessageFactory.Text(promptNext),
                RetryPrompt = MessageFactory.Text(dialogsMUI.MainDictionary["reprompt"]),
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
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);
            var foundChoice = stepContext.Context.Activity.Text;
            var dialogsMUI = DecisionMaker.GetDialogsMui(_DialogInfo.Language);
            var promptAddress = dialogsMUI.EmailDictionary["promptAddress"]; //Введіть адресу:
            var promptDelete = dialogsMUI.EmailDictionary["promptDelete"]; //Ви справді хочете видалити?
            var reprompt = dialogsMUI.MainDictionary["reprompt"];
            var yes = dialogsMUI.MainDictionary["yes"];
            var no = dialogsMUI.MainDictionary["no"];

            if (foundChoice == dialogsMUI.MainDictionary["no"] || foundChoice == dialogsMUI.EmailDictionary["back"])
            {
                return await stepContext.ReplaceDialogAsync(nameof(ChooseOptionDialog), "begin", cancellationToken);
            }
            else if (foundChoice == dialogsMUI.EmailDictionary["changeEmail"] || foundChoice == dialogsMUI.MainDictionary["yes"])
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
            else if (foundChoice == dialogsMUI.EmailDictionary["deleteEmail"])
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
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);
            var foundChoice = stepContext.Context.Activity.Text;
            var dialogsMUI = DecisionMaker.GetDialogsMui(_DialogInfo.Language);
            var promptCode = dialogsMUI.EmailDictionary["promptCode"];//Будь ласка, введіть код який я відправив Вам на пошту

            switch (foundChoice)
            {
                case "ні":
                    return await stepContext.ReplaceDialogAsync(nameof(EmailDialog),
                        cancellationToken: cancellationToken);

                case "так":
                    _db.DeleteUserEmail(_DialogInfo);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(dialogsMUI.EmailDictionary["deleted"]));
                    return await stepContext.ReplaceDialogAsync(nameof(EmailDialog),
                        cancellationToken: cancellationToken);

                default:
                    var message = GenerateCode();
                    userEmail = foundChoice;
                    await EmailSender.SendEmailAsync(foundChoice, dialogsMUI.EmailDictionary["subjectValidationCode"], $"{dialogsMUI.EmailDictionary["emailMessage"]} <b>{message}</b>.");


                    var options = new PromptOptions()
                    {
                        Prompt = MessageFactory.Text(promptCode),
                        RetryPrompt = MessageFactory.Text(dialogsMUI.MainDictionary["reprompt"]),
                        Choices = new List<Choice> { new Choice(dialogsMUI.EmailDictionary["back"]) }
                    };


                    return await stepContext.PromptAsync("validation", options, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);
            var dialogsMUI = DecisionMaker.GetDialogsMui(_DialogInfo.Language);
            if (stepContext.Context.Activity.Text == dialogsMUI.EmailDictionary["back"])
            {
                return await stepContext.ReplaceDialogAsync(nameof(EmailDialog), cancellationToken: cancellationToken);
            }

            _db.EditUserEmail(_DialogInfo, userEmail);
            return await stepContext.ReplaceDialogAsync(nameof(EmailDialog),
                cancellationToken: cancellationToken);
        }

        private async Task<bool> EmailFormValidator(PromptValidatorContext<string> promptcontext, CancellationToken cancellationtoken)
        {
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(promptcontext.Context);
            var dialogsMUI = DecisionMaker.GetDialogsMui(_DialogInfo.Language);
            try
            {
                var mail = new System.Net.Mail.MailAddress(promptcontext.Context.Activity.Text);
                return await Task.FromResult(true);
            }
            catch
            {
                promptcontext.Context.SendActivityAsync(MessageFactory.Text(dialogsMUI.EmailDictionary["wrongFormat"]), cancellationtoken);
                return await Task.FromResult(false);
            }
        }

        private async Task<bool> CodeValidator(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(promptContext.Context);
            var dialogsMUI = DecisionMaker.GetDialogsMui(_DialogInfo.Language);
            if (promptContext.Context.Activity.Text == validationCode 
                || promptContext.Context.Activity.Text == dialogsMUI.EmailDictionary["back"]
                || promptContext.Context.Activity.Text == "passcode")
            {
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
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