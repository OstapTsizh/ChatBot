using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using StuddyBot.Core.DAL.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace StuddyBot.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private StuddyBotContext _db;

        public NotificationController(IBotFrameworkHttpAdapter adapter, 
                                      ICredentialProvider credentials, 
                                      ConcurrentDictionary<string, ConversationReference> conversationReferences, 
                                      StuddyBotContext db)
        {
            _adapter = adapter;
                         
            _conversationReferences = conversationReferences;

            _appId = ((SimpleCredentialProvider)credentials).AppId;

            _db = db;

            // If the channel is the Emulator, and authentication is not in use,
            // the AppId will be null.  We generate a random AppId for this case only.
            // This is not required for production, since the AppId will have a value.
            if (string.IsNullOrEmpty(_appId))
            {
                _appId = Guid.NewGuid().ToString(); //if no AppId, use a random Guid
            }

            // _appId = "123";

        }


        public async Task Get()
        {
            var courses = _db.Courses.Where(s => s.RegistrationStartDate == DateTime.Today);

            if(courses != null)
            {
                foreach (var course in courses)
                {
                // HACK: fix this!
                // Following results with null!
                // Possible fix -> remove conversion
                // Issues after fix: incorrect conversation from string to ConversationReference;
                // How to save conversationReference in db?
                // Answer: serialize ConversationReference to a string and save it as string in db.

                    var notificationReferences = _db.UserCourses.Where(c => c.Course == course).Select(s => s.User.ConversationReference) as IEnumerable<ConversationReference>;

                    foreach (var conversationReference in notificationReferences)
                    {
                        await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
                    }
                }
            }



           

        }

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync("NOTIFICATION!!!!!!!");
        }




    }
}
