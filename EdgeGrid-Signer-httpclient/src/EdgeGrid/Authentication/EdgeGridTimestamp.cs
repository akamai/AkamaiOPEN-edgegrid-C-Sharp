using System;
using System.Globalization;

namespace AkamaiEdgeGrid.EdgeGrid
{
    public class EdgeGridTimestamp
    {

        private const string FORMAT = "yyyyMMddTHH:mm:ss+0000";
        private TimeSpan timestampInterval;
        private DateTime timestamp;

        public EdgeGridTimestamp()
        {
            this.timestamp = DateTime.UtcNow;
            this.timestampInterval = new TimeSpan(0, 0, 10);
        }

        public EdgeGridTimestamp(string timestamp)
        {
            DateTime dateValue;
            if(DateTime.TryParseExact(timestamp, FORMAT, null, DateTimeStyles.None, out dateValue))
            {
                this.timestamp = dateValue;
                this.timestampInterval = new TimeSpan(0, 0, 10);
            }
            else
            {
                throw new ArgumentException("Invalid timestamp format");
            }
        }

        public TimeSpan TimestampInterval
        {
            get => this.timestampInterval;
            set
            {
                if (value == null)
                {
                    this.timestampInterval = new TimeSpan(0, 0, 10);
                }
                else
                {
                    this.timestampInterval = value;
                }
            }
        }

        public DateTime Timestamp
        {
            get => this.timestamp;
            set => this.timestamp = value;
        }

        /// <summary>
        /// Determinate if the Timestamp is valid
        /// </summary>
        /// <returns><c>true</c>, if valid timestamp, <c>false</c> otherwise.</returns>
        public bool IsValid()
        {
            bool result = false;

            DateTime now = DateTime.UtcNow;
            result = this.timestamp.Add(this.timestampInterval) >= now;

            return result;
        }

        /// <summary>
        /// Sets the valid interval
        /// </summary>
        /// <param name="timespanInterval">Timespan interval</param>
        public void SetValidFor(TimeSpan timespanInterval)
        {
            this.timestampInterval = timespanInterval;
        }

        public override string ToString()
        {
            return this.timestamp.ToString(FORMAT);
        }
    }
}
