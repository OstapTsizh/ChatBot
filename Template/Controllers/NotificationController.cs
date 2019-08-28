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
using Newtonsoft.Json;

namespace StuddyBot.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        //private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly StuddyBotContext _db = new StuddyBotContext();
        private readonly IEmailSender _emailSender;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IDecisionMaker _decisionMaker;
        private string message;

        public NotificationController(IBotFrameworkHttpAdapter adapter,
                                      ICredentialProvider credentials,
                                      //ConcurrentDictionary<string, ConversationReference> conversationReferences,
                                      StuddyBotContext db,
                                      IEmailSender emailSender,
                                      ISubscriptionManager subscriptionManager,
                                      IDecisionMaker decisionMaker)
        {
            _adapter = adapter;
            //_conversationReferences = conversationReferences;
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
            var courses = _db.Courses.Where(s => s.RegistrationStartDate.ToShortDateString() == DateTime.Today.ToShortDateString());

            if (courses.Any())
            {
                foreach (var course in courses)
                {
                    // This is for notification on email and chat
                    var matchedCourses = _db.UserCourses.Where(item => item.CourseId == course.Id);
                    
                    if (matchedCourses.Any())
                    {
                        foreach (var matchedCourse in matchedCourses)
                        {
                            var user = _db.User.First(u => u.Id == matchedCourse.UserId);
                            
                            if (!matchedCourse.Notified)
                            {
                                var language = user.Language;
                                var dialogsMUI = _decisionMaker.GetDialogsMui(language);
                                var courseByName = _decisionMaker.GetCourses(language)
                                    .First(item => item.Name == matchedCourse.Course.Name);

                                var msg = $"<h3>{dialogsMUI.SubscriptionDictionary["title"]}</h3> <br>" +
                                              $"<h5>{dialogsMUI.SubscriptionDictionary["info"]} {matchedCourse.Course.Name}," +
                                              $" {dialogsMUI.SubscriptionDictionary["registrationStarts"]} {matchedCourse.Course.RegistrationStartDate.ToShortDateString()}" +
                                              $" {dialogsMUI.SubscriptionDictionary["courseStarts"]} {matchedCourse.Course.StartDate.ToShortDateString()}. <br> </h5>" +
                                              $"{courseByName.Resources}";

                                // Notifying into the chat
                                if (!string.IsNullOrEmpty(user.ConversationReference))
                                {
                                    message = $"**{dialogsMUI.SubscriptionDictionary["subject"]}**\n\n" +
                                              $"{dialogsMUI.SubscriptionDictionary["title"]}\n\n" +
                                              $"{dialogsMUI.SubscriptionDictionary["info"]} {matchedCourse.Course.Name}," +
                                              $" {dialogsMUI.SubscriptionDictionary["registrationStarts"]} {matchedCourse.Course.RegistrationStartDate.ToShortDateString()}" +
                                              $" {dialogsMUI.SubscriptionDictionary["courseStarts"]} {matchedCourse.Course.StartDate.ToShortDateString()}.\n\n" +
                                              $"{courseByName.Resources}";

                                    var conversationReference = JsonConvert.DeserializeObject<ConversationReference>(user.ConversationReference);
                                    await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
                                }

                                message = string.Empty;

                                // Notifying into the mail
                                if (!string.IsNullOrEmpty(user.Email))
                                {
                                    await _emailSender.SendEmailAsync(matchedCourse.User.Email,
                                    dialogsMUI.SubscriptionDictionary["subject"],
                                        msg);
                                }

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
            var step = turnContext.Responded;
            await turnContext.SendActivityAsync(message);
            //await turnContext.SendActivityAsync(step.AsMessageActivity());

        }

    }
}
