using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DecisionMakers;
using LoggerService;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using StuddyBot.Core.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using StuddyBot.Bots;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using StuddyBot.Dialogs;

namespace StuddyBot.Tests
{
    [TestFixture]
    public class DecisionMakerTests
    {
        private DecisionMaker _decisionMaker;
        private QuestionAndAnswerModel qaModel;
        private IOptions<PathSettings> pathSettings;

        [SetUp]
        public void Setup()
        {
            _decisionMaker = new DecisionMaker(pathSettings)
            {
                //Path = "../../../Dialogs.json"
            };
            qaModel = new QuestionAndAnswerModel()
            {
                Answers = new List<string> { ".net", "1-2 hours", "novice" },
                QuestionModel = new QuestionModel()
            };

        }

        [Test]
        public void Should_ReturnStartTopicsList_When_CallGetStartTopicsMethod()
        {
            //Arrange 
            var expectedResult = new List<string>
            {
                "courses",
                "job"
            };

            //Act
            var actualResult = _decisionMaker.GetStartTopics();

            //Assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        public void Should_ReturnQuestionOrResult_When_TopicExist()
        {
            //Arrange 
            var expectedResult = new QuestionModel
            {
                Keywords = new string[] { "courses", "learn", "study" },
                Name = "Searching .Net courses",
                Decisions = new List<DecisionModel>
                {
                    new DecisionModel
                    {
                        Answer = "We have super hero course for you",
                        Meta = new string[] {".net", "1-2 hours", "novice"},
                        Resources = "epam.learn.com/super-hero/courses/web-api"
                    },
                    new DecisionModel
                    {
                        Answer = "You are lucky! here is your course",
                        Meta = new string[] {"Azure", "1 hour per day", "expert"},
                        Resources = "epam.learn.com/death-match/azure"
                    }
                },
                Questions = new List<Question>
                {
                    new Question
                    {
                        IsAnswered = "false",
                        Keywords = new string[] {".net", "java", "python"},
                        Text = "What technology do you interested in?"
                    },
                    new Question
                    {
                        IsAnswered = "false",
                        Keywords = new string[] {"1-2 hours", "3-5 hours", "6+ hours"},
                        Text = "How much time are you ready to spend on study (per day)?"
                    },
                    new Question
                    {
                        IsAnswered = "false",
                        Keywords = new string[] {"novice", "intermediate", "expert"},
                        Text = "Select difficulty"
                    }
                }
            };

            //Act
            var actualResult = _decisionMaker.GetQuestionOrResult("courses");

            //Assert
            for (var i = 0; i < expectedResult.Decisions.Count; i++)
            {
                for (var j = 0; j < expectedResult.Decisions[i].Meta.Length; j++)
                {
                    Assert.AreEqual(expectedResult.Decisions[i].Meta[j], actualResult.Decisions[i].Meta[j]);
                }

                Assert.AreEqual(expectedResult.Decisions[i].Answer, actualResult.Decisions[i].Answer);
            }

            for (var i = 0; i < expectedResult.Questions.Count; i++)
            {
                for (var j = 0; j < expectedResult.Questions[i].Keywords.Length; j++)
                {
                    Assert.AreEqual(expectedResult.Questions[i].Keywords[j], actualResult.Questions[i].Keywords[j]);
                }

                Assert.AreEqual(expectedResult.Questions[i].Text, actualResult.Questions[i].Text);
            }
        }

        [Test]
        public void Should_ReturnDecision_When_DecisionExist()
        {
            //Arrange 
            var qModel = new QuestionModel
            {
                Keywords = new string[] { "courses", "learn", "study" },
                Name = "Searching .Net courses",
                Decisions = new List<DecisionModel>
                {
                    new DecisionModel
                    {
                        Answer = "We have super hero course for you",
                        Meta = new string[] {".net", "1-2 hours", "novice"},
                        Resources = "epam.learn.com/super-hero/courses/web-api"
                    }
                },
                Questions = new List<Question>
                {
                }
            };

            var userAnswers = qaModel.Answers;

            var expectedResult = new DecisionModel
            {
                Answer = "We have super hero course for you",
                Meta = new string[] { ".net", "1-2 hours", "novice" },
                Resources = "epam.learn.com/super-hero/courses/web-api"
            };

            //Act
            var actualResult = _decisionMaker.GetDecision(userAnswers, qModel);

            //Assert
            Assert.AreEqual(expectedResult.Answer, actualResult.Answer);
            Assert.AreEqual(expectedResult.Resources, actualResult.Resources);
            Assert.AreEqual(expectedResult.Meta.ToString(), actualResult.Meta.ToString());
        }

        [Test]
        public void Should_ReturnNull_When_DecisionDoNotExist()
        {
            //Arrange 
            var qModel = new QuestionModel
            {
                Keywords = new string[] { "courses", "learn", "study" },
                Name = "Searching .Net courses",
                Decisions = new List<DecisionModel>
                {
                    new DecisionModel
                    {
                        Answer = "You are lucky! here is your course",
                        Meta = new string[] {"Azure", "1 hour per day", "expert"},
                        Resources = "epam.learn.com/death-match/azure"
                    }
                },
                Questions = new List<Question>
                {
                }
            };

            var userAnswers = new List<string> { "ruby", "2 hours", "novice" };

            //Act
            var actualResult = _decisionMaker.GetDecision(userAnswers, qModel);

            //Assert
            Assert.IsNull(actualResult);
        }
    }

    [TestFixture]
    public class StartUpTest
    {
        public class TestController
        { }


        [Test]
        public void Should_RegistersDependenciesCorrectly_When_ConfigureServices()
        {
            //  Arrange

            Mock<IConfigurationSection> configurationSectionStub = new Mock<IConfigurationSection>();
            configurationSectionStub.Setup(x => x["DefaultConnection"]).Returns("TestConnectionString");
            Mock<IConfiguration> configurationStub = new Mock<IConfiguration>();
            configurationStub.Setup(x => x.GetSection("ConnectionStrings")).Returns(configurationSectionStub.Object);

            IServiceCollection services = new ServiceCollection();
            var target = new Startup();

            //  Act

            target.ConfigureServices(services);
            services.AddTransient<TestController>();

            //  Assert

            var serviceProvider = services.BuildServiceProvider();

            var controller = serviceProvider.GetService<TestController>();
            Assert.IsNotNull(controller);
        }
    }

    [TestFixture]
    public class TurnContextTest
    {
        private TestAdapter adapter;
        private Activity activity;

        [SetUp]
        public async Task SetUp()
        {
            adapter = new TestAdapter();

            activity = new Activity
            {
                Id = "1",
                ChannelId = "1",
                Text = "hi",
                Type = ActivityTypes.Message,
                Conversation = new ConversationAccount(id: "1")
            };
        }

        [Test]
        public async Task Should_CreateContext_When_ConstructContext()
        {
            //Act
            var turnContext = new TurnContext(adapter, activity);

            //Assert
            Assert.IsNotNull(turnContext);
        }

        [Test]
        public async Task Should_ChacheValue_When_UsingGetAndSet()
        {
            //Arrange
            var expectedResult = "hi";

            //Act
            var turnContext = new TurnContext(adapter, activity);
            turnContext.TurnState.Add("message", "hi");

            var actualResult = turnContext.TurnState.Get<string>("message");

            //Assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        public async Task Should_SetRequest()
        {
            //Act
            var turnContext = new TurnContext(adapter, activity);

            //Assert
            Assert.AreEqual("1", turnContext.Activity.Id);
        }

        [Test]
        public async Task Should_SetResponded_When_Send()
        {
            //Arrange
            var turnContext = new TurnContext(adapter, activity);
            Assert.IsFalse(turnContext.Responded);

            //Act
            var response = await turnContext.SendActivitiesAsync(new IActivity[] { MessageFactory.Text("message1"), MessageFactory.Text("message2") });

            //Assert
            Assert.IsTrue(turnContext.Responded);
            Assert.AreEqual(2, response.Length);
        }
    }

    [TestFixture]
    public class ActivityTest
    {
        private Activity CreateActivity()
        {
            var account1 = new ChannelAccount
            {
                Id = "ChannelAccount_Id_1",
                Name = "ChannelAccount_Name_1",
                Properties = new JObject { { "Name", "Value" } },
            };

            var account2 = new ChannelAccount
            {
                Id = "ChannelAccount_Id_2",
                Name = "ChannelAccount_Name_2",
                Properties = new JObject { { "Name", "Value" } },
            };

            var conversationAccount = new ConversationAccount
            {
                ConversationType = "a",
                Id = "123",
                IsGroup = true,
                Name = "Name",
                Properties = new JObject { { "Name", "Value" } },
            };

            var activity = new Activity
            {
                Id = "123",
                From = account1,
                Recipient = account2,
                Conversation = conversationAccount,
                ChannelId = "ChannelId123",
                ServiceUrl = "ServiceUrl123",
            };

            return activity;
        }

        [Test]
        public void Should_ReturnConversationReference_When_GetIt()
        {
            //Arrange
            var activity = CreateActivity();

            //Act
            var conversationReference = activity.GetConversationReference();

            //Assert
            Assert.AreEqual(activity.Id, conversationReference.ActivityId);
            Assert.AreEqual(activity.From.Id, conversationReference.User.Id);
            Assert.AreEqual(activity.Recipient.Id, conversationReference.Bot.Id);
            Assert.AreEqual(activity.Conversation.Id, conversationReference.Conversation.Id);
            Assert.AreEqual(activity.ChannelId, conversationReference.ChannelId);
            Assert.AreEqual(activity.ServiceUrl, conversationReference.ServiceUrl);
        }

        [Test]
        public void Should_ApplyConversationReference_When_Incoming()
        {
            //Arrange
            var activity = CreateActivity();

            var reference = new ConversationReference
            {
                ChannelId = "2",
                ServiceUrl = "serviceUrl",
                Conversation = new ConversationAccount
                {
                    Id = "3",
                },
                User = new ChannelAccount
                {
                    Id = "4",
                },
                Bot = new ChannelAccount
                {
                    Id = "5",
                },
                ActivityId = "6",
            };

            //Act
            activity.ApplyConversationReference(reference, true);

            //Assert
            Assert.AreEqual(reference.ChannelId, activity.ChannelId);
            Assert.AreEqual(reference.ServiceUrl, activity.ServiceUrl);
            Assert.AreEqual(reference.Conversation.Id, activity.Conversation.Id);

            Assert.AreEqual(reference.User.Id, activity.From.Id);
            Assert.AreEqual(reference.Bot.Id, activity.Recipient.Id);
            Assert.AreEqual(reference.ActivityId, activity.Id);
        }

    }

    [TestFixture]
    public class DialogAndWelcomeBotTest
    {
        private MemoryStorage memoryStorage;
        private Mock<ConversationState> conversationStateMock;
        private Mock<UserState> userStateMock;
        private Mock<Dialog> dialogMock;
        private Mock<ILogger<DialogAndWelcomeBot<Dialog>>> loggerMock;
        private Mock<DecisionMaker> decisionMakerMock;
        private Mock<ThreadedLogger> threadedLoggerMock;

        [SetUp]
        public void SetUp()
        {
            memoryStorage = new MemoryStorage();
            conversationStateMock = new Mock<ConversationState>(memoryStorage)
            {
                CallBase = true,
            };
                
            userStateMock = new Mock<UserState>(memoryStorage)
            {
                CallBase = true,
            };

            dialogMock = new Mock<Dialog>("dialogMock");
            dialogMock
                .Setup(x => x.ContinueDialogAsync(It.IsAny<DialogContext>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new DialogTurnResult(DialogTurnStatus.Empty)));

            loggerMock = new Mock<ILogger<DialogAndWelcomeBot<Dialog>>>();
            loggerMock.Setup(x =>
                x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()));

            decisionMakerMock = new Mock<DecisionMaker>();

           threadedLoggerMock = new Mock<ThreadedLogger>();
        }

        [Test]
        public async Task Should_CreateBot_When_Construct()
        {
            //Act
            var welcomeBot = new DialogAndWelcomeBot<Dialog>(conversationStateMock.Object, userStateMock.Object,
                dialogMock.Object, loggerMock.Object, decisionMakerMock.Object, threadedLoggerMock.Object);

            //Assert
            Assert.IsNotNull(welcomeBot);
        }

        [Test]
        public async Task Should_SaveTurnState_When_SaveChangesAsync()
        {
            //Assert
           var welcomeBot = new DialogAndWelcomeBot<Dialog>(conversationStateMock.Object, userStateMock.Object,
               dialogMock.Object, loggerMock.Object, decisionMakerMock.Object, threadedLoggerMock.Object);

            //Act
            var adapter = new TestAdapter();
            var testFlow = new TestFlow(adapter, welcomeBot);

            await testFlow.Send("message").StartTestAsync();

            //Assert
            conversationStateMock.Verify(x => x.SaveChangesAsync(It.IsAny<TurnContext>(),
                It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
            userStateMock.Verify(x => x.SaveChangesAsync(It.IsAny<TurnContext>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Test()
        {
            //Assert
            var welcomeBot = new DialogAndWelcomeBot<Dialog>(conversationStateMock.Object, userStateMock.Object,
                dialogMock.Object, loggerMock.Object, decisionMakerMock.Object, threadedLoggerMock.Object);

            var logger = new Mock<ILogger<DialogAndWelcomeBot<Dialog>>>();
            logger.Setup(x =>
                x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), null,
                    It.IsAny<Func<object, Exception, string>>()));

            //Act
            var adapter = new TestAdapter();
            var testFlow = new TestFlow(adapter, welcomeBot);

            await testFlow.Send("message").StartTestAsync();

            //Assert
           /* logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<object>(o => o.ToString() == ""),
                    null,
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);*/
        }
    }
}