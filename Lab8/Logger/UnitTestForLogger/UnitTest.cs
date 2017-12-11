
namespace UnitTestForLogger
{
    using Logger;
    using NUnit.Framework;

    [TestFixture]
    public class UnitTest
    {

        [Test]
        public void TestMethod1()
        {
            Logger logger = new Logger();
            logger.DoSomething();
        }
    }
}
