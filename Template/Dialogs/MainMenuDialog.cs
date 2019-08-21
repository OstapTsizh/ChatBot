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
        //private DialogInfo _DialogInfo;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private IStatePropertyAccessor<DialogInfo> _dialogInfoStateProperty;

        private List<MainMenuItem> _menuItems;
        private List<string> _menuItemsNeutral;
        



        public MainMenuDialog(IStatePropertyAccessor<DialogInfo> dialogInfoStateProperty, IDecisionMaker decisionMaker, ISubscriptionManager SubscriptionManager,
                             ThreadedLogger _myLogger, 
                             //DialogInfo dialogInfo, 
                             ConcurrentDictionary<string, ConversationReference> conversationReferences,
                             StuddyBotContext db, IEmailSender emailSender)
            : base(nameof(MainMenuDialog))
        {
            
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            //_DialogInfo = dialogInfo;
            _conversationReferences = conversationReferences;
            _dialogInfoStateProperty = dialogInfoStateProperty;


            AddDialog(new CoursesDialog(dialogInfoStateProperty, DecisionMaker, emailSender,SubscriptionManager, _myLogger,
                //dialogInfo,
                conversationReferences, db));
            AddDialog(new ChooseOptionDialog(dialogInfoStateProperty, DecisionMaker, emailSender, SubscriptionManager, _myLogger,
                //dialogInfo,
                conversationReferences, db));
            AddDialog(new PlannedEventsDialog(dialogInfoStateProperty, DecisionMaker, _myLogger,
                //dialogInfo,
                conversationReferences));
            AddDialog(new QAsDialog(dialogInfoStateProperty, DecisionMaker, emailSender, SubscriptionManager, _myLogger,
                //dialogInfo,
                conversationReferences, db));
            AddDialog(new SubscriptionDialog(dialogInfoStateProperty, DecisionMaker, emailSender, SubscriptionManager, _myLogger,
                //dialogInfo,
                conversationReferences, db));
            AddDialog(new MailingDialog(dialogInfoStateProperty, DecisionMaker, emailSender, SubscriptionManager, _myLogger,
                //dialogInfo,
                conversationReferences, db));
            AddDialog(new FinishDialog(dialogInfoStateProperty, DecisionMaker, emailSender, SubscriptionManager, _myLogger,
                //dialogInfo,
                conversationReferences, db));

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
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);//new DialogState()
            {
                _menuItems = DecisionMaker.GetMainMenuItems(_DialogInfo.Language);
                //_menuItemsNeutral = DecisionMaker.GetMainMenuItemsNeutral();

                var choices = new List<Choice>();

                foreach (var item in _menuItems)
                {
                    choices.Add(new Choice(item.Name));
                }

                var dialogsMUI = DecisionMaker.GetDialogsMui(_DialogInfo.Language);

                var prompt = dialogsMUI.MainMenuDictionary["prompt"];// "What you are interested in?";

                var options = new PromptOptions()
                {
                    Prompt = MessageFactory.Text(prompt),
                    RetryPrompt = MessageFactory.Text(dialogsMUI.MainDictionary["reprompt"]),
                    Choices = choices,
                    Style = ListStyle.SuggestedAction
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

            //await _dialogInfoStateProperty.SetAsync(stepContext.Context, _DialogInfo);

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
