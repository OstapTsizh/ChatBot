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
    public class MainDialog : CancelAndRestartDialog
    {
        protected readonly IConfiguration Configuration;
        protected readonly IDecisionMaker DecisionMaker;
        protected readonly IEmailSender EmailSender;
        protected QuestionAndAnswerModel _QuestionAndAnswerModel;
        protected DecisionModel _DecisionModel;
        protected ThreadedLogger _Logger;
        //protected DialogInfo _DialogInfo; // static
        private IStatePropertyAccessor<DialogInfo> _dialogInfoStateProperty;

        private StuddyBotContext _db;
        
        /// <summary>
        /// conversations with users which subscribed to notifications
        /// TODO: possible renaming needed
        /// </summary>
        //private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;        
        

        public MainDialog(UserState state, IStatePropertyAccessor<DialogInfo> dialogInfoStateProperty,
            IConfiguration configuration, IDecisionMaker decisionMaker, 
            ISubscriptionManager subscriptionManager, IEmailSender emailSender,
            ThreadedLogger Logger, 
            //ConcurrentDictionary<string, ConversationReference> conversationReferences, 
            StuddyBotContext db
            ) // DialogInfo dialogInfo, StuddyBotContext db
            : base(nameof(MainDialog), dialogInfoStateProperty, Logger)         
        {
            Configuration = configuration;
            this._Logger = Logger;    
            DecisionMaker = decisionMaker;
            EmailSender = emailSender;
            //_conversationReferences = conversationReferences;

            _QuestionAndAnswerModel = new QuestionAndAnswerModel();
            _QuestionAndAnswerModel.QuestionModel = new QuestionModel();
            _QuestionAndAnswerModel.QuestionModel.Questions=new List<Question>();
            _QuestionAndAnswerModel.QuestionModel.Decisions=new List<DecisionModel>();
            _QuestionAndAnswerModel.Answers = new List<string>();
            _DecisionModel = new DecisionModel();
            //_DialogInfo = new DialogInfo(); //dialogInfo;
            _dialogInfoStateProperty = dialogInfoStateProperty; //state.CreateProperty<DialogInfo>(nameof(DialogInfo));
            //_DialogInfo = _dialogInfoState.GetAsync();
            
            _db = db;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new LanguageDialog(_dialogInfoStateProperty, DecisionMaker, subscriptionManager, _Logger,
                EmailSender, db, Configuration));
            AddDialog(new LocationDialog(_dialogInfoStateProperty, DecisionMaker, subscriptionManager, _Logger,

                EmailSender, db));
            
            AddDialog(new MailingDialog(_dialogInfoStateProperty, DecisionMaker, emailSender, subscriptionManager,
                _Logger, db));


            AddDialog(new EmailDialog(_dialogInfoStateProperty, decisionMaker, subscriptionManager, emailSender,
                Logger,
                //_DialogInfo,
                //conversationReferences, 
                db
                ));
            AddDialog(new HelpDialog(_dialogInfoStateProperty, DecisionMaker, subscriptionManager, _Logger,

                EmailSender, db));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                StartLanguageDialogAsync                
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
            //StartNotificationService();
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


        private async Task<DialogTurnResult> StartLanguageDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context, () => null);
            await _dialogInfoStateProperty.SetAsync(stepContext.Context, _DialogInfo);
            
            return await stepContext.BeginDialogAsync(nameof(LanguageDialog), cancellationToken: cancellationToken);
        }
    }
}
