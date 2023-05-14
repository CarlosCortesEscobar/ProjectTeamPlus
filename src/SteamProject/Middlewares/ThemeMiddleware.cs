﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SteamProject.DAL.Abstract;
using SteamProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace SteamProject.Middlewares
{
    public class ThemeMiddleware
    {
        private readonly RequestDelegate _next;

        public ThemeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IServiceScopeFactory scopeFactory)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                var userId = userManager.GetUserId(context.User);
                string theme;
                if (userId is null)
                {
                    theme = "light"; // Set default theme if user not found
                }
                else
                {
                    //try catch here to catch cases where you down db but still have userId cached
                    try
                    {
                        User user = userRepository.GetUser(userId);
                        theme = user.Theme ?? "light"; // Default to light theme if not set
                    }
                    catch (ArgumentNullException exception)
                    {
                        Console.WriteLine(exception);
                        theme = "light"; // Default to light theme if not set
                    }
                   
                }
                context.Items["Theme"] = theme;
            }

            await _next(context);
        }
    }
}