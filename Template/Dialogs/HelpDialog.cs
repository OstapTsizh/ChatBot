using System;
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
using Microsoft.Extensions.Configuration;
using StuddyBot.Core.DAL.Data;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

namespace StuddyBot.Dialogs
{
    public class HelpDialog : ComponentDialog
    {
        private readonly IDecisionMaker DecisionMaker;
        private readonly ThreadedLogger _Logger;
        private IStatePropertyAccessor<DialogInfo> _dialogInfoStateProperty;


        public HelpDialog(IStatePropertyAccessor<DialogInfo> dialogInfoStateProperty, IDecisionMaker decisionMaker, ISubscriptionManager SubscriptionManager,
                             ThreadedLogger _myLogger, 
                             IEmailSender emailSender,
                             StuddyBotContext db)
            : base(nameof(HelpDialog))//, dialogInfoStateProperty)
             
        {
            _Logger = _myLogger;
            DecisionMaker = decisionMaker;
            
            _dialogInfoStateProperty = dialogInfoStateProperty;

            AddDialog(new ChooseOptionDialog(dialogInfoStateProperty, DecisionMaker, emailSender, SubscriptionManager,
                _myLogger, db
                ));



            AddDialog(new MainMenuDialog(dialogInfoStateProperty, DecisionMaker, SubscriptionManager, _myLogger,
                db,
                emailSender));
            AddDialog(new CoursesDialog(dialogInfoStateProperty, DecisionMaker, emailSender, SubscriptionManager,
                _myLogger, db
                ));
            AddDialog(new PlannedEventsDialog(dialogInfoStateProperty, DecisionMaker, _myLogger, db
                ));
            AddDialog(new QAsDialog(dialogInfoStateProperty, DecisionMaker, emailSender, SubscriptionManager,
                _myLogger, db
                ));
            AddDialog(new SubscriptionDialog(dialogInfoStateProperty, DecisionMaker, emailSender, SubscriptionManager,
                _myLogger, db
                ));
            AddDialog(new MailingDialog(dialogInfoStateProperty, DecisionMaker, emailSender, SubscriptionManager,
                _myLogger, db
                ));
            AddDialog(new FinishDialog(dialogInfoStateProperty, DecisionMaker, emailSender, SubscriptionManager,
                _myLogger, db
                ));



            AddDialog(new TextPrompt(nameof(TextPrompt)));
            
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ShowHelpAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ShowHelpAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);
            var dialogsMUI = DecisionMaker.GetDialogsMui(_DialogInfo.Language);
            var helpMsg = dialogsMUI.HelpDictionary["commands"];
            var message = string.Join("\n\n", helpMsg);

            await stepContext.Context.SendActivityAsync(message);

            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;

            _Logger.LogMessage(message, sender, time, _DialogInfo.DialogId);

            return await stepContext.ReplaceDialogAsync(nameof(ChooseOptionDialog), "begin",
                    cancellationToken: cancellationToken);
        }
    }
}
