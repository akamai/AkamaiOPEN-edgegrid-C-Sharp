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
            var timestamp = new EdgeGridTimestamp();
            var check = DateTime.UtcNow.ToString("yyyyMMddTHH:mm:ss+0000");
            Assert.AreEqual(check, timestamp.ToString());
        }

        [Test]
        public void TestIsValid()
        {
            var timestamp = new EdgeGridTimestamp();
            Assert.IsTrue(timestamp.IsValid());
            var timestampInterval = new TimeSpan(0, 0, 0);
            timestamp.SetValidFor(timestampInterval);
            System.Threading.Thread.Sleep(1000);
            Assert.IsFalse(timestamp.IsValid());
        }
    }
}
