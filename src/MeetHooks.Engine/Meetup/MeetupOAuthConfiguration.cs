using System;

namespace MeetHooks.Engine.Meetup
{
    public class MeetupOAuthConfiguration
    {
        public Uri BaseUri { get; } =
            new Uri(Environment.GetEnvironmentVariable("meetup:oauthbaseuri"));

        public string ClientId { get; } =
            Environment.GetEnvironmentVariable("meetup:clientid");

        public string ClientSecret { get; } =
            Environment.GetEnvironmentVariable("meetup:clientsecret");

    }
}