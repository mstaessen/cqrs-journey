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
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using Conference;
using Infrastructure;
using Infrastructure.Database;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using Infrastructure.Processes;
using Infrastructure.Serialization;
using Infrastructure.Sql.Database;
using Infrastructure.Sql.Processes;
using Microsoft.Practices.Unity;
using Payments;
using Payments.Database;
using Payments.Handlers;
using Registration;
using Registration.Database;
using Registration.Handlers;
using Registration.ReadModel;
using Registration.ReadModel.Implementation;

namespace WorkerRoleCommandProcessor
{
    public sealed partial class ConferenceProcessor : IDisposable
    {
        private readonly CancellationTokenSource cancellationTokenSource;

        private readonly IUnityContainer container;

        private readonly bool instrumentationEnabled;

        private readonly List<IProcessor> processors;

        public ConferenceProcessor(bool instrumentationEnabled = false)
        {
            this.instrumentationEnabled = instrumentationEnabled;

            OnCreating();

            cancellationTokenSource = new CancellationTokenSource();
            container = CreateContainer();

            processors = container.ResolveAll<IProcessor>().ToList();
        }

        public void Start()
        {
            processors.ForEach(p => p.Start());
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();

            processors.ForEach(p => p.Stop());
        }

        private UnityContainer CreateContainer()
        {
            var container = new UnityContainer();

            // infrastructure
            container.RegisterInstance<ITextSerializer>(new JsonTextSerializer());
            container.RegisterInstance<IMetadataProvider>(new StandardMetadataProvider());

            container.RegisterType<DbContext, RegistrationProcessManagerDbContext>("registration", new TransientLifetimeManager(), new InjectionConstructor("ConferenceRegistrationProcesses"));
            container.RegisterType<IProcessManagerDataContext<RegistrationProcessManager>, SqlProcessManagerDataContext<RegistrationProcessManager>>(
                new TransientLifetimeManager(),
                new InjectionConstructor(new ResolvedParameter<Func<DbContext>>("registration"), typeof(ICommandBus), typeof(ITextSerializer)));

            container.RegisterType<DbContext, PaymentsDbContext>("payments", new TransientLifetimeManager(), new InjectionConstructor("Payments"));
            container.RegisterType<IDataContext<ThirdPartyProcessorPayment>, SqlDataContext<ThirdPartyProcessorPayment>>(
                new TransientLifetimeManager(),
                new InjectionConstructor(new ResolvedParameter<Func<DbContext>>("payments"), typeof(IEventBus)));

            container.RegisterType<ConferenceRegistrationDbContext>(new TransientLifetimeManager(), new InjectionConstructor("ConferenceRegistration"));

            container.RegisterType<IConferenceDao, ConferenceDao>(new ContainerControlledLifetimeManager());
            container.RegisterType<IOrderDao, OrderDao>(new ContainerControlledLifetimeManager());

            container.RegisterType<IPricingService, PricingService>(new ContainerControlledLifetimeManager());

            // handlers
            container.RegisterType<ICommandHandler, RegistrationProcessManagerRouter>("RegistrationProcessManagerRouter");
            container.RegisterType<ICommandHandler, OrderCommandHandler>("OrderCommandHandler");
            container.RegisterType<ICommandHandler, SeatsAvailabilityHandler>("SeatsAvailabilityHandler");
            container.RegisterType<ICommandHandler, ThirdPartyProcessorPaymentCommandHandler>("ThirdPartyProcessorPaymentCommandHandler");
            container.RegisterType<ICommandHandler, SeatAssignmentsHandler>("SeatAssignmentsHandler");

            // Conference management integration
            container.RegisterType<ConferenceContext>(new TransientLifetimeManager(), new InjectionConstructor("ConferenceManagement"));

            OnCreateContainer(container);

            return container;
        }

        partial void OnCreating();

        partial void OnCreateContainer(UnityContainer container);

        public void Dispose()
        {
            container.Dispose();
            cancellationTokenSource.Dispose();
        }
    }
}