﻿// ==============================================================================================================
// Microsoft patterns & practices
// CQRS Journey project
// ==============================================================================================================
// ©2012 Microsoft. All rights reserved. Certain content used with permission from contributors
// http://go.microsoft.com/fwlink/p/?LinkID=258575
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance 
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and limitations under the License.
// ==============================================================================================================

using System;
using System.Diagnostics;
using System.IO;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using Infrastructure.Serialization;
using Infrastructure.Sql.Messaging;
using Infrastructure.Sql.Messaging.Handling;
using Moq;
using Xunit;

namespace Infrastructure.Sql.IntegrationTests.Messaging.EventProcessorFixture
{
    public class given_event_processor
    {
        private readonly EventProcessor processor;

        private readonly Mock<IMessageReceiver> receiverMock;

        public given_event_processor()
        {
            Trace.Listeners.Clear();
            receiverMock = new Mock<IMessageReceiver>();
            processor = new EventProcessor(receiverMock.Object, CreateSerializer());
        }

        [Fact]
        public void when_starting_then_starts_receiver()
        {
            processor.Start();

            receiverMock.Verify(r => r.Start());
        }

        [Fact]
        public void when_stopping_after_starting_then_stops_receiver()
        {
            processor.Start();
            processor.Stop();

            receiverMock.Verify(r => r.Stop());
        }

        [Fact]
        public void when_receives_message_then_notifies_registered_handler()
        {
            var handlerAMock = new Mock<IEventHandler>();
            handlerAMock.As<IEventHandler<Event1>>();
            handlerAMock.As<IEventHandler<Event2>>();

            var handlerBMock = new Mock<IEventHandler>();
            handlerBMock.As<IEventHandler<Event2>>();

            processor.Register(handlerAMock.Object);
            processor.Register(handlerBMock.Object);

            processor.Start();

            var event1 = new Event1 {SourceId = Guid.NewGuid()};
            var event2 = new Event2 {SourceId = Guid.NewGuid()};

            receiverMock.Raise(r => r.MessageReceived += null, new MessageReceivedEventArgs(new Message(Serialize(event1))));
            receiverMock.Raise(r => r.MessageReceived += null, new MessageReceivedEventArgs(new Message(Serialize(event2))));

            handlerAMock.As<IEventHandler<Event1>>().Verify(h => h.Handle(It.Is<Event1>(e => e.SourceId == event1.SourceId)));
            handlerAMock.As<IEventHandler<Event2>>().Verify(h => h.Handle(It.Is<Event2>(e => e.SourceId == event2.SourceId)));
            handlerBMock.As<IEventHandler<Event2>>().Verify(h => h.Handle(It.Is<Event2>(e => e.SourceId == event2.SourceId)));
        }

        [Fact]
        public void when_receives_message_then_notifies_generic_handler()
        {
            var handler = new Mock<IEventHandler>();
            handler.As<IEventHandler<IEvent>>();

            processor.Register(handler.Object);

            processor.Start();

            var event1 = new Event1 {SourceId = Guid.NewGuid()};
            var event2 = new Event2 {SourceId = Guid.NewGuid()};

            receiverMock.Raise(r => r.MessageReceived += null, new MessageReceivedEventArgs(new Message(Serialize(event1))));
            receiverMock.Raise(r => r.MessageReceived += null, new MessageReceivedEventArgs(new Message(Serialize(event2))));

            handler.As<IEventHandler<IEvent>>().Verify(h => h.Handle(It.Is<Event1>(e => e.SourceId == event1.SourceId)));
            handler.As<IEventHandler<IEvent>>().Verify(h => h.Handle(It.Is<Event2>(e => e.SourceId == event2.SourceId)));
        }

        private static string Serialize(object payload)
        {
            var serializer = CreateSerializer();

            using (var writer = new StringWriter()) {
                serializer.Serialize(writer, payload);
                return writer.ToString();
            }
        }

        private static ITextSerializer CreateSerializer()
        {
            return new JsonTextSerializer();
        }

        public class Event1 : IEvent
        {
            public Guid SourceId { get; set; }
        }

        public class Event2 : IEvent
        {
            public Guid SourceId { get; set; }
        }
    }
}