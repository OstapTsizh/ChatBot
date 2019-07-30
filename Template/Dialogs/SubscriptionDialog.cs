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
using Microsoft.VisualBasic;
using StuddyBot.Core.DAL.Entities;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;
using Course = StuddyBot.Core.DAL.Entities.Course;

namespace StuddyBot.Dialogs
{
    public class SubscriptionDialog : ComponentDialog
    {
        private DecisionModel _DecisionModel;
        private readonly IDecisionMaker DecisionMaker;
        private readonly ISubscriptionManager _subscriptionManager;
        private List<string> UserAnswers;
        private readonly ThreadedLogger _myLogger;
        private DialogInfo _DialogInfo;
        private bool isNeededToGetQuestions = false;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        private ICollection<UserCourse> userSubscription;
        
        public SubscriptionDialog(IDecisionMaker decisionMaker, ISubscriptionManager subscriptionManager,
                             ThreadedLogger _myLogger, 
                             DialogInfo dialogInfo, 
                             ConcurrentDictionary<string, ConversationReference> conversationReferences)
            : base(nameof(SubscriptionDialog))
        {
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            _subscriptionManager = subscriptionManager;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ChooseOptionDialog(DecisionMaker, subscriptionManager,  _myLogger, dialogInfo, conversationReferences));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                FirstStepAsync,
                UnsubStepAsync,
                DeleteSubscriptionStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> FirstStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var conversationReference = stepContext.Context.Activity.GetConversationReference();

            userSubscription = _subscriptionManager.GetUserSubscriptions(conversationReference.User.Id);

            if (userSubscription != null && userSubscription.Count != 0)
            {
                var subs = userSubscription.ToList();

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Ваші підписки:"));

                var message = "";

                foreach (var sub in subs)
                {
                    var courseInfo = _subscriptionManager.GetCourseInfo(sub.CourseId.ToString());
                    message +=
                        $"\n\nId курсу: {courseInfo.Id}, Назва: {courseInfo.Name}, Реєстрація починається: {courseInfo.RegistrationStartDate.ToShortDateString()}," +
                        $" Курс починається: {courseInfo.StartDate.Date.ToShortDateString()};";
                }

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(message), cancellationToken);

                var unsubQuestion = "Хочете відписатись від сповіщення на курс?";

                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                    new PromptOptions()
                    {
                        Prompt = MessageFactory.Text(unsubQuestion),
                        Choices = new List<Choice> {new Choice("так"), new Choice("ні")}
                    },
                    cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Підписок немає"), cancellationToken);
            return await stepContext.ReplaceDialogAsync(nameof(ChooseOptionDialog), "begin", cancellationToken);
        }

        private async Task<DialogTurnResult> UnsubStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var foundChoice = (stepContext.Result as FoundChoice).Value;

            if (foundChoice == "так" && userSubscription.Count != 0)
            {
                var subs = userSubscription.ToList();

                var variants = new List<Choice>();
                subs.ForEach(sub => variants.Add(new Choice($"{sub.Course.Id}")));
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                    new PromptOptions()
                    {
                        Prompt = MessageFactory.Text("Id курсу від якого Ви хочете відписатись"),
                        RetryPrompt = MessageFactory.Text("Будь ласка, спробуйте ще раз"),
                        Choices = variants,
                        Style = ListStyle.HeroCard
                    },
                    cancellationToken);
            }

            return await stepContext.ReplaceDialogAsync(nameof(ChooseOptionDialog), "begin", cancellationToken);
        }

        private async Task<DialogTurnResult> DeleteSubscriptionStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var foundChoice = (stepContext.Result as FoundChoice).Value;

            var conversationReference = stepContext.Context.Activity.GetConversationReference();

            _subscriptionManager.CancelSubscription(conversationReference.User.Id, foundChoice);

            return await stepContext.ReplaceDialogAsync(nameof(SubscriptionDialog),
                cancellationToken: cancellationToken);
        }
    }
}