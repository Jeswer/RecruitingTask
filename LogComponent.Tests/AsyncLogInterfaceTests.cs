namespace LogComponent.Tests
{
    using System;
    using System.IO;
    using System.Threading;
    using LogTest;
    using Xunit;

    public class AsyncLogInterfaceTests
    {
        // Is something being written?
        [Fact]
        public void LogTest()
        {
            var logger = new AsyncLogInterface();
            logger.WriteLog("Test Entry");
            logger.Stop_With_Flush();
            Thread.Sleep(20);

            var files = Directory.GetFiles("./LogTest", "*.log");
            Assert.True(files.Length > 0);

            var content = File.ReadAllText(files[^1]);
            Assert.Contains("Test Entry", content);
        }

        // Does the midnight check work?
        [Fact]
        public void MidnightTest()
        {
            var logger = new AsyncLogInterface();
            int filesBefore = Directory.GetFiles("./LogTest", "*.log").Length;

            // Simulate midnight by forcing _curDate back a day via reflection
            var field = typeof(AsyncLogInterface)
                .GetField("_curDate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(logger, DateTime.Now.AddDays(-1));

            logger.WriteLog("After midnight");
            Thread.Sleep(200);

            int filesAfter = Directory.GetFiles("./LogTest", "*.log").Length;
            logger.Stop_With_Flush();

            Assert.True(filesAfter > filesBefore, "Expected a new log file after midnight crossing");
        }
    
    // Stop with flush working?
    [Fact]
        public void StopWithFlushTest()
        {
            var logger = new AsyncLogInterface();
            for (int i = 0; i < 10; i++)
                logger.WriteLog("Flush entry " + i);

            logger.Stop_With_Flush();
            Thread.Sleep(200);

            var files = Directory.GetFiles("./LogTest", "*.log");
            var content = File.ReadAllText(files[^1]);

            for (int i = 0; i < 10; i++)
                Assert.Contains("Flush entry " + i, content);
        }
        // stop without flush working?
        [Fact]
        public void StopWithoutFlushTest()
        {
            var logger = new AsyncLogInterface();
            for (int i = 0; i < 100; i++)
                logger.WriteLog("NoFlush entry " + i);

            logger.Stop_Without_Flush();
            Thread.Sleep(200);

            var files = Directory.GetFiles("./LogTest", "*.log");
            var content = File.ReadAllText(files[^1]);

            // Not all 100 entries should be present - some were discarded
            int count = 0;
            for (int i = 0; i < 100; i++)
                if (content.Contains("NoFlush entry " + i)) count++;

            Assert.True(count < 100, "Expected some logs to be discarded");
        }
    }
}
