using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetHooks.Engine
{
    public static class SystemDateTime
    {
        private static Func<DateTime> _utcNowFactory = () => DateTime.UtcNow;

        public static DateTime UtcNow => _utcNowFactory();

        public static void Reset()
        {
            _utcNowFactory = () => DateTime.UtcNow;
        }

        public static void Set(DateTime dateTime)
        {
            _utcNowFactory = () => dateTime;
        }
    }
}
