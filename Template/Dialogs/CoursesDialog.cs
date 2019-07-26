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
using StuddyBot.Core.DAL.Data;
using StuddyBot.Core.DAL.Entities;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;
using Course = StuddyBot.Core.Models.Course;

namespace StuddyBot.Dialogs
{
    public class CoursesDialog : ComponentDialog// CancelAndRestartDialog
    {
        private readonly IDecisionMaker DecisionMaker;
        private readonly ThreadedLogger _myLogger;
        private DialogInfo _DialogInfo;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private StuddyBotContext _db;
        private List<Course> courses;

        private string selectedCourse;

        public CoursesDialog(IDecisionMaker decisionMaker, 
                             ThreadedLogger _myLogger, 
                             DialogInfo dialogInfo, 
                             ConcurrentDictionary<string, ConversationReference> conversationReferences, StuddyBotContext db)
            : base(nameof(CoursesDialog))
        {
            
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            _DialogInfo = dialogInfo;
            _conversationReferences = conversationReferences;
            _db = db;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            //AddDialog(new ChooseOptionDialog(DecisionMaker, _myLogger, dialogInfo, conversationReferences));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AskCourseStepAsync,
                SendInfoAboutCourseStepAsync,
                AskForNotifyStepAsync
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
            courses = DecisionMaker.GetCourses(_DialogInfo.Language);

            {
                var choices = new List<Choice>();

                foreach (var course in courses)
                {
                    choices.Add(new Choice(course.Name));
                }

                var msg = "ўо ¬ас ц≥кавить?";// "What you are interested in?";
                var retryMsg = "Ѕудь ласка, спробуйте ще раз:";// "Try one more time, please:";

                var options = new PromptOptions()
                {
                    Prompt = MessageFactory.Text(msg),
                    RetryPrompt = MessageFactory.Text(retryMsg),
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
            selectedCourse = (string)(stepContext.Result as FoundChoice).Value;
            var msgText = string.Join("\n", courses.FirstOrDefault(it => it.Name == selectedCourse).Resources);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msgText), cancellationToken: cancellationToken);

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Do you want to receive a notification about the start?"),
                RetryPrompt = MessageFactory.Text("Try one more time, please."),
                Choices = new List<Choice> { new Choice("yes"), new Choice("no") },
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> AskForNotifyStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var foundChoice = (stepContext.Result as FoundChoice).Value;
            var userId = stepContext.Context.Activity.From.Id;
            if (foundChoice == "yes")
            {
                var subscriber = _db.User.First(user => user.Id == userId);
                var onCourse = _db.Courses.First(course => course.Name == selectedCourse);
                
                _db.UserCourses.Add(
                    new UserCourse()
                {
                    UserId =  subscriber.Id,
                    CourseId = onCourse.Id
                });
                _db.SaveChanges();

                return await stepContext.ReplaceDialogAsync(nameof(MailingDialog),
                    cancellationToken: cancellationToken);
            }

            return await stepContext.ReplaceDialogAsync(nameof(ChooseOptionDialog), "begin",
                cancellationToken: cancellationToken);
        }
    }
}
