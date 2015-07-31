
namespace TankardDB.Core.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class TankardConfiguration
    {
        private TimeSpan concurrentAccessTimeout = TimeSpan.FromSeconds(10D);

        public TankardConfiguration()
        {
        }

        /// <summary>
        /// When trying to access a store that is locked, defines the maximum time to wait for access.
        /// </summary>
        public TimeSpan ConcurrentAccessTimeout
        {
            get { return this.concurrentAccessTimeout; }
            set { this.concurrentAccessTimeout = value; }
        }
    }
}
