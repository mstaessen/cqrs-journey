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
using System.Linq;
using Infrastructure.MessageLog;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Infrastructure.Azure.MessageLog
{
    public class AzureEventLogReader : IEventLogReader
    {
        private readonly CloudStorageAccount account;

        private readonly CloudTableClient tableClient;

        private readonly string tableName;

        private readonly ITextSerializer serializer;

        public AzureEventLogReader(CloudStorageAccount account, string tableName, ITextSerializer serializer)
        {
            if (account == null) {
                throw new ArgumentNullException("account");
            }
            if (tableName == null) {
                throw new ArgumentNullException("tableName");
            }
            if (string.IsNullOrWhiteSpace(tableName)) {
                throw new ArgumentException("tableName");
            }
            if (serializer == null) {
                throw new ArgumentNullException("serializer");
            }

            this.account = account;
            this.tableName = tableName;
            tableClient = account.CreateCloudTableClient();
            this.serializer = serializer;
        }

        // NOTE: we don't have a need (yet?) to query commands, as we don't use them to
        // recreate read models, nor we are using it for BI, so we just 
        // expose events.
        public IEnumerable<IEvent> Query(QueryCriteria criteria)
        {
            var context = tableClient.GetDataServiceContext();
            var query = context.CreateQuery<MessageLogEntity>(tableName)
                .Where(x => x.Kind == StandardMetadata.EventKind);

            var where = criteria.ToExpression();
            if (where != null) {
                query = query.Where(where);
            }

            return query
                .AsTableServiceQuery()
                .Execute()
                .Select(e => serializer.Deserialize<IEvent>(e.Payload));
        }
    }
}