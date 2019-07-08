using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LoggerService;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using StuddyBot.Core.Interfaces;

namespace StuddyBot.Bots
{
    public class DialogAndWelcomeBot<T> : DialogBot<T> where T : Dialog
    {
        // protected MyDialog _myDialog;
        protected ThreadedLogger _Logger;
        public DialogAndWelcomeBot(ConversationState conversationState, UserState userState, T dialog, 
            ILogger<DialogBot<T>> logger, IDecisionMaker questionCtor, ThreadedLogger Logger)
            : base(conversationState, userState, dialog, logger, questionCtor)
        {
            _Logger = Logger;

        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {                
                if (member.Id != turnContext.Activity.Recipient.Id)
                {                    
                    await turnContext.SendActivityAsync(MessageFactory.Text("Hello! Say something"), cancellationToken);
                  
                }
            }
        }



    }
}

