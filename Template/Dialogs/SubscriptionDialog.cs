using System;
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
using StuddyBot.Core.DAL.Entities;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;


namespace StuddyBot.Dialogs
{
    public class SubscriptionDialog : ComponentDialog
    {
        private readonly ISubscriptionManager _subscriptionManager;
        private ICollection<UserCourse> userSubscription;

        //delete
        private readonly List<UserCourse> _userCourses = new List<UserCourse>();

        public SubscriptionDialog(ISubscriptionManager subscriptionManager, IDecisionMaker decisionMaker,
            QuestionAndAnswerModel questionAndAnswerModel,
            ThreadedLogger _myLogger,
            DialogInfo dialogInfo,
            ConcurrentDictionary<string, ConversationReference> conversationReferences) : base(nameof(SubscriptionDialog))
        {
            _subscriptionManager = subscriptionManager;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new LoopingDialog(decisionMaker, questionAndAnswerModel, _myLogger, dialogInfo,
                conversationReferences));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                FirstStepAsync,
                UnsubStepAsync,
                DeleteSubscriptionStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> FirstStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Subscription dialog was started"), cancellationToken);

            var Course = new Course() { Id = 1, RegistrationStartDate = DateTime.Now, StartDate = DateTime.Now.AddDays(5) };
            var User = new User() { Id = "1" };

            var conversationReference = stepContext.Context.Activity.GetConversationReference();

            // _userCourses.Add(new UserCourse(){Course = Course,CourseId = 1,User = User,UserId = "1"});

            userSubscription = _subscriptionManager.GetUserSubscriptions(conversationReference.User.Id);

            if (userSubscription != null && userSubscription.Count != 0)
            {
                var subs = userSubscription.ToList();

                var message = "Your subscriptions:";
                subs.ForEach(sub =>
                    message += $"\nCourse Id: {sub.Course.Id}, Registration starts: {sub.Course.RegistrationStartDate.ToShortDateString()}," +
                               $" Course starts: {sub.Course.StartDate.Date.ToShortDateString()};");
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(message), cancellationToken);

                var unsubQuestion = "Would you like to unsubscribe from some?";

                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                       new PromptOptions()
                       {
                           Prompt = MessageFactory.Text(unsubQuestion),
                           Choices = new List<Choice> { new Choice("yes"), new Choice("no") }
                       },
                       cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("No subscriptions yet"), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> UnsubStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var foundChoice = (stepContext.Result as FoundChoice).Value;

            if (foundChoice == "yes")
            {
                var subs = userSubscription.ToList();

                var variants = new List<Choice>();
                subs.ForEach(sub => variants.Add(new Choice($"{sub.CourseId}")));
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                    new PromptOptions()
                    {
                        Prompt = MessageFactory.Text("Choose id of course from which you want to unsubscribe"),
                        Choices = variants
                    },
                    cancellationToken);
            }

            return await stepContext.ReplaceDialogAsync(nameof(LoopingDialog), "begin", cancellationToken);
            //return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> DeleteSubscriptionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var foundChoice = (stepContext.Result as FoundChoice).Value;

            var conversationReference = stepContext.Context.Activity.GetConversationReference();

            _subscriptionManager.CancelSubscription(conversationReference.User.Id, foundChoice);

            var subs = _subscriptionManager.GetUserSubscriptions(conversationReference.User.Id);

            var answer = "After Remove";
            subs.ToList().ForEach(sub => answer += $"\nId: {sub.CourseId}, course starts: {sub.Course.StartDate.ToShortDateString()}");
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(answer), cancellationToken);

            return await stepContext.ReplaceDialogAsync(nameof(SubscriptionDialog), cancellationToken: cancellationToken);
            //return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private List<UserCourse> GetUserSubscription(Activity activity)
        {
            return _userCourses;
            //return userCourses.Where(el => el.UserId == activity.GetConversationReference().User.Id).ToList();
        }
    }
}
