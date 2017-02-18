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
using System.ComponentModel.DataAnnotations;

namespace Registration.ReadModel
{
    public class Conference
    {
        [Key]
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public string Tagline { get; set; }

        public string TwitterSearch { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public bool IsPublished { get; set; }

        public Conference(Guid id, string code, string name, string description, string location, string tagline, string twitterSearch, DateTimeOffset startDate, IEnumerable<SeatType> seats)
        {
            Id = id;
            Code = code;
            Name = name;
            Description = description;
            Location = location;
            Tagline = tagline;
            TwitterSearch = twitterSearch;
            StartDate = startDate;
        }

        protected Conference() { }
    }
}