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
    public class SubscriptionDialog : CancelAndRestartDialog
    {
        private QuestionAndAnswerModel QuestionAndAnswerModel;
        private DecisionModel _DecisionModel;
        private readonly IDecisionMaker DecisionMaker;
        private List<string> UserAnswers;
        private readonly ThreadedLogger _myLogger;
        private DialogInfo _DialogInfo;
        private bool isNeededToGetQuestions = false;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        



        public SubscriptionDialog(IDecisionMaker decisionMaker, 
                             QuestionAndAnswerModel questionAndAnswerModel, 
                             ThreadedLogger _myLogger, 
                             DialogInfo dialogInfo, 
                             ConcurrentDictionary<string, ConversationReference> conversationReferences)
            : base(nameof(SubscriptionDialog))
        {
            
            QuestionAndAnswerModel = questionAndAnswerModel;
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            _DialogInfo = dialogInfo;
            _conversationReferences = conversationReferences;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            //AddDialog(new ChooseOptionDialog(DecisionMaker, _myLogger, dialogInfo, conversationReferences));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                SubscribeStepAsync,
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
        private async Task<DialogTurnResult> SubscribeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            return await stepContext.NextAsync(cancellationToken);
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
            



            return await stepContext.ReplaceDialogAsync(nameof(ChooseOptionDialog),
                cancellationToken: cancellationToken);
        }
    }
}
