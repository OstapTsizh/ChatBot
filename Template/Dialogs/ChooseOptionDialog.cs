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
    public class ChooseOptionDialog : ComponentDialog// CancelAndRestartDialog
    {
        private readonly IDecisionMaker DecisionMaker;
        private readonly ThreadedLogger _myLogger;
        //private DialogInfo _DialogInfo;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private IStatePropertyAccessor<DialogInfo> _dialogInfoStateProperty;

        private Dictionary<string, string> _chooseOptionList;


        public ChooseOptionDialog(IStatePropertyAccessor<DialogInfo> dialogInfoStateProperty, IDecisionMaker decisionMaker, IEmailSender emailSender, ISubscriptionManager SubscriptionManager,
                             ThreadedLogger _myLogger, 
                             //DialogInfo dialogInfo, 
                             ConcurrentDictionary<string, ConversationReference> conversationReferences, StuddyBotContext db)
            : base(nameof(ChooseOptionDialog))
        {
            
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            //_DialogInfo = dialogInfo;
            _conversationReferences = conversationReferences;
            _dialogInfoStateProperty = dialogInfoStateProperty;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            //AddDialog(new MailingDialog(DecisionMaker, emailSender, SubscriptionManager, _myLogger, dialogInfo, conversationReferences, db));
            //AddDialog(new SubscriptionDialog(DecisionMaker, SubscriptionManager, _myLogger, dialogInfo, conversationReferences));
            //AddDialog(new FinishDialog(DecisionMaker, emailSender, SubscriptionManager, _myLogger, dialogInfo, conversationReferences, db));
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
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);

            _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);

            _chooseOptionList = DecisionMaker.GetChooseOptions(_DialogInfo.Language);

            var choices = new List<Choice>();

            var dialogsMUI = DecisionMaker.GetDialogsMui(_DialogInfo.Language);

            var mainMenu = dialogsMUI.ChooseOptionDictionary["mainMenu"]; // "Main Menu";
            var sendEmail = dialogsMUI.ChooseOptionDictionary["sendEmail"];// "Send conversation to email";
            var subscriptions = dialogsMUI.ChooseOptionDictionary["subscriptions"];// "My subscriptions";

            var ch1 = mainMenu;
            var ch2 = sendEmail;
            var ch3 = subscriptions;
            {
                choices.Add(new Choice(ch1));
                choices.Add(new Choice(ch2));
                choices.Add(new Choice(ch3));
            }
            var stepOptions = stepContext.Options.ToString().ToLower();

            if (stepOptions=="qa")
            {
                var ch4 = dialogsMUI.ChooseOptionDictionary["questions/answers"]; // "Questions/Answers";
                var ch5 = dialogsMUI.ChooseOptionDictionary["proposeQuestion"]; ; // "Propose a new question";

                choices.Add(new Choice(ch4));
                choices.Add(new Choice(ch5));
            }

            {
                var ch6 = dialogsMUI.ChooseOptionDictionary["endDialog"]; // "End dialog";
                choices.Add(new Choice(ch6));
            }

            var msg = dialogsMUI.ChooseOptionDictionary["next"];// "What to do next?";
            var retryMsg = dialogsMUI.MainDictionary["reprompt"];// "Try one more time, please:";

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text(msg),
                RetryPrompt = MessageFactory.Text(retryMsg),
                Choices = choices,
                Style = ListStyle.SuggestedAction
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
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);

            var choiceValue = (string)(stepContext.Result as FoundChoice).Value;
            var choice = _chooseOptionList[choiceValue];
            switch (choice)
            {
                case "Main Menu":
                    return await stepContext.ReplaceDialogAsync(nameof(MainMenuDialog),
                        cancellationToken: cancellationToken);
                case "Send conversation to email":
                    return await stepContext.ReplaceDialogAsync(nameof(MailingDialog), "send",
                        cancellationToken: cancellationToken);
                case "Questions/Answers":
                    return await stepContext.ReplaceDialogAsync(nameof(QAsDialog),
                        cancellationToken: cancellationToken);
                case "Propose a new question":
                    return await stepContext.ReplaceDialogAsync(nameof(AddQuestionDialog),
                        cancellationToken: cancellationToken);
                case "My subscriptions":
                    return await stepContext.ReplaceDialogAsync(nameof(SubscriptionDialog),
                        cancellationToken: cancellationToken);
                default: // "End dialog"
                    return await stepContext.ReplaceDialogAsync(nameof(FinishDialog),
                        cancellationToken: cancellationToken);
            }
        }
    }
}
