namespace Blogosphere.API.Middlewares;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class JwtValidationMiddleware
{
   private readonly RequestDelegate _next;
   private readonly IConfiguration _configuration;

   public JwtValidationMiddleware(RequestDelegate next, IConfiguration configuration)
   {
      _next = next;
      _configuration = configuration;
   }

   public async Task Invoke(HttpContext context)
   {
      var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

      if (token != null)
      {
         try
         {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtKey"] ?? "");
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
               ValidateIssuerSigningKey = true,
               IssuerSigningKey = new SymmetricSecurityKey(key),
               ValidateIssuer = true,
               ValidIssuer = _configuration["JwtIssuer"],
               ValidateAudience = true,
               ValidAudience = _configuration["JwtAudience"],
               ValidateLifetime = true,
               ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

            // Add the validated user id to the context items
            context.Items["UserId"] = userId;
         }
         catch
         {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
            return;
         }
      }

      await _next(context);
   }
}

// Extension method used to add the middleware to the HTTP request pipeline.
public static class JwtValidationMiddlewareExtensions
{
   public static IApplicationBuilder UseJwtValidation(this IApplicationBuilder builder)
   {
      return builder.UseMiddleware<JwtValidationMiddleware>();
   }
}

// Attribute to apply the middleware selectively
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireJwtValidationAttribute : Attribute, IAsyncActionFilter
{
   public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
   {
      if (context.HttpContext.Items["UserId"] == null)
      {
         context.Result = new UnauthorizedResult();
         return;
      }

      await next();
   }
}