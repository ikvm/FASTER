﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using FASTER.core;
using System.IO;
using NUnit.Framework;
using FASTER.test.recovery.sumstore;
using System.Threading.Tasks;

namespace FASTER.test.async
{

    [TestFixture]
    public class SimpleAsyncTests
    {
        IDevice log;
        FasterKV<long, long> fht1;
        const int numOps = 5000;
        AdId[] inputArray;
        string path;

        [SetUp]
        public void Setup()
        {
            inputArray = new AdId[numOps];
            for (int i = 0; i < numOps; i++)
            {
                inputArray[i].adId = i;
            }

            path = TestContext.CurrentContext.TestDirectory + "\\SimpleAsyncTests\\";
            log = Devices.CreateLogDevice(path + "hlog.log", deleteOnClose: true);
            Directory.CreateDirectory(path);
            fht1 = new FasterKV<long, long>
                (1L << 10,
                logSettings: new LogSettings { LogDevice = log, MutableFraction = 1, PageSizeBits = 10, MemorySizeBits = 15 },
                checkpointSettings: new CheckpointSettings { CheckpointDir = path }
                );
        }

        [TearDown]
        public void TearDown()
        {
            fht1.Dispose();
            log.Dispose();
            new DirectoryInfo(path).Delete(true);
        }


        [Test]
        public async Task SimpleAsyncTest1()
        {
            using var s1 = fht1.NewSession(new SimpleFunctions<long, long>());
            for (long key = 1; key < numOps; key++)
            {
                s1.Upsert(ref key, ref key);
            }

            for (long key = 1; key < numOps; key++)
            {
                Status status;
                long output = default;
                (status, output) = (await s1.ReadAsync(ref key, ref output)).CompleteRead();
                Assert.IsTrue(status == Status.OK && output == key);
            }
        }
    }
}