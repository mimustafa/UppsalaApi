using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace UppsalaApi.Infrastructure
{
    public class IonOutputFormatter :TextOutputFormatter
    {
        private readonly JsonOutputFormatter _jsonOutputFormatter;

        public IonOutputFormatter(JsonOutputFormatter jsonOutputFormatter)
        {
            if (jsonOutputFormatter == null) throw new ArgumentNullException(nameof(jsonOutputFormatter));
            _jsonOutputFormatter = jsonOutputFormatter;

            SupportedMediaTypes.Add(new MediaTypeHeaderValue("applicaion/ion+json"));
            SupportedEncodings.Add(Encoding.UTF8);

        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding) 
        => _jsonOutputFormatter.WriteResponseBodyAsync(context, selectedEncoding);


    }
   
}
