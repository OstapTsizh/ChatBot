// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        protected static DialogInfo _DialogInfo;

        private StuddyBotContext _db;

        /// <summary>
        /// conversations with users which subscribed to notifications
        /// TODO: possible renaming needed
        /// </summary>
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;        
        

        public MainDialog(IConfiguration configuration, IDecisionMaker decisionMaker, ISubscriptionManager subscriptionManager, IEmailSender emailSender,
            ThreadedLogger Logger, ConcurrentDictionary<string, ConversationReference> conversationReferences, DialogInfo dialogInfo, StuddyBotContext db)
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
            _DialogInfo = dialogInfo;

            _db = db;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new LocationDialog(DecisionMaker, _Logger, _DialogInfo, _conversationReferences, db));
            //AddDialog(new MainMenuDialog(DecisionMaker, _Logger, _DialogInfo, _conversationReferences));
            //AddDialog(new MailingDialog(DecisionMaker, _Logger, _DialogInfo, _conversationReferences));
            //AddDialog(new CoursesDialog(DecisionMaker, _Logger, _DialogInfo, _conversationReferences));
            // AddDialog(new AddQuestionDialog(DecisionMaker, _Logger, dialogInfo, conversationReferences));


            //AddDialog(new LoopingDialog(DecisionMaker, _QuestionAndAnswerModel, _Logger, _DialogInfo, _conversationReferences, _db));

            ////AddDialog(new LocationDialog(DecisionMaker, _Logger, _DialogInfo, _conversationReferences, db));
            ////AddDialog(new MainMenuDialog(DecisionMaker, _Logger, _DialogInfo, _conversationReferences, db));
            ////AddDialog(new MailingDialog(DecisionMaker, emailSender, _Logger, _DialogInfo, _conversationReferences, db));
            ////AddDialog(new CoursesDialog(DecisionMaker, _Logger, _DialogInfo, _conversationReferences, db));
            AddDialog(new SubscriptionDialog(decisionMaker, subscriptionManager, Logger, dialogInfo,
                conversationReferences));


            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
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

        /// <summary>
        /// Starts child (Location) dialog after retrieving UserId
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> StartLocationDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // ToDo language selection
            _DialogInfo.Language = "uk-ua";

            _DialogInfo.UserId = _Logger.LogUser(stepContext.Context.Activity.From.Id).Result;

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
            _DialogInfo.UserId = _Logger.LogUser(stepContext.Context.Activity.From.Id).Result;

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
