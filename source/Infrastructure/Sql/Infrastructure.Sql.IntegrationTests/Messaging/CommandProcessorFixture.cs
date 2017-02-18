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
using System.IO;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using Infrastructure.Serialization;
using Infrastructure.Sql.Messaging;
using Infrastructure.Sql.Messaging.Handling;
using Moq;
using Xunit;

namespace Infrastructure.Sql.IntegrationTests.Messaging.CommandProcessorFixture
{
    public class given_command_processor
    {
        private readonly CommandProcessor processor;

        private readonly Mock<IMessageReceiver> receiverMock;

        public given_command_processor()
        {
            receiverMock = new Mock<IMessageReceiver>();
            processor = new CommandProcessor(receiverMock.Object, CreateSerializer());
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
            var handlerAMock = new Mock<ICommandHandler>();
            handlerAMock.As<ICommandHandler<Command1>>();

            var handlerBMock = new Mock<ICommandHandler>();
            handlerBMock.As<ICommandHandler<Command2>>();

            processor.Register(handlerAMock.Object);
            processor.Register(handlerBMock.Object);

            processor.Start();

            var command1 = new Command1 {Id = Guid.NewGuid()};
            var command2 = new Command2 {Id = Guid.NewGuid()};

            receiverMock.Raise(r => r.MessageReceived += null, new MessageReceivedEventArgs(new Message(Serialize(command1))));
            receiverMock.Raise(r => r.MessageReceived += null, new MessageReceivedEventArgs(new Message(Serialize(command2))));

            handlerAMock.As<ICommandHandler<Command1>>().Verify(h => h.Handle(It.Is<Command1>(e => e.Id == command1.Id)));
            handlerBMock.As<ICommandHandler<Command2>>().Verify(h => h.Handle(It.Is<Command2>(e => e.Id == command2.Id)));
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

        public class Command1 : ICommand
        {
            public Guid Id { get; set; }
        }

        public class Command2 : ICommand
        {
            public Guid Id { get; set; }
        }
    }
}