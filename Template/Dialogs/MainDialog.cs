// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LoggerService;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Services.NotificationService;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

namespace StuddyBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly IConfiguration Configuration;
        protected readonly IDecisionMaker DecisionMaker;
        protected QuestionAndAnswerModel _QuestionAndAnswerModel;
        protected DecisionModel _DecisionModel;
        protected ThreadedLogger _Logger;
        protected static DialogInfo _DialogInfo;

        /// <summary>
        /// conversations with users which subscribed to notifications
        /// TODO: possible renaming needed
        /// </summary>
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;        
        

        public MainDialog(IConfiguration configuration, IDecisionMaker decisionMaker,
            ThreadedLogger Logger, ConcurrentDictionary<string, ConversationReference> conversationReferences, DialogInfo dialogInfo)
            : base(nameof(MainDialog))         
        {
            Configuration = configuration;
            this._Logger = Logger;    
            DecisionMaker = decisionMaker;
            _conversationReferences = conversationReferences;

            _QuestionAndAnswerModel = new QuestionAndAnswerModel();
            _QuestionAndAnswerModel.QuestionModel = new QuestionModel();
            _QuestionAndAnswerModel.Answers = new List<string>();
            _DecisionModel = new DecisionModel();
            _DialogInfo = dialogInfo;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new LoopingDialog(DecisionMaker, _QuestionAndAnswerModel, _Logger, _DialogInfo, _conversationReferences));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync
            }));


            StartService();

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private void StartService()
        {
            var service = new NotificationService();           

            var startServiceThread = new Thread(service.StartService)
            {
                IsBackground = true
            };

            startServiceThread.Start();
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _DialogInfo.UserId = _Logger.LogUser(stepContext.Context.Activity.From.Id).Result;

            return await stepContext.BeginDialogAsync(nameof(LoopingDialog), "begin", cancellationToken);
        }        

       
        protected override Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_DialogInfo.DialogId != 0) {

                LogMessage(innerDc.Context.Activity.Text, innerDc.Context.Activity.Timestamp.Value);
            }
            return base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        private void LogMessage(string message, DateTimeOffset time)
        {
            _Logger.LogMessage(message, "user", time, _DialogInfo.DialogId);
        }



       

    }

}
