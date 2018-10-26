using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UppsalaApi.Models;

namespace UppsalaApi.Filters
{
    public class JsonExecptionFilter : IExceptionFilter
    {
        private readonly IHostingEnvironment _env;

        public JsonExecptionFilter(IHostingEnvironment env) 
        {
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            var error = new ApiError();

            if (_env.IsDevelopment())
            {
                error.Message = context.Exception.Message;
                error.Detail = context.Exception.StackTrace;
            }
            else
            {
                error.Message = "A server error occurred.";
                error.Detail = context.Exception.Message;

            }

            context.Result = new ObjectResult(error)
            {
                StatusCode = 500
            };

        }
    }
}
