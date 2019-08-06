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

        private ICollection<UserCourse> userSubscription;

        public EmailDialog(IDecisionMaker decisionMaker, ISubscriptionManager subscriptionManager, IEmailSender emailSender,
                             ThreadedLogger _myLogger,
                             DialogInfo dialogInfo,
                             ConcurrentDictionary<string, ConversationReference> conversationReferences, StuddyBotContext db,
                             DialogsMUI dialogsMui)
            : base(nameof(EmailDialog))
        {
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            _subscriptionManager = subscriptionManager;
            EmailSender = emailSender;
            _DialogInfo = dialogInfo;
            _db = db;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt("email", EmailFormValidator));
            AddDialog(new ChoicePrompt("validation", CodeValidator));
            AddDialog(new SubscriptionDialog(DecisionMaker, emailSender, subscriptionManager, _myLogger, dialogInfo, conversationReferences, db, dialogsMui));
            AddDialog(new ChooseOptionDialog(DecisionMaker, emailSender, subscriptionManager, _myLogger, dialogInfo, conversationReferences, db, dialogsMui));
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
                var msg = "Ви ще не додали email";
                var optionsAddEmail = new PromptOptions()
                {
                    Prompt = MessageFactory.Text("Хочете додати email?"),
                    RetryPrompt = MessageFactory.Text("Будь ласка, спробуйте ще раз"),
                    Choices = new List<Choice> { new Choice("так"), new Choice("ні") },
                };

                var messageAdd = msg + "\n" + optionsAddEmail.Prompt.Text;
                var senderAdd = "bot";
                var timeAdd = stepContext.Context.Activity.Timestamp.Value;

                _myLogger.LogMessage(messageAdd, senderAdd, timeAdd, _DialogInfo.DialogId);

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

                return await stepContext.PromptAsync(nameof(ChoicePrompt), optionsAddEmail, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Ваш email адрес **{userEmail}**"));

            var optionsManageEmail = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Що Ви хочете зробити?"),
                RetryPrompt = MessageFactory.Text("Будь ласка, спробуйте ще раз"),
                Choices = new List<Choice>
                {
                    new Choice("Змінити адрес"),
                    new Choice("Видалити пошту"),
                    new Choice("Назад")
                },
                Style = ListStyle.HeroCard
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

            switch (foundChoice)
            {
                case "ні":
                case "Назад":
                    return await stepContext.ReplaceDialogAsync(nameof(ChooseOptionDialog), "begin", cancellationToken);

                case "Змінити адрес":
                case "так":
                    validationCode = string.Empty;
                    var options = new PromptOptions()
                    {
                        Prompt = MessageFactory.Text("Введіть адресу:"),
                        RetryPrompt = MessageFactory.Text("Будь ласка, спробуйте ще раз"),
                        Style = ListStyle.HeroCard
                    };

                    var message = options.Prompt.Text;
                    var sender = "bot";
                    var time = stepContext.Context.Activity.Timestamp.Value;

                    _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

                    return await stepContext.PromptAsync("email", options, cancellationToken);

                case "Видалити пошту":
                    var optionsDeleteEmail = new PromptOptions()
                    {
                        Prompt = MessageFactory.Text("Ви справді хочете видалити?"),
                        RetryPrompt = MessageFactory.Text("Будь ласка, спробуйте ще раз"),
                        Choices = new List<Choice> { new Choice("так"), new Choice("ні") },
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

            switch (foundChoice)
            {
                case "ні":
                    return await stepContext.ReplaceDialogAsync(nameof(EmailDialog),
                        cancellationToken: cancellationToken);

                case "так":
                    _db.DeleteUserEmail(_DialogInfo);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Видалено"));
                    return await stepContext.ReplaceDialogAsync(nameof(EmailDialog),
                        cancellationToken: cancellationToken);

                default:
                    var message = GenerateCode();
                    userEmail = foundChoice;
                    await EmailSender.SendEmailAsync(foundChoice, "Validation Code", $"Your code: <b>{message}</b>.");


                    var options = new PromptOptions()
                    {
                        Prompt = MessageFactory.Text("Будь ласка, введіть код який я відправив Вам на пошту"),
                        RetryPrompt = MessageFactory.Text("Будь ласка, спробуйте ще раз"),
                        Choices = new List<Choice> { new Choice("Назад") }
                    };


                    return await stepContext.PromptAsync("validation", options, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if(stepContext.Context.Activity.Text == "Назад")
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