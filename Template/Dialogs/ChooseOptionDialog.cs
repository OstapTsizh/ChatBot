using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LoggerService;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.VisualBasic;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

namespace StuddyBot.Dialogs
{
    public class ChooseOptionDialog : ComponentDialog
    {
        private readonly IDecisionMaker DecisionMaker;
        private readonly ThreadedLogger _myLogger;
        private DialogInfo _DialogInfo;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        



        public ChooseOptionDialog(IDecisionMaker decisionMaker,
                             ThreadedLogger _myLogger, 
                             DialogInfo dialogInfo, 
                             ConcurrentDictionary<string, ConversationReference> conversationReferences)
            : base(nameof(ChooseOptionDialog))
        {
            
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            _DialogInfo = dialogInfo;
            _conversationReferences = conversationReferences;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                FirstStepAsync,
                RunDialogCourseStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Asks the user what to do next.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> FirstStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choices = new List<Choice>();

            {
                choices.Add(new Choice("Main Menu"));
                choices.Add(new Choice("Send conversation to email"));
            }
            var stepOptions = stepContext.Options.ToString().ToLower();

            if (stepOptions=="qa")
            {
                choices.Add(new Choice("Questions/Answers"));
                choices.Add(new Choice("Add a question to the database"));
            }
            else
            {
                choices.Add(new Choice("End dialog"));
            }

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("What to do next?"),
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

        /// <summary>
        /// Sends info about selected course.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> RunDialogCourseStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var choiceValue = (string)(stepContext.Result as FoundChoice).Value;
            switch (choiceValue)
            {
                case "Main Menu":
                    return await stepContext.ReplaceDialogAsync(nameof(MainMenuDialog),
                        cancellationToken: cancellationToken);
                case "Send conversation to email":
                    return await stepContext.ReplaceDialogAsync(nameof(MailingDialog),
                        cancellationToken: cancellationToken);
                case "Questions/Answers":
                    return await stepContext.ReplaceDialogAsync(nameof(QAsDialog),
                        cancellationToken: cancellationToken);
                case "Add a question to the database":
                    return await stepContext.ReplaceDialogAsync(nameof(AddQuestionDialog),
                        cancellationToken: cancellationToken);
                default: // "End dialog"
                    return await stepContext.ReplaceDialogAsync(nameof(FinishDialog),
                        cancellationToken: cancellationToken);
            }
        }
    }
}
