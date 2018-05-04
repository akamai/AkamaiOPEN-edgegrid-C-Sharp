using System;
using System.Globalization;

namespace Akamai.EdgeGrid
{
    public class EdgeGridTimestamp
    {

        private const string FORMAT = "yyyyMMddTHH:mm:ss+0000";
        private TimeSpan timestampInterval;
        private DateTime timestamp;

        public EdgeGridTimestamp()
        {
            timestamp = DateTime.UtcNow;
            timestampInterval = new TimeSpan(0, 0, 10);
        }

        public EdgeGridTimestamp(string timestamp)
        {
            DateTime dateValue;
            if(DateTime.TryParseExact(timestamp, FORMAT, null, DateTimeStyles.None, out dateValue))
            {
                this.timestamp = dateValue;
                timestampInterval = new TimeSpan(0, 0, 10);
            }
            else
            {
                throw new ArgumentException("Invalid timestamp format");
            }
        }

        public DateTime Timestamp
        {
            get => timestamp;
            set => timestamp = value;
        }

        /// <summary>
        /// Determinate if the Timestamp is valid
        /// </summary>
        /// <returns><c>true</c>, if valid timestamp, <c>false</c> otherwise.</returns>
        public bool IsValid()
        {
            bool Result;

            DateTime Now = DateTime.UtcNow;
            Result = timestamp.Add(timestampInterval) >= Now;

            return Result;
        }

        /// <summary>
        /// Sets the valid interval
        /// </summary>
        /// <param name="timespanInterval">Timespan interval</param>
        public void SetValidFor(TimeSpan timespanInterval)
        {
            timestampInterval = timespanInterval;
        }

        public override string ToString()
        {
            return timestamp.ToString(FORMAT);
        }
    }
}
