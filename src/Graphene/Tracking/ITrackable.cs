﻿// Copyright 2013-2014 Boban Jose
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Graphene.Tracking
{
    public enum Resolution
    {
        Hour = 0,
        FifteenMinute = 1,
        ThirtyMinute = 2,
        FiveMinute = 3,
        Minute = 4
    }

    public interface ITrackable
    {
        string Name { get; }

        string Description { get; }

        Resolution MinResolution { get; }
    }
}