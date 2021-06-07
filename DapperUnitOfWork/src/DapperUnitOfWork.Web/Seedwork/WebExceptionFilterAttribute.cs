using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;

namespace DapperUnitOfWork.Web.Seedwork
{
    public class WebExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly IHostEnvironment _hostEnvironment;

        public WebExceptionFilterAttribute(IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public override void OnException(ExceptionContext context)
        {
            var code = context.Exception switch
            {
                ValidationException _ => HttpStatusCode.BadRequest,
                // DomainException _ => HttpStatusCode.BadRequest,
                // NotFoundException _ => HttpStatusCode.NotFound,
                // ConflictException _ => HttpStatusCode.Conflict,
                // AuthorizationException _ => HttpStatusCode.Unauthorized,
                _ => HttpStatusCode.InternalServerError
            };

            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)code;
            context.Result = _hostEnvironment.IsDevelopment()
                ? new JsonResult(new { error = context.Exception.Message, stackTrace = context.Exception.StackTrace })
                : new JsonResult(new { error = context.Exception.Message });
        }
    }
}