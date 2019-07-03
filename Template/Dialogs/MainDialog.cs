// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LoggerService;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StuddyBot.Core.DAL.Entities;
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
        protected MyDialog _myDialog;
        


        public MainDialog(IConfiguration configuration, IDecisionMaker decisionMaker,
            ThreadedLogger _myLogger, MyDialog myDialog)
            : base(nameof(MainDialog))//, myDialog
        {
            Configuration = configuration;
            this._myLogger = _myLogger;    
            DecisionMaker = decisionMaker;
            _myDialog = myDialog;
            _QuestionAndAnswerModel = new QuestionAndAnswerModel();
            _QuestionAndAnswerModel.QuestionModel = new QuestionModel();
            _QuestionAndAnswerModel.Answers = new List<string>();
            _DecisionModel = new DecisionModel();

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new LoopingDialog(DecisionMaker, _QuestionAndAnswerModel, _myLogger, _myDialog));
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
            if (_myDialog.DialogsId == 0)
            {
                var dialogId = _myLogger.LogDialog();
                _myDialog.DialogsId = dialogId;
            }

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
            _myDialog.Message = options.Prompt.Text;
            _myDialog.Sender = "bot";
            _myDialog.Time = stepContext.Context.Activity.Timestamp.Value;
           
           
            _myLogger.LogMessage(_myDialog);

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

           _DecisionModel =  DecisionMaker.GetDecision(_QuestionAndAnswerModel.Answers, _QuestionAndAnswerModel.QuestionModel);


            var response = _DecisionModel.Answer + "\n" + _DecisionModel.Resources;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(response), cancellationToken);
            

            _myDialog.Message = response + "\n" + "Would you like to continue?";
            _myDialog.Time = stepContext.Context.Activity.Timestamp.Value;

            _myLogger.LogMessage(_myDialog);

           

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

            stepContext.Context.Activity.Text = "restart";

            if (foundChoice == "yes")
            {
               
                return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken);
            }
            
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you!"), cancellationToken);
           
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
