// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

using Autofac;
using System.Configuration;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
//using Microsoft.Bot.Builder.Dialogs.Internals;

namespace StuddyBot.Bots
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly ILogger Logger;
        protected readonly IDecisionMaker DecisionMaker;

        private readonly static IStorage azureBlobStorage = new AzureBlobStorage("DefaultEndpointsProtocol=https;AccountName=rdbotstorage;AccountKey=8mePuJGlKxPah6H8UZ6B44WLW9HYzmT82KERgGLDCGQDHA7Xtt+isqt1oQeAr/SPjP0DsrZRUt2blEsPDiHUPg==;EndpointSuffix=core.windows.net",
                "blobcontainerforrdbot");


        public DialogBot(ConversationState conversationState, UserState userState, T dialog,
            ILogger<DialogBot<T>> logger, IDecisionMaker decisionMaker)
        { 
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
            DecisionMaker = decisionMaker;
            
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);            
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Get the state properties from the turn context.
            //var dialogStateAccessors = UserState.CreateProperty<DialogState>("DialogState");
            //var dialogState = await dialogStateAccessors.GetAsync(turnContext, () => new DialogState());

            var userStateAccessors = UserState.CreateProperty<DialogInfo>(nameof(DialogInfo));
            var dialogInfo = await userStateAccessors.GetAsync(turnContext, () => new DialogInfo());

            //await turnContext.Activity.Entities.Add(new Entity { Type = DialogInfo } );
            // Run the Dialog with the new message Activity.s

            await Dialog.Run(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        }


    }
}
