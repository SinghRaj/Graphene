﻿// Copyright 2013-2014 Boban Jose
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Graphene.Configuration;
using Graphene.Data;
using Graphene.Publishing;
using Graphene.Tracking;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Graphene.Tests
{
    public class FakePersister : IPersist
    {
        private object _lock = new object();
        private List<TrackerData> _trackingDate = new List<TrackerData>();

        public async void Persist(TrackerData trackerData)
        {
        }
    }

    public class CustomerAgeTracker : ITrackable
    {
        public long KidsCount { get; set; }
        public long MiddleAgedCount { get; set; }
        public long ElderlyCount { get; set; }

        public string Name
        {
            get { return "Customer Age Tracker"; }
        }

        public string Description
        {
            get { return "Counts the number of customer visits"; }
        }

        public Resolution MinResolution
        {
            get { return Resolution.Hour; }
        }
    }

    public class CustomerVisitTracker : ITrackable
    {
        public string Name
        {
            get { return "Customer Visit Tracker"; }
        }

        public string Description
        {
            get { return "Counts the number of customer visits"; }
        }

        public Resolution MinResolution
        {
            get { return Resolution.Hour; }
        }
    }

    public class CustomerPurchaseTracker : ITrackable
    {
        public string Name
        {
            get { return "Customer Purchase Tracker"; }
        }

        public string Description
        {
            get { return "Counts the number of customer purchases"; }
        }

        public Resolution MinResolution
        {
            get { return Resolution.Hour; }
        }
    }

    public class PerformanceTracker : ITrackable
    {
        public int NumberOfCalls { get; set; }

        public long TotalResponseTimeInMilliseconds { get; set; }

        public string Name
        {
            get { return "Method A Performance Tracker"; }
        }

        public string Description
        {
            get { return "Tracks the response time of ### method"; }
        }

        public Resolution MinResolution
        {
            get { return Resolution.FiveMinute; }
        }
    }

    public struct CustomerFilter
    {
        public string State { get; set; }
        public string StoreID { get; set; }
        public string Gender { get; set; }
        public string Environment_ServerName { get; set; }
    }

    public struct EnvironmentFilter
    {
        public string ServerName { get; set; }
    }

    [TestClass]
    public class TrackerTest
    {
        private static int _task1Count;
        private static int _task2Count;
        private static int _task3Count;


        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            Configurator.Initialize(
                new Settings {Persister = new PersistToMongo("mongodb://localhost/Graphene")}
                );
        }

        [ClassCleanup]
        public static void ShutDown()
        {
            Configurator.ShutDown();
        }

        [TestMethod]
        public void TestEmpty()
        {
        }

        [TestMethod]
        public void TestIncrement()
        {
            var ct = new CancellationTokenSource();

            Task task1 = Task.Run(() =>
            {
                while (!ct.IsCancellationRequested)
                {
                    //Graphene.Tracking.Container<PatientLinkValidationTracker>.IncrementBy(1);
                    Container<CustomerVisitTracker>
                        .Where(
                            new CustomerFilter
                            {
                                State = "CA",
                                StoreID = "3234",
                                Environment_ServerName = "Server1"
                            }).IncrementBy(1);
                    _task1Count++;
                    // System.Threading.Thread.Sleep(500);
                }
            }, ct.Token);

            Task task2 = Task.Run(() =>
            {
                while (!ct.IsCancellationRequested)
                {
                    Container<CustomerPurchaseTracker>
                        .Where(
                            new CustomerFilter
                            {
                                State = "MN",
                                StoreID = "334",
                                Environment_ServerName = "Server2"
                            }).IncrementBy(1);
                    _task2Count++;
                    // System.Threading.Thread.Sleep(100);
                }
            }, ct.Token);

            Task task3 = Task.Run(() =>
            {
                while (!ct.IsCancellationRequested)
                {
                    Container<CustomerVisitTracker>.IncrementBy(3);
                    _task3Count++;
                    //System.Threading.Thread.Sleep(500);
                }
            }, ct.Token);

            Thread.Sleep(1000);

            ct.Cancel();

            Task.WaitAll(task1, task2, task3);
        }

        [TestMethod]
        public void TestNamedMetrics()
        {
            var ct = new CancellationTokenSource();

            Task task1 = Task.Run(() =>
            {
                while (!ct.IsCancellationRequested)
                {
                    Container<CustomerAgeTracker>
                        .Where(
                            new CustomerFilter
                            {
                                State = "MN",
                                StoreID = "334",
                                Environment_ServerName = "Server2"
                            })
                        .Increment(e => e.MiddleAgedCount, 1)
                        .Increment(e => e.ElderlyCount, 2);
                }
            }, ct.Token);

            Task task2 = Task.Run(() =>
            {
                while (!ct.IsCancellationRequested)
                {
                    Container<CustomerAgeTracker>
                        .Increment(e => e.KidsCount, 2);
                }
            }, ct.Token);


            Thread.Sleep(1000);

            ct.Cancel();

            Task.WaitAll(task1, task2);
        }

        [TestMethod]
        public void TestPerformaceMetrics()
        {
            var ct = new CancellationTokenSource();

            Task task1 = Task.Run(() =>
            {
                while (!ct.IsCancellationRequested)
                {
                    Stopwatch sw = Stopwatch.StartNew();

                    Thread.Sleep(Convert.ToInt32((new Random()).NextDouble()*100) + 5);

                    sw.Stop();

                    Container<PerformanceTracker>
                        .Where(
                            new EnvironmentFilter
                            {
                                ServerName = "Server2"
                            })
                        .Increment(e => e.NumberOfCalls, 1)
                        .Increment(e => e.TotalResponseTimeInMilliseconds, sw);

                    sw.Reset();
                    sw.Start();
                    Thread.Sleep(Convert.ToInt32((new Random()).NextDouble()*100) + 5);
                    sw.Stop();

                    Container<PerformanceTracker>
                        .Where(
                            new EnvironmentFilter
                            {
                                ServerName = "Server1"
                            })
                        .Increment(e => e.NumberOfCalls, 1)
                        .Increment(e => e.TotalResponseTimeInMilliseconds, sw);
                }
            }, ct.Token);

            Task task2 = Task.Run(() =>
            {
                while (!ct.IsCancellationRequested)
                {
                    Stopwatch sw = Stopwatch.StartNew();

                    Thread.Sleep(Convert.ToInt32((new Random()).NextDouble()*100) + 5);

                    sw.Stop();

                    Container<PerformanceTracker>
                        .Increment(e => e.NumberOfCalls, 1)
                        .Increment(e => e.TotalResponseTimeInMilliseconds, sw);
                }
            }, ct.Token);


            Thread.Sleep(1000);

            ct.Cancel();

            Task.WaitAll(task1, task2);
        }
    }
}