using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KawaBot.Server.Controllers
{
    public class HealthController
    {
        public static void Register(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapGet("health", ctx => ctx.Response.WriteAsync("OK"));
        }
    }
}
