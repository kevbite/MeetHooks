using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using MeetHooks.Portal.Commands;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace MeetHooks.Portal
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddRouting(o => o.LowercaseUrls = true);

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o =>
                {    
                    o.LoginPath = new PathString("/account/login");
                    o.LogoutPath =  new PathString("/account/logout");
                })
                .AddOAuth("Meetup", o =>
                {
                    o.ClientId = Configuration["meetup:clientid"];
                    o.ClientSecret = Configuration["meetup:clientsecret"];
                    o.CallbackPath = new PathString("/signin-meetup");
                    o.AuthorizationEndpoint = "https://secure.meetup.com/oauth2/authorize";
                    o.TokenEndpoint = "https://secure.meetup.com/oauth2/access";
                    o.UserInformationEndpoint = "https://api.meetup.com/members/self";
                    o.ClaimsIssuer = "OAuth2-Meetup";
                    o.SaveTokens = true;

                    o.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    o.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                    o.ClaimActions.MapJsonKey(ClaimTypes.Country, "country");

                    o.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                        {
                            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                            response.EnsureSuccessStatusCode();

                            var user = JObject.Parse(await response.Content.ReadAsStringAsync());
                            
                            context.RunClaimActions(user);

                            var handler = context.HttpContext.RequestServices.GetService<UpsertUserCommandHandler>();
                            await handler.HandleAsync(new UpsertUserCommand()
                            {
                                Id = context.Identity.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value,
                                Name = context.Identity.Name,
                                Country = context.Identity.Claims.First(x => x.Type == ClaimTypes.Country).Value,
                                AccessToken = context.AccessToken,
                                RefreshToken = context.RefreshToken,
                                TokenExpiry = context.ExpiresIn.HasValue ? DateTime.UtcNow.AddSeconds(context.ExpiresIn.Value.Seconds) : DateTime.MinValue
                            });
                        }
                    };
                });

            services.AddTransient<UpsertUserCommandHandler>();
            services.AddTransient<EngineHttpClientFactory>();
            services.AddScoped<HttpClientHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

        }
    }
}
