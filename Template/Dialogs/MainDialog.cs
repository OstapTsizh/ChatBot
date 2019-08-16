// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

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
using Microsoft.Extensions.Configuration;
using Services.NotificationService;
using StuddyBot.Core.BLL.Interfaces;
using StuddyBot.Core.DAL.Data;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

namespace StuddyBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly IConfiguration Configuration;
        protected readonly IDecisionMaker DecisionMaker;
        protected readonly IEmailSender EmailSender;
        protected QuestionAndAnswerModel _QuestionAndAnswerModel;
        protected DecisionModel _DecisionModel;
        protected ThreadedLogger _Logger;
        protected DialogInfo _DialogInfo; // static
        private IStatePropertyAccessor<DialogInfo> _dialogInfoStateProperty;

        private StuddyBotContext _db;
        
        /// <summary>
        /// conversations with users which subscribed to notifications
        /// TODO: possible renaming needed
        /// </summary>
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;        
        

        public MainDialog(UserState state, IConfiguration configuration, IDecisionMaker decisionMaker, ISubscriptionManager subscriptionManager, IEmailSender emailSender,
            ThreadedLogger Logger, ConcurrentDictionary<string, ConversationReference> conversationReferences, StuddyBotContext db) // DialogInfo dialogInfo, StuddyBotContext db
            : base(nameof(MainDialog))         
        {
            Configuration = configuration;
            this._Logger = Logger;    
            DecisionMaker = decisionMaker;
            EmailSender = emailSender;
            _conversationReferences = conversationReferences;

            _QuestionAndAnswerModel = new QuestionAndAnswerModel();
            _QuestionAndAnswerModel.QuestionModel = new QuestionModel();
            _QuestionAndAnswerModel.QuestionModel.Questions=new List<Question>();
            _QuestionAndAnswerModel.QuestionModel.Decisions=new List<DecisionModel>();
            _QuestionAndAnswerModel.Answers = new List<string>();
            _DecisionModel = new DecisionModel();
            _DialogInfo = new DialogInfo(); //dialogInfo;
            _dialogInfoStateProperty= state.CreateProperty<DialogInfo>(nameof(DialogInfo));
            //_DialogInfo = _dialogInfoState.GetAsync();
            
            _db = db;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new LocationDialog(_dialogInfoStateProperty, DecisionMaker, subscriptionManager, _Logger, _DialogInfo, _conversationReferences, db, EmailSender));
            //AddDialog(new MainMenuDialog(DecisionMaker, _Logger, _DialogInfo, _conversationReferences));
            //AddDialog(new MailingDialog(DecisionMaker, _Logger, _DialogInfo, _conversationReferences));
            //AddDialog(new CoursesDialog(DecisionMaker, _Logger, _DialogInfo, _conversationReferences));
            // AddDialog(new AddQuestionDialog(DecisionMaker, _Logger, dialogInfo, conversationReferences));


            //AddDialog(new LoopingDialog(DecisionMaker, _QuestionAndAnswerModel, _Logger, _DialogInfo, _conversationReferences, _db));

            ////AddDialog(new LocationDialog(DecisionMaker, _Logger, _DialogInfo, _conversationReferences, db));
            ////AddDialog(new MainMenuDialog(DecisionMaker, _Logger, _DialogInfo, _conversationReferences, db));


            AddDialog(new MailingDialog(_dialogInfoStateProperty, DecisionMaker, emailSender, subscriptionManager, _Logger, _DialogInfo, _conversationReferences, db));


            ////AddDialog(new CoursesDialog(DecisionMaker, _Logger, _DialogInfo, _conversationReferences, db));


            //AddDialog(new SubscriptionDialog(decisionMaker, subscriptionManager, Logger, dialogInfo,
            //    conversationReferences));


            AddDialog(new EmailDialog(_dialogInfoStateProperty, decisionMaker, subscriptionManager, emailSender, Logger, _DialogInfo,
                conversationReferences, db));


            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CheckLanguageStepAsync,
                ChangeLanguageStepAsync,
                StartLocationDialogAsync,
                //StartLoopingDialogAsync
            }));


            StartServices();

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Starts all services
        /// </summary>
        private void StartServices()
        {
            StartNotificationService();
        }

        /// <summary>
        /// Starts the service responsible for users notification
        /// who is subscribed for some course.
        /// </summary>
        private void StartNotificationService()
        {
            var service = new NotificationService();           

            var startServiceThread = new Thread(service.StartService)
            {
                IsBackground = true
            };

            startServiceThread.Start();
        }
        

        private async Task<DialogTurnResult> CheckLanguageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context, () => null);//new DialogState()
            
            try
            {
                _DialogInfo.DialogId = _Logger.LogDialog(_DialogInfo.UserId).Result;
            }
            catch (Exception e)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Error on creating user in DB" +
                    $"{e.InnerException}"),
                    cancellationToken: cancellationToken);
            }
            try
            {
                var languageTest = "en-us";// stepContext.Context.Activity.Locale.ToLower();
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(languageTest+" - hardcode"),
                    cancellationToken: cancellationToken);
                _DialogInfo.Language = languageTest;
            }
            catch
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Error on getting user's language from activity"),
                    cancellationToken: cancellationToken);
            }

            try
            {
                _DialogInfo.UserId = _Logger.LogUser(stepContext.Context.Activity.From.Id, _DialogInfo.Language).Result;
            }
            catch
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Error on getting user's ID from DB"),
                    cancellationToken: cancellationToken);
            }

            //_DialogInfo.DialogId = _Logger.LogDialog(_DialogInfo.UserId).Result;
            //_DialogInfo.Language = stepContext.Context.Activity.Locale.ToLower();
            //_DialogInfo.UserId = _Logger.LogUser(stepContext.Context.Activity.From.Id, _DialogInfo.Language).Result;

            string language;

            switch (_DialogInfo.Language)
            {
                case "ru-ru":
                    language = "Русский";
                    break;
                case "uk-ua":
                    language = "Українська";
                    break;
                default:
                    language = "English";
                    break;
            }

            var choices = new List<Choice>();
            {
                choices.Add(new Choice("Yes"));
                choices.Add(new Choice("No"));
            }

            var msg = $"Is your language {language}?";

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text(msg),
                Choices = choices,
                Style = ListStyle.SuggestedAction
            };

            var message = options.Prompt.Text;
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;
            
            _Logger.LogMessage(message, sender, time, _DialogInfo.DialogId);

            await _dialogInfoStateProperty.SetAsync(stepContext.Context, _DialogInfo);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options,
                cancellationToken: cancellationToken);
        }


        private async Task<DialogTurnResult> ChangeLanguageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);

            var choiceValue = (string)(stepContext.Result as FoundChoice).Value;

            if (choiceValue == "No")
            {
                var choices = new List<Choice>();
                {
                    choices.Add(new Choice("Ukrainian"));
                    choices.Add(new Choice("Russian"));
                    choices.Add(new Choice("English"));
                }

                var msg = "Choose your language:";

                var promptOptions = new PromptOptions()
                {
                    Prompt = MessageFactory.Text(msg),
                    Choices = choices,
                    Style = ListStyle.SuggestedAction
                };

                var promptMessage = promptOptions.Prompt.Text;
                var promptSender = "bot";
                var promptTime = stepContext.Context.Activity.Timestamp.Value;

                _Logger.LogMessage(promptMessage, promptSender, promptTime, _DialogInfo.DialogId);

                return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions,
                    cancellationToken: cancellationToken);
            }

            //await _dialogInfoStateProperty.SetAsync(stepContext.Context, _DialogInfo);

            return await stepContext.NextAsync(cancellationToken:cancellationToken);
        }

        
        /// <summary>
        /// Starts child (Location) dialog after retrieving UserId
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> StartLocationDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //_DialogInfo.Language = Configuration.GetValue<string>("LanguageForNewUser"); //"uk-ua";

            //_DialogInfo.UserId = _Logger.LogUser(stepContext.Context.Activity.From.Id, _DialogInfo.Language).Result;

            //_DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);

            var choiceValue = (stepContext.Result as FoundChoice)?.Value;

            if (!string.IsNullOrEmpty(choiceValue))
            {
                switch (choiceValue)
                {
                    case "Russian":
                        _DialogInfo.Language = "ru-ru";
                        break;
                    case "Ukrainian":
                        _DialogInfo.Language = "uk-ua";
                        break;
                    default:
                        _DialogInfo.Language = "en-us";
                        break;
                }
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Getting DialogsMUI"),
                    cancellationToken: cancellationToken);
            try
            {
                DecisionMaker.GetDialogsMui(_DialogInfo.Language);
            }
            catch
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Error on getting DialogsMUI"),
                        cancellationToken: cancellationToken);
            }
            try
            {
                _db.User.First(user => user.Id == _DialogInfo.UserId).Language = _DialogInfo.Language;
                _db.SaveChanges();
            }
            catch
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Error on saving user's Language to DB" +
                    $"\nuserID: {_DialogInfo.UserId}\nlang: {_DialogInfo.Language}"),
                    cancellationToken: cancellationToken);
            }

            await _dialogInfoStateProperty.SetAsync(stepContext.Context, _DialogInfo);

            return await stepContext.BeginDialogAsync(nameof(LocationDialog), cancellationToken:cancellationToken);
        }

        /// <summary>
        /// Starts child (Looping) dialog after retrieving UserId
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> StartLoopingDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _DialogInfo.UserId = _Logger.LogUser(stepContext.Context.Activity.From.Id, _DialogInfo.Language).Result;

            // assigning conversation reference
            // TODO: Serialize to string and insert to db.            
            _db.User.Find(stepContext.Context.Activity.From.Id).ConversationReference = stepContext.Context.Activity.GetConversationReference().Conversation.Id;
            await _db.SaveChangesAsync();

            return await stepContext.BeginDialogAsync(nameof(LoopingDialog), "begin", cancellationToken);
        }        

        
        protected override Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_DialogInfo.DialogId != 0) {

                LogMessage(innerDc.Context.Activity.Text, innerDc.Context.Activity.Timestamp.Value);
            }
            return base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        /// <summary>
        /// Logs message from a user.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="time"></param>
        private void LogMessage(string message, DateTimeOffset time)
        {
            _Logger.LogMessage(message, "user", time, _DialogInfo.DialogId);
        }



       

    }

}
