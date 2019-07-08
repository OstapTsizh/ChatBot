// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LoggerService;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Services.NotificationService;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

namespace StuddyBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly IConfiguration Configuration;
        protected readonly IDecisionMaker DecisionMaker;
        protected QuestionAndAnswerModel _QuestionAndAnswerModel;
        protected DecisionModel _DecisionModel;
        protected ThreadedLogger _myLogger;
        //  protected MyDialog _myDialog;

        /// <summary>
        /// conversations with users which subscribed to notifications
        /// TODO: possible renaming needed
        /// </summary>
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;


        public MainDialog(IConfiguration configuration, IDecisionMaker decisionMaker, 
            ThreadedLogger _myLogger, ConcurrentDictionary<string, ConversationReference> conversationReferences)
            : base(nameof(MainDialog))
         
        {
            Configuration = configuration;
            this._myLogger = _myLogger;    
            DecisionMaker = decisionMaker;
            _conversationReferences = conversationReferences;

            _QuestionAndAnswerModel = new QuestionAndAnswerModel();
            _QuestionAndAnswerModel.QuestionModel = new QuestionModel();
            _QuestionAndAnswerModel.Answers = new List<string>();
            _DecisionModel = new DecisionModel();

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new LoopingDialog(DecisionMaker, _QuestionAndAnswerModel, _myLogger));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                AskNotifyStepAsync,
                PreFinalStepAsync,
                FinalStepAsync
            }));


            StartService();

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private void StartService()
        {
            var service = new NotificationService();           

            var startServiceThread = new Thread(service.StartService)
            {
                IsBackground = true
            };

            startServiceThread.Start();
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _myLogger.LogDialog();
          
            var topics = DecisionMaker.GetStartTopics();
            var choices = new List<Choice>();
            
            foreach (var topic in topics)
            {
                choices.Add(new Choice(topic));
            }
            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Choose needed topic, please."),
                RetryPrompt = MessageFactory.Text("Try one more time, please."),
                Choices = choices,
                Style = ListStyle.HeroCard
            };
            var message = options.Prompt.Text;
            var sender = "bot";
           var time = stepContext.Context.Activity.Timestamp.Value;
           
           
            _myLogger.LogMessage(message, sender, time);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _QuestionAndAnswerModel.Answers = new List<string>();

            _QuestionAndAnswerModel.QuestionModel = DecisionMaker.GetQuestionOrResult((stepContext.Result as FoundChoice).Value);

            return await stepContext.BeginDialogAsync(nameof(LoopingDialog), _QuestionAndAnswerModel, cancellationToken);
        }


        /// <summary>
        /// Весь вміст з преФінал тільки поміняний текст
        /// </summary>
        private async Task<DialogTurnResult> AskNotifyStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _QuestionAndAnswerModel = (QuestionAndAnswerModel)stepContext.Result;

            _DecisionModel = DecisionMaker.GetDecision(_QuestionAndAnswerModel.Answers, _QuestionAndAnswerModel.QuestionModel);

            var response = _DecisionModel.Answer + "\n" + _DecisionModel.Resources;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(response), cancellationToken);


            var message = response + "\n" + "Would you like me to notify You when registration starts?";
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;
            _myLogger.LogMessage(message, sender, time);

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions()
                {
                    Prompt = MessageFactory.Text("Would you like me to notify You when registration starts?"),
                    Choices = new List<Choice> { new Choice("yes"), new Choice("no") }
                },
                cancellationToken);
             
        }

        private async Task<DialogTurnResult> PreFinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var foundChoice = (stepContext.Result as FoundChoice).Value;
            var activity = stepContext.Context.Activity as Activity;
            if (foundChoice == "yes")
            {                
                if (!CheckConversationReference(activity))
                    AddConversationReference(activity);
                // OPTIONAL
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("You already are subscribed!)"), cancellationToken);
                }
            }
            else if(foundChoice == "no")
            {
                if(CheckConversationReference(activity))
                {
                    DeleteConversationReference(activity);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Subscription was canceled"), cancellationToken);
                    
                    // TODO: Choice prompt or something to reassure -- optional -> possibly user might cancel subscription in specific dialog.
                    
                }
            }

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
               new PromptOptions()
               {
                   Prompt = MessageFactory.Text("Would you like to continue?"),
                   Choices = new List<Choice> { new Choice("yes"), new Choice("no") }
               },
               cancellationToken);

        }

        

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var foundChoice = (stepContext.Result as FoundChoice).Value;

            stepContext.Context.Activity.Text = "restart";

            if (foundChoice == "yes")
            {
               
                return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken);
            }
            
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you!"), cancellationToken);
           
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }



        private bool CheckConversationReference(Activity activity)
        {
            return _conversationReferences.ContainsKey(activity.GetConversationReference().User.Id);
        }

        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }

        private void DeleteConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.TryRemove(conversationReference.User.Id, out _ );
        }

    }

}
