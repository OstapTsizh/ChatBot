using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmailSender.Interfaces;
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

        public CoursesDialog(IDecisionMaker decisionMaker, IEmailSender emailSender, ISubscriptionManager SubscriptionManager,
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
            AddDialog(new FinishDialog(DecisionMaker,emailSender, SubscriptionManager, _myLogger, dialogInfo, conversationReferences, db));
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

                var prompt = DialogsMUI.CoursesDictionary["prompt"];// "What you are interested in?";
                var reprompt = DialogsMUI.CoursesDictionary["reprompt"];

                var options = new PromptOptions()
                {
                    Prompt = MessageFactory.Text(prompt),
                    RetryPrompt = MessageFactory.Text(reprompt),
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
            var msgText = courses.FirstOrDefault(it => it.Name == selectedCourse).Resources;
            //string.Join("\n", courses.FirstOrDefault(it => it.Name == selectedCourse).Resources);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msgText), cancellationToken: cancellationToken);

            var prompt = DialogsMUI.CoursesDictionary["promptNotification"];//Do you want to receive a notification about the start?
            var reprompt = DialogsMUI.CoursesDictionary["reprompt"];

            var yes = DialogsMUI.MainDictionary["yes"];
            var no = DialogsMUI.MainDictionary["no"];


        var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text(prompt), 
                RetryPrompt = MessageFactory.Text(reprompt),
                Choices = new List<Choice> { new Choice(yes), new Choice(no) },
            };

            var message = options.Prompt.Text;
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;

            _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> AskForNotifyStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var foundChoice = (stepContext.Result as FoundChoice).Value;
            
            if (foundChoice == DialogsMUI.MainDictionary["yes"])
            {
                _db.AddSubscriptionToCourse(_DialogInfo.UserId,selectedCourse);
                _db.SaveChanges();

                return await stepContext.ReplaceDialogAsync(nameof(MailingDialog), "notification",
                    cancellationToken: cancellationToken);
            }

            return await stepContext.ReplaceDialogAsync(nameof(ChooseOptionDialog), "begin",
                    cancellationToken: cancellationToken);
        }
    }
}
