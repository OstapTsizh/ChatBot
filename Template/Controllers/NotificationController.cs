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
using EmailSender.Interfaces;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

namespace StuddyBot.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly StuddyBotContext _db;
        private readonly IEmailSender _emailSender;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IDecisionMaker _decisionMaker;

        public NotificationController(IBotFrameworkHttpAdapter adapter,
                                      ICredentialProvider credentials,
                                      ConcurrentDictionary<string, ConversationReference> conversationReferences,
                                      StuddyBotContext db,
                                      IEmailSender emailSender,
                                      ISubscriptionManager subscriptionManager,
                                      IDecisionMaker decisionMaker)
        {
            _adapter = adapter;
            _conversationReferences = conversationReferences;
            _appId = ((SimpleCredentialProvider)credentials).AppId;
            _db = db;
            _emailSender = emailSender;
            _decisionMaker = decisionMaker;
            _subscriptionManager = subscriptionManager;

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

            //foreach (var conversationReference in _conversationReferences.Values)
            //{
            //    await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            //}


            var courses = _db.Courses.Where(s => s.RegistrationStartDate.ToShortDateString() == DateTime.Today.ToShortDateString());

            if (courses.Any())
            {
                foreach (var course in courses)
                {
                    // HACK: fix this!
                    // Following results with null!
                    // Possible fix -> remove conversion
                    // Issues after fix: incorrect conversation from string to ConversationReference;
                    // How to save conversationReference in db?
                    // Answer: serialize ConversationReference to a string and save it as string in db.

                    //var notificationReferences = _db.UserCourses.Where(c => c.Course == course).Select(s => s.User.ConversationReference) as IEnumerable<ConversationReference>;


                    // This is for notification into the chat.
                    //foreach (var conversationReference in _conversationReferences.Values)
                    //{
                    //    await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
                    //}

                    // This is for notification on email.


                    var matchedCourses = _db.UserCourses.Where(item => item.CourseId == course.Id);


                    if (matchedCourses.Any())
                    {
                        foreach (var matchedCourse in matchedCourses)
                        {
                            if (!string.IsNullOrEmpty(_db.User.First(user => user.Id == matchedCourse.UserId).Email) &&
                                !matchedCourse.Notified)
                            {
                                var language = _db.User.First(user => user.Id == matchedCourse.UserId).Language;
                                var courseByName = _decisionMaker.GetCourses(language)
                                    .First(item => item.Name == matchedCourse.Course.Name);

                                var message = $"<h3>{DialogsMUI.SubscriptionDictionary["title"]}</h3> <br>" +
                                              $"<h5>{DialogsMUI.SubscriptionDictionary["info"]} {matchedCourse.Course.Name}," +
                                              $" {DialogsMUI.SubscriptionDictionary["registrationStarts"]} {matchedCourse.Course.RegistrationStartDate.ToShortDateString()}" +
                                              $" {DialogsMUI.SubscriptionDictionary["courseStarts"]} {matchedCourse.Course.StartDate.ToShortDateString()}. <br> </h5>" +
                                              $"{courseByName.Resources}";

                                await _emailSender.SendEmailAsync(matchedCourse.User.Email,
                                DialogsMUI.SubscriptionDictionary["subject"],
                                    message);

                                matchedCourse.Notified = true;
                                _db.SaveChanges();

                                Console.WriteLine("-----------------\n" +
                                                  "Notification sent!!!\n" +
                                                  "-----------------");
                            }
                        }
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
