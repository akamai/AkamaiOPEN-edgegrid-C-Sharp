using System;
using NUnit.Framework;
using Akamai.EdgeGrid;

namespace Tests
{
    public class EdgeGridTimestampTest
    {
        [Test]
        public void TestTimestampFormat()
        {
            var Timestamp = new EdgeGridTimestamp();
            var Check = DateTime.UtcNow.ToString("yyyyMMddTHH:mm:ss+0000");
            Assert.AreEqual(Check, Timestamp.ToString());
        }

        [Test]
        public void TestIsValid()
        {
            var Timestamp = new EdgeGridTimestamp();
            Assert.IsTrue(Timestamp.IsValid());
            var TimestampInterval = new TimeSpan(0, 0, 0);
            Timestamp.SetValidFor(TimestampInterval);
            System.Threading.Thread.Sleep(1000);
            Assert.IsFalse(Timestamp.IsValid());
        }
    }
}
