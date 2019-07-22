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
    public class CoursesDialog : CancelAndRestartDialog
    {
        private readonly IDecisionMaker DecisionMaker;
        private readonly ThreadedLogger _myLogger;
        private DialogInfo _DialogInfo;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        private List<Course> courses;



        public CoursesDialog(IDecisionMaker decisionMaker, 
                             ThreadedLogger _myLogger, 
                             DialogInfo dialogInfo, 
                             ConcurrentDictionary<string, ConversationReference> conversationReferences)
            : base(nameof(CoursesDialog))
        {
            
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            _DialogInfo = dialogInfo;
            _conversationReferences = conversationReferences;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            //AddDialog(new ChooseOptionDialog(DecisionMaker, _myLogger, dialogInfo, conversationReferences));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AskCourseStepAsync,
                SendInfoAboutCourseStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Asks the user to choose some course.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> AskCourseStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            courses = DecisionMaker.GetCourses();

            {
                var choices = new List<Choice>();

                foreach (var course in courses)
                {
                    choices.Add(new Choice(course.Name));
                }

                var options = new PromptOptions()
                {
                    Prompt = MessageFactory.Text("What you are interested in?"),
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
        }

        /// <summary>
        /// Sends info about selected course.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> SendInfoAboutCourseStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var selectedCourse = (string)(stepContext.Result as FoundChoice).Value;
            var msgText = string.Join("\n", courses.FirstOrDefault(it => it.Name == selectedCourse).Resources);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msgText), cancellationToken: cancellationToken);
            return await stepContext.ReplaceDialogAsync(nameof(SubscriptionDialog),
                cancellationToken: cancellationToken);
        }
    }
}
