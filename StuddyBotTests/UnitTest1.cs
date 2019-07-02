using DecisionMakers;
using NUnit.Framework;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace StuddyBot.Tests
{
    [TestFixture]
    public class DecisionMakerTests
    {
        DecisionMaker _decisionMaker;

        [SetUp]
        public void Setup()
        {
            _decisionMaker = new DecisionMaker
            {
                Path = "../../../Dialogs.json"
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
                Decisions = new List<DecisionModel> {
                    new DecisionModel {
                        Answer = "We have super hero course for you",
                        Meta = new string[]{".net","1-2 hours","novice"},
                        Resources = "epam.learn.com/super-hero/courses/web-api"
                    },
                    new DecisionModel {
                        Answer = "You are lucky! here is your course",
                        Meta = new string[]{"Azure","1 hour per day","expert"},
                        Resources = "epam.learn.com/death-match/azure"
                    }
                },
                Questions = new List<Question>
                {
                    new Question {
                        IsAnswered = "false",
                        Keywords = new string[] {".net","java","python"},
                        Text = "What technology do you interested in?"
                    },
                    new Question {
                        IsAnswered = "false",
                        Keywords = new string[] {"1-2 hours","3-5 hours","6+ hours"},
                        Text = "How much time are you ready to spend on study (per day)?"
                    },
                    new Question {
                        IsAnswered = "false",
                        Keywords = new string[] {"novice","intermediate","expert"},
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
                Decisions = new List<DecisionModel> {
                    new DecisionModel {
                        Answer = "We have super hero course for you",
                        Meta = new string[]{".net","1-2 hours","novice"},
                        Resources = "epam.learn.com/super-hero/courses/web-api"
                    }
                },
                Questions = new List<Question>
                {
                }
            };

            var userAnswers = new List<string> { ".net", "1-2 hours", "novice" };

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
                Decisions = new List<DecisionModel> {
                    new DecisionModel {
                        Answer = "You are lucky! here is your course",
                        Meta = new string[]{"Azure","1 hour per day","expert"},
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
}