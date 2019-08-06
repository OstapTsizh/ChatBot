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
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

namespace StuddyBot.Dialogs
{
    public class MainMenuDialog : ComponentDialog// CancelAndRestartDialog
    {
        private readonly IDecisionMaker DecisionMaker;
        private readonly ThreadedLogger _myLogger;
        private DialogInfo _DialogInfo;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        private List<MainMenuItem> _menuItems;
        private List<string> _menuItemsNeutral;
        



        public MainMenuDialog(IDecisionMaker decisionMaker, ISubscriptionManager SubscriptionManager,
                             ThreadedLogger _myLogger, 
                             DialogInfo dialogInfo, 
                             ConcurrentDictionary<string, ConversationReference> conversationReferences,
                             StuddyBotContext db, IEmailSender emailSender,
                             DialogsMUI dialogsMui)
            : base(nameof(MainMenuDialog))
        {
            
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            _DialogInfo = dialogInfo;
            _conversationReferences = conversationReferences;


            AddDialog(new CoursesDialog(DecisionMaker,emailSender,SubscriptionManager, _myLogger, dialogInfo,
                conversationReferences, db, dialogsMui));
            AddDialog(new ChooseOptionDialog(DecisionMaker, emailSender, SubscriptionManager, _myLogger, dialogInfo, conversationReferences, db, dialogsMui));
            AddDialog(new PlannedEventsDialog(DecisionMaker, _myLogger, dialogInfo, conversationReferences, dialogsMui));
            AddDialog(new QAsDialog(DecisionMaker,emailSender, SubscriptionManager, _myLogger, dialogInfo, conversationReferences, db, dialogsMui));
            AddDialog(new SubscriptionDialog(DecisionMaker, emailSender, SubscriptionManager, _myLogger, dialogInfo, conversationReferences, db, dialogsMui));
            AddDialog(new MailingDialog(DecisionMaker, emailSender, SubscriptionManager, _myLogger, dialogInfo, conversationReferences, db, dialogsMui));
            AddDialog(new FinishDialog(DecisionMaker, emailSender, SubscriptionManager, _myLogger, dialogInfo, conversationReferences, db, dialogsMui));

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GetMenuItemsStepAsync,
                StartSelectedDialogStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Generates new QuestionAndAnswerModel and returns ChoicePrompt
        /// or passes on not modified QuestionAndAnswerModel to the next step.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> GetMenuItemsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            {
                _menuItems = DecisionMaker.GetMainMenuItems(_DialogInfo.Language);
                //_menuItemsNeutral = DecisionMaker.GetMainMenuItemsNeutral();

                var choices = new List<Choice>();

                foreach (var item in _menuItems)
                {
                    choices.Add(new Choice(item.Name));
                }

                var msg = "�� ��� ��������?";// "What you are interested in?";
                var retryMsg = "���� �����, ��������� �� ���";// "Try one more time, please:";
                
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

        private async Task<DialogTurnResult> StartSelectedDialogStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var selectedDialog = (string)(stepContext.Result as FoundChoice).Value;

            switch (_menuItems.FirstOrDefault(i => i.Name == selectedDialog).Dialog)
            {
                case "About":
                {
                    var msgText = string.Join("\n", _menuItems.FirstOrDefault(it => it.Dialog == "About").Resources);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(msgText), cancellationToken:cancellationToken);
                    return await stepContext.ReplaceDialogAsync(nameof(ChooseOptionDialog), "begin",
                        cancellationToken: cancellationToken);
                }
                case "Courses":
                {
                    
                    return await stepContext.ReplaceDialogAsync(nameof(CoursesDialog),
                        cancellationToken: cancellationToken);
                }
                case "PlannedEvents":
                {
                    return await stepContext.ReplaceDialogAsync(nameof(PlannedEventsDialog),
                        cancellationToken: cancellationToken);
                }
                default: //"QAs"
                {
                    return await stepContext.ReplaceDialogAsync(nameof(QAsDialog),
                        cancellationToken: cancellationToken);
                }

            }
        }
    }
}
