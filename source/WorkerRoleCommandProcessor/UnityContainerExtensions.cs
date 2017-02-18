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

using Infrastructure;
using Infrastructure.Azure.Messaging;
using Infrastructure.Messaging.Handling;
using Infrastructure.Serialization;
using Microsoft.Practices.Unity;

namespace WorkerRoleCommandProcessor
{
    public static class UnityContainerExtensions
    {
        public static void RegisterEventProcessor<T>(this IUnityContainer container, ServiceBusConfig busConfig, string subscriptionName, bool instrumentationEnabled = false)
            where T : IEventHandler
        {
            container.RegisterInstance<IProcessor>(subscriptionName, busConfig.CreateEventProcessor(
                subscriptionName,
                container.Resolve<T>(),
                container.Resolve<ITextSerializer>(),
                instrumentationEnabled));
        }
    }
}