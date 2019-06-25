// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DecisionMakers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Template.Core.Interfaces;

namespace Template.Bots
{
    public class DialogAndWelcomeBot<T> : DialogBot<T> where T : Dialog
    {
        public DialogAndWelcomeBot(ConversationState conversationState, UserState userState, T dialog, 
            ILogger<DialogBot<T>> logger, IDecisionMaker questionCtor)
            : base(conversationState, userState, dialog, logger, questionCtor)
        {
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {                
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    // ToDo
                    // Send the greetings and the first question from DecisionMaker
                    // with keywords and a list of questions.

                    await turnContext.SendActivityAsync(MessageFactory.Text("Hello! Say something"), cancellationToken);
                   // var model = DecisionMaker.GetQuestionOrResult("start");
                }
            }
        }



    }
}

