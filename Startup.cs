// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;

namespace SignalRHubs
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddAuthorization(option =>
            {
                option.AddPolicy("ClaimBasedAuth", policy =>
                    {
                        policy.RequireClaim(ClaimTypes.NameIdentifier);
                    });
                option.AddPolicy("PolicyBasedAuth", policy => policy.Requirements.Add(new PolicyBasedAuthRequirement()));
            });

            services.AddSingleton<IAuthorizationHandler, PolicyBasedAuthHandler>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(option =>
                {
                    option.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Configuration.GetConnectionString("Jwt:Issuer"),
                        ValidAudience = Configuration.GetConnectionString("Jwt:Audience"),
                        IssuerSigningKey = UserController.SigningCreds.Key
                    };
                });

            services.AddControllers();

            services.AddSignalR()
                .AddAzureSignalR(options =>
            {
                options.ConnectionString = "Endpoint=https://paipaisignalr.service.signalr.net/;AccessKey=xvCB88J0XjYLkhO1oQ6yO9j5nGnSXWb/kysDihoDB4I=;Version=1.0;";
                options.ClaimsProvider = context => new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, context.Request.Query["username"])
                };
            });

            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app)
        {
            // This middleware serves generated Swagger document as a JSON endpoint
            app.UseSwagger();

            // This middleware serves the Swagger documentation UI
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SignalR API V1");
            });

            // Rest of the code
            app.UseAuthentication();
            app.UseRouting();
            app.UseFileServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatJwtSampleHub>("/chatjwt");
                //endpoints.MapHub<ChatCookieSampleHub>("/chatcookie");
            });
        }
    }
}
