// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LoggerService;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Configuration;
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
        protected ThreadedLogger _myLogger;
        protected int _dialogId;
        


        public MainDialog(IConfiguration configuration, IDecisionMaker decisionMaker, 
            ThreadedLogger _myLogger)
            : base(nameof(MainDialog))
         
        {
            Configuration = configuration;
            this._myLogger = _myLogger;    
            DecisionMaker = decisionMaker;

            _QuestionAndAnswerModel = new QuestionAndAnswerModel();
            _QuestionAndAnswerModel.QuestionModel = new QuestionModel();
            _QuestionAndAnswerModel.Answers = new List<string>();
            _DecisionModel = new DecisionModel();

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new LoopingDialog(DecisionMaker, _QuestionAndAnswerModel, _myLogger));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                PreFinalStepAsync,
                FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }



        
        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            _dialogId = _myLogger.LogDialog();
            logDialog(stepContext.Context.Activity.Text, stepContext.Context.Activity.Timestamp.Value);

            var topics = DecisionMaker.GetStartTopics();
            var choices = new List<Choice>();
            
            foreach (var topic in topics)
            {
                choices.Add(new Choice(topic));
            }
            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Choose needed topic, please."),
                RetryPrompt = MessageFactory.Text("Try one more time, please."),
                Choices = choices,
                Style = ListStyle.HeroCard
            };
            var message = options.Prompt.Text;
            var sender = "bot";
           var time = stepContext.Context.Activity.Timestamp.Value;
           
           
            _myLogger.LogMessage(message, sender, time, _dialogId);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _QuestionAndAnswerModel.Answers = new List<string>();

            _QuestionAndAnswerModel.QuestionModel = DecisionMaker.GetQuestionOrResult((stepContext.Result as FoundChoice).Value);

            return await stepContext.BeginDialogAsync(nameof(LoopingDialog), _QuestionAndAnswerModel, cancellationToken);
        }

        private async Task<DialogTurnResult> PreFinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            _QuestionAndAnswerModel = (QuestionAndAnswerModel)stepContext.Result;

           _DecisionModel = DecisionMaker.GetDecision(_QuestionAndAnswerModel.Answers, _QuestionAndAnswerModel.QuestionModel);



            var response = _DecisionModel.Answer + "\n" + _DecisionModel.Resources;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(response), cancellationToken);
            

            var message = response + "\n" + "Would you like to continue?";
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;

            _myLogger.LogMessage(message, sender, time, _dialogId);

           

           

            return await stepContext.PromptAsync(nameof(ChoicePrompt), 
                new PromptOptions()
                {
                    Prompt = MessageFactory.Text("Would you like to continue?"),
                    Choices = new List<Choice> { new Choice("yes"), new Choice("no") }
                },
                cancellationToken);

           
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var foundChoice = (stepContext.Result as FoundChoice).Value;

            if (foundChoice == "yes")
            {
               
                return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken);
            }
            
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you!"), cancellationToken);
           
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        protected override Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            //  _dialogId = _myLogger.LogDialog();
            //  logDialog(innerDc.Context.Activity.Text, innerDc.Context.Activity.Timestamp.Value);
            return base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        protected override Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            logDialog(innerDc.Context.Activity.Text, innerDc.Context.Activity.Timestamp.Value);
            return base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        //protected override Task OnEndDialogAsync(ITurnContext context, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //   // logDialog(context.Activity.Text, context.Activity.Timestamp.Value);
        //    return base.OnEndDialogAsync(context, instance, reason, cancellationToken);
        //}

        private void logDialog(string message, DateTimeOffset time)
        {
            _myLogger.LogMessage(message, "user", time, _dialogId);
        }
    }
}
