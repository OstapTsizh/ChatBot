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
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

namespace StuddyBot.Dialogs
{
    /// <summary>
    /// This dialog is responsible for the communication about
    /// adding new question to the QA database.
    /// </summary>
    public class AddQuestionDialog : ComponentDialog// CancelAndRestartDialog
    {
        private readonly IDecisionMaker DecisionMaker;
        private readonly ThreadedLogger _myLogger;
        private DialogInfo _DialogInfo;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        private Dictionary<string, string> _newQuestion;


        public AddQuestionDialog(IDecisionMaker decisionMaker, IEmailSender emailSender, 
                             ThreadedLogger _myLogger, 
                             DialogInfo dialogInfo, 
                             ConcurrentDictionary<string, ConversationReference> conversationReferences, StuddyBotContext db)
            : base(nameof(AddQuestionDialog))
        {
            
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            _DialogInfo = dialogInfo;
            _conversationReferences = conversationReferences;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            //AddDialog(new FinishDialog(DecisionMaker, _myLogger, dialogInfo, conversationReferences));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AskForQuestionStepAsync,
                FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Asks the user to type a new question.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> AskForQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Будь ласка, введіть Ваше питання:"), // Type Your question, please:
                Style = ListStyle.HeroCard
            };

            var message = options.Prompt.Text;
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;

            _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

            return await stepContext.PromptAsync(nameof(TextPrompt), options, cancellationToken);
        }

        /// <summary>
        /// Adds new question to a Database if not empty.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(stepContext.Result.ToString()))
            {
                return await stepContext.ReplaceDialogAsync(nameof(ChooseOptionDialog), "begin",
                    cancellationToken: cancellationToken);
            }

            // ToDo add new question to a Database
            // 
            // 

            var message = "Дякуємо за допомогу!"; // "Thank you for your help!";
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;

            _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(message),cancellationToken);

            return await stepContext.ReplaceDialogAsync(nameof(FinishDialog),
                cancellationToken: cancellationToken);
        }
    }
}
