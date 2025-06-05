// -----------------------------------------------------------------------
// <copyright file = "UtpEventSubscriber.Tests.cs" company = "IGT">
//     Copyright (c) 2016-2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using Communications;
    using Framework;

    [TestFixture]
    public class GivenUtpEventSubscriberWithEventCommand
    {
        private AutomationCommander commander;
        private UtpCommMock communication;

        [SetUp]
        public void Arrange()
        {
            var command = new AutomationCommand
            {
                Module = "Module",
                Command = "Command",
                IsEvent = true
            };
            communication = new UtpCommMock();
            commander = new AutomationCommander(communication, command);
        }

        [Test]
        public void SubscribeWithNullCommander()
        {
            Assert.Throws<ArgumentNullException>(() => UtpEventSubscriber.Subscribe(null));
        }

        [Test]
        public void SubscribeWithNonEventCommand()
        {
            commander.AutoCommand.IsEvent = false;
            Assert.IsFalse(UtpEventSubscriber.Subscribe(commander));
            Assert.IsNull(communication.LastCommand);
        }

        [Test]
        public void SubscribeWithNullParameters()
        {
            commander.AutoCommand.Parameters = null;
            Assert.IsFalse(UtpEventSubscriber.Subscribe(commander));
            Assert.IsNull(communication.LastCommand);
        }

        [Test]
        public void SubscribeWithIncorrectParameters()
        {
            var ap1 = new AutomationParameter {Name = "BadName1", Value = "EventNameValue"};
            var ap2 = new AutomationParameter {Name = "BadName2", Value = "Add"};
            var parameters = new List<AutomationParameter> {ap1, ap2};
            commander.AutoCommand.Parameters = parameters;
            Assert.IsFalse(UtpEventSubscriber.Subscribe(commander));
            Assert.IsNotNull(communication.LastCommand);
        }

        [Test]
        public void SubscribeWithExtraParameter()
        {
            var ap1 = new AutomationParameter {Name = "EventName", Value = "EventNameValue"};
            var ap2 = new AutomationParameter {Name = "Action", Value = "Add"};
            var ap3 = new AutomationParameter {Name = "ExtraParam", Value = "SomeExtraValue"};
            var parameters = new List<AutomationParameter> {ap1, ap2, ap3};
            commander.AutoCommand.Parameters = parameters;
            Assert.IsFalse(UtpEventSubscriber.Subscribe(commander));
            Assert.IsNull(communication.LastCommand);
        }

        [Test]
        public void SubscribeToEvent()
        {
            var ap1 = new AutomationParameter { Name = "EventName", Value = "EventNameValue" };
            var ap2 = new AutomationParameter {Name = "Action", Value = "Add"};
            var parameters = new List<AutomationParameter> { ap1, ap2 };
            commander.AutoCommand.Parameters = parameters;
            Assert.IsTrue(UtpEventSubscriber.Subscribe(commander));
            Assert.IsNotNull(communication.LastCommand);
            Assert.AreEqual(1, communication.LastCommand.Parameters.Count(p => p.Name == "ModuleName" && p.Value == commander.AutoCommand.Module));
            Assert.AreEqual(1, communication.LastCommand.Parameters.Count(p => p.Name == "EventName" && p.Value == "EventNameValue"));
            Assert.AreEqual(1, communication.LastCommand.Parameters.Count(p => p.Name == "Action" && p.Value == UtpEventSubscriber.SubscriptionManager.EventSubscriptionAction.Add.ToString()));
            Assert.AreEqual(1, communication.LastCommand.Parameters.Count(p => p.Name == "Result" && p.Value == true.ToString()));
            Assert.AreEqual(1, UtpEventSubscriber.UtpSubscriptions.Count);
            Assert.AreEqual(1, UtpEventSubscriber.UtpSubscriptions["Module"].GetSubscribers("EventNameValue").Count);
        }

        public class UtpCommMock : IUtpCommunication
        {
            public AutomationCommand LastCommand { get; private set; }

            public UtpCommMock()
            {
                LastCommand = null;
            }

            public bool Send(AutomationCommand command)
            {
                LastCommand = command;
                return true;
            }
        }
    }
}
