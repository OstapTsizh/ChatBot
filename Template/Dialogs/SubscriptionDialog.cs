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
using StuddyBot.Core.DAL.Data;
using StuddyBot.Core.DAL.Entities;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

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
        
        public SubscriptionDialog(IDecisionMaker decisionMaker, IEmailSender emailSender, ISubscriptionManager subscriptionManager,
                             ThreadedLogger _myLogger, 
                             DialogInfo dialogInfo, 
                             ConcurrentDictionary<string, ConversationReference> conversationReferences, StuddyBotContext db)
            : base(nameof(SubscriptionDialog))
        {
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            _subscriptionManager = subscriptionManager;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ChooseOptionDialog(DecisionMaker, emailSender, subscriptionManager,  _myLogger, dialogInfo, conversationReferences, db));
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

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("���� �������:"));

                var message = "";
                
                for (var i = 0; i < subs.Count; i++)
                {
                    var courseInfo = _subscriptionManager.GetCourseInfo(subs[i].CourseId.ToString());
                    message +=
                        $"\n\n**{i+1}** �����: {courseInfo.Name}, ��������� ����������: {courseInfo.RegistrationStartDate.ToShortDateString()}," +
                        $" ���� ����������: {courseInfo.StartDate.Date.ToShortDateString()};";
                }

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(message), cancellationToken);

                var unsubQuestion = "������ ���������� �� ��������� �� ����?";

                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                    new PromptOptions()
                    {
                        Prompt = MessageFactory.Text(unsubQuestion),
                        Choices = new List<Choice> {new Choice("���"), new Choice("�")}
                    },
                    cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("ϳ������ ����"), cancellationToken);
            return await stepContext.ReplaceDialogAsync(nameof(ChooseOptionDialog), "begin", cancellationToken);
        }

        private async Task<DialogTurnResult> UnsubStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var foundChoice = (stepContext.Result as FoundChoice).Value;

            if (foundChoice == "���" && userSubscription.Count != 0)
            {
                var subs = userSubscription.ToList();

                var variants = new List<Choice>();

                for (var i = 0; i < subs.Count; i++)
                {
                    variants.Add(new Choice($"{i+1}"));
                }
                
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                    new PromptOptions()
                    {
                        Prompt = MessageFactory.Text("������ ����� ����� �� ����� �� ������ ����������"),
                        RetryPrompt = MessageFactory.Text("���� �����, ��������� �� ���"),
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
            var foundChoice = int.Parse((stepContext.Result as FoundChoice).Value);

            var conversationReference = stepContext.Context.Activity.GetConversationReference();

            var cancelCourse = userSubscription.ToList()[foundChoice - 1];

            _subscriptionManager.CancelSubscription(conversationReference.User.Id, cancelCourse.CourseId.ToString());

            return await stepContext.ReplaceDialogAsync(nameof(SubscriptionDialog),
                cancellationToken: cancellationToken);
        }
    }
}