// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Template.Core.Interfaces;
using Template.Core.Models;

namespace Template.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly IConfiguration Configuration;
        protected readonly ILogger Logger;
        protected readonly IDecisionMaker DecisionMaker;
        protected QuestionAndAnswerModel _QuestionAndAnswerModel;
        
        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger, IDecisionMaker decisionMaker)
            : base(nameof(MainDialog))
        {
            Configuration = configuration;
            Logger = logger;
            DecisionMaker = decisionMaker;
            _QuestionAndAnswerModel = new QuestionAndAnswerModel();

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new LoopingDialog(DecisionMaker, _QuestionAndAnswerModel));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        
        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {


            var topics = DecisionMaker.GetStartTopics();
            var choices = new List<Choice>();
            
            foreach (var topic in topics)
            {
                choices.Add(new Choice(topic));
            }
            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Choose needed topic, please."),
                RetryPrompt = MessageFactory.Text("try one more time"),
                Choices = choices,
                Style = ListStyle.HeroCard
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _QuestionAndAnswerModel.QuestionModel = DecisionMaker.GetQuestionOrResult((stepContext.Result as FoundChoice).Value);

            return await stepContext.BeginDialogAsync(nameof(LoopingDialog), _QuestionAndAnswerModel, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
           
            


            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
