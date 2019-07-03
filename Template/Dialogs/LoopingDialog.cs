
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LoggerService;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using StuddyBot.Core.DAL.Entities;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

namespace StuddyBot.Dialogs
{
    public class LoopingDialog : CancelAndRestartDialog
    {
        //protected readonly IDecisionMaker DecisionMaker;
        protected QuestionAndAnswerModel QuestionAndAnswerModel;
        protected int numberOfQuestion;
        protected List<string> UserAnswers;
        protected ThreadedLogger _myLogger;
        protected readonly MyDialog _myDialog;

        public LoopingDialog(IDecisionMaker decisionMaker, QuestionAndAnswerModel questionAndAnswerModel, ThreadedLogger _myLogger, MyDialog myDialog)
            : base(nameof(LoopingDialog), myDialog)//, myDialog
        {
            //DecisionMaker = decisionMaker;
            QuestionAndAnswerModel = questionAndAnswerModel;
            _myDialog = myDialog;
            this._myLogger = _myLogger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                FirstStepAsync,
                FillAnswerStepAsync
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


            _myDialog.Message = options.Prompt.Text;
            _myDialog.Time = stepContext.Context.Activity.Timestamp.Value;
           

            _myLogger.LogMessage(_myDialog);

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
            
            return await stepContext.EndDialogAsync(QuestionAndAnswerModel, cancellationToken);
        }
        
    }
}
