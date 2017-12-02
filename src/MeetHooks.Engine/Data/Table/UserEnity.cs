using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace MeetHooks.Engine.Data.Table
{
    public class UserEnity : TableEntity
    {
        public string Name { get; set; }
        
        public string AccessToken { get; set; }
        
        public string RefreshToken { get; set; }

        public DateTime TokenExpiry { get; set; }
    }
}
