using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.IO;
using System;

namespace DotnetServer.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Get the US Central Time zone info
            TimeZoneInfo centralZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            DateTime centralTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, centralZone);

            // Log the request method, path, and query string
            Console.WriteLine(" ");
            Console.WriteLine("########## Start DotNet Request ##########");
            Console.WriteLine($"{centralTime}");
            Console.WriteLine($"HTTP {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
            Console.WriteLine("########### End DotNet Request ###########");
            Console.WriteLine(" ");

            // Log the request body if it's not empty
            if (context.Request.ContentLength > 0)
            {
                context.Request.EnableBuffering(); // Allows the request body to be read multiple times
                var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();

                Console.WriteLine($"Request Body: {requestBody}");
                context.Request.Body.Position = 0; // Reset the stream position for further processing
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }
}
