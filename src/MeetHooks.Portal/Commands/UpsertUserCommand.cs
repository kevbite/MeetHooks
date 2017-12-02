using System;
using System.Collections.Generic;
using System.Linq;

namespace MeetHooks.Portal.Commands
{
    public class UpsertUserCommand
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Country { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public DateTime TokenExpiry { get; set; }
    }
}
