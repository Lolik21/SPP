
namespace UnitTestForLogger
{
    using System;
    using System.IO;

    using Logger;
    using NUnit.Framework;

    [TestFixture]
    public class UnitTest
    {
        private string TEMP_PATH = AppDomain.CurrentDomain.BaseDirectory;


        [Test]
        public void TestMethodForTask1()
        {
            var result = Logger.GetSortedUrls();
            Logger.WriteToFile(result, TEMP_PATH + "/Task1.csv");
            Assert.IsTrue(File.Exists(TEMP_PATH + "/Task1.csv"));
        }

        [Test]
        public void TestMethodForTask2()
        {
            var result = Logger.GetSortedIpWithUrl("http://www.apple.com");
            Logger.WriteToFile(result, TEMP_PATH + "/Task2.csv");
            Assert.IsTrue(File.Exists(TEMP_PATH + "/Task2.csv"));
        }

        [Test]
        public void TestMethodForTask3()
        {
            var result = Logger.GetSortedUrlsWithPeriod(new DateTime(2017, 1, 1), DateTime.Now);
            Logger.WriteToFile(result, TEMP_PATH + "/Task3.csv");
            Assert.IsTrue(File.Exists(TEMP_PATH + "/Task3.csv"));
        }

        [Test]
        public void TestMethodForTask4()
        {
            var result = Logger.GetSortedUrlsWithEntranceIp("46.215.142.205");
            Logger.WriteToFile(result, TEMP_PATH + "/Task4.csv");
            Assert.IsTrue(File.Exists(TEMP_PATH + "/Task4.csv"));
        }

        [Test]
        public void TestMethodForTask5()
        {
            var result = Logger.MapReduceUrlsSumTime();
            Logger.WriteToFile(result, TEMP_PATH + "/Task5.csv");
            Assert.IsTrue(File.Exists(TEMP_PATH + "/Task5.csv"));
        }

        [Test]
        public void TestMethodForTask6()
        {
            var result = Logger.MapReduceUrlsSumCount();
            Logger.WriteToFile(result, TEMP_PATH + "/Task6.csv");
            Assert.IsTrue(File.Exists(TEMP_PATH + "/Task6.csv"));
        }

        [Test]
        public void TestMethodForTask7()
        {
            var result = Logger.MapReduceByTime(new DateTime(2017, 1, 1), DateTime.Now);
            Logger.WriteToFile(result, TEMP_PATH + "/Task7.csv");
            Assert.IsTrue(File.Exists(TEMP_PATH + "/Task7.csv"));
        }

        [Test]
        public void TestMethodForTask8()
        {
            var result = Logger.MapReduceByIp();
            Logger.WriteToFile(result, TEMP_PATH + "/Task8.csv");
            Assert.IsTrue(File.Exists(TEMP_PATH + "/Task8.csv"));
        }
    }
}
