// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DecisionMakers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Template.Core.Interfaces;
using Template.Core.Models;
using Template.Core.Services;

namespace Template.Dialogs
{
    public class LoopingDialog : CancelAndHelpDialog
    {
        protected readonly IDecisionMaker DecisionMaker;
        protected QuestionAndAnswerModel QuestionAndAnswerModel;
        protected IUserAnswerResolveService UserAnswerResolveService;
        protected int numberOfQuestion;
        protected List<string> UserAnswers;

        public LoopingDialog(IDecisionMaker decisionMaker, QuestionAndAnswerModel questionAndAnswerModel)
            : base(nameof(LoopingDialog))
        {
            DecisionMaker = decisionMaker;
            QuestionAndAnswerModel = questionAndAnswerModel;
            
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                FirstStepAsync,
                FillAnswerStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> FirstStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            QuestionAndAnswerModel = (QuestionAndAnswerModel)stepContext.Options;

            if (QuestionAndAnswerModel.QuestionModel == null)
            {
                return await stepContext.EndDialogAsync(cancellationToken);
            }

            
            var prompt = QuestionAndAnswerModel.QuestionModel.Questions.FirstOrDefault(q => q.IsAnswered == "false");
            var promptText = MessageFactory.Text(prompt.Text);
            numberOfQuestion = QuestionAndAnswerModel.QuestionModel.Questions.IndexOf(prompt);

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions(){ Prompt = promptText }, cancellationToken);
        }

        private async Task<DialogTurnResult> FillAnswerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var answer = (string) stepContext.Result;
            QuestionAndAnswerModel.Answers = new List<string>();
            QuestionAndAnswerModel.Answers.Add(answer);

            QuestionAndAnswerModel.QuestionModel.Questions.ElementAt(numberOfQuestion).IsAnswered = "true";

            if(QuestionAndAnswerModel.Answers.Count < QuestionAndAnswerModel.QuestionModel.Questions.Count)
            {
                return await stepContext.ReplaceDialogAsync(nameof(LoopingDialog), QuestionAndAnswerModel, cancellationToken);
            }


            UserAnswerResolveService.GetDecision(QuestionAndAnswerModel.Answers, QuestionAndAnswerModel.QuestionModel);

            return await stepContext.EndDialogAsync(QuestionAndAnswerModel, cancellationToken);

        }
        
        // UNDONE: decisions.resources == null

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingDetails)stepContext.Options;

            bookingDetails.TravelDate = (string)stepContext.Result;

            var msg = $"Please confirm, I have you traveling to: {bookingDetails.Destination} from: {bookingDetails.Origin} on: {bookingDetails.TravelDate}";

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var bookingDetails = (BookingDetails)stepContext.Options;

                return await stepContext.EndDialogAsync(bookingDetails, cancellationToken);
            }
            else
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

    }
}
