
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DecisionMakers;
using LoggerService;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using StuddyBot.Core.DAL.Entities;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;
using System;

namespace StuddyBot.Dialogs
{
    public class LoopingDialog : CancelAndRestartDialog
    {
        protected QuestionAndAnswerModel QuestionAndAnswerModel;
        protected DecisionModel _DecisionModel;
        protected readonly IDecisionMaker DecisionMaker;
        protected int numberOfQuestion;
        protected List<string> UserAnswers;
        protected ThreadedLogger _myLogger;
      //  protected UserState _UserState;
        protected DialogInfo _DialogInfo;
        private bool isNeededToGetQuestions = false;

        public LoopingDialog(IDecisionMaker decisionMaker, QuestionAndAnswerModel questionAndAnswerModel, ThreadedLogger _myLogger, DialogInfo dialogInfo)
            : base(nameof(LoopingDialog))
        {
            
            QuestionAndAnswerModel = questionAndAnswerModel;
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            _DialogInfo = dialogInfo;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                FirstStepAsync,
                FillQuestionModel,
                ActDialogAsync,
                FillAnswerStepAsync, 
                PreFinalStepAsync,
                FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> FirstStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Options.ToString() == "begin")
            {
                _DialogInfo.DialogId = _myLogger.LogDialog();
                isNeededToGetQuestions = true;

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


                _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

                return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
            }

            isNeededToGetQuestions = false;
            QuestionAndAnswerModel = (QuestionAndAnswerModel)stepContext.Result;
            return await stepContext.NextAsync(QuestionAndAnswerModel, cancellationToken);

        }

        private async Task<DialogTurnResult> FillQuestionModel(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (isNeededToGetQuestions == true)
            {
                QuestionAndAnswerModel.QuestionModel = DecisionMaker.GetQuestionOrResult((stepContext.Result as FoundChoice).Value);

                return await stepContext.NextAsync(QuestionAndAnswerModel, cancellationToken);
            }

            return await stepContext.NextAsync((QuestionAndAnswerModel)stepContext.Options, cancellationToken);

        }

        private async Task<DialogTurnResult> ActDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            QuestionAndAnswerModel = (QuestionAndAnswerModel)stepContext.Result;

            if (QuestionAndAnswerModel.QuestionModel == null)
            {
                return await stepContext.EndDialogAsync(cancellationToken);
            }

            var prompt = QuestionAndAnswerModel.QuestionModel.Questions.FirstOrDefault(q => q.IsAnswered == "false");

            numberOfQuestion = QuestionAndAnswerModel.QuestionModel.Questions.IndexOf(prompt);

            var promptText = MessageFactory.Text(prompt.Text);
            var choices = new List<Choice>();

            foreach (var option in prompt.Keywords)
            {
                choices.Add(new Choice(option));
            }

            var options = new PromptOptions()
            {
                Prompt = promptText,
                RetryPrompt = MessageFactory.Text("Try one more time, please"),
                Choices = choices,
                Style = ListStyle.HeroCard
            };


            var message = options.Prompt.Text;
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;


            _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> FillAnswerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var answer = (string) (stepContext.Result as FoundChoice).Value;
            QuestionAndAnswerModel.Answers.Add(answer);

            QuestionAndAnswerModel.QuestionModel.Questions.ElementAt(numberOfQuestion).IsAnswered = "true";

            if(QuestionAndAnswerModel.Answers.Count < QuestionAndAnswerModel.QuestionModel.Questions.Count)
            {
                return await stepContext.ReplaceDialogAsync(nameof(LoopingDialog), QuestionAndAnswerModel, cancellationToken);
                
            }

            return await stepContext.NextAsync(QuestionAndAnswerModel, cancellationToken);
        }

        private async Task<DialogTurnResult> PreFinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            QuestionAndAnswerModel = (QuestionAndAnswerModel)stepContext.Result;

            _DecisionModel = DecisionMaker.GetDecision(QuestionAndAnswerModel.Answers, QuestionAndAnswerModel.QuestionModel);



            var response = _DecisionModel.Answer + "\n" + _DecisionModel.Resources;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(response), cancellationToken);


            var message = response + "\n" + "Would you like to continue?";
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;

            _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

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
                QuestionAndAnswerModel = new QuestionAndAnswerModel();
                QuestionAndAnswerModel.Answers = new List<string>();
                _DecisionModel = new DecisionModel();
                return await stepContext.ReplaceDialogAsync(nameof(LoopingDialog),"begin", cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you!"), cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
        
    }
}
