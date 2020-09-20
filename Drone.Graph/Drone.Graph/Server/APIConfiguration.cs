using System.IO;
using System.Text;
using System.Threading.Tasks;
using Drone.Graph.Server.Requests;
using Microsoft.AspNetCore.Http;
using Rigger.Attributes;
using Rigger.ManagedTypes;

namespace Drone.Graph.Server
{

        [Bootstrap] 
        class DefaultConfiguration
        {
            [Autowire]
            public DefaultConfiguration(IContainer container)
            {
                Task DefaultRequestDelegate(HttpContext context)
                {
                    var requestHandler = container.Get<IIncomingRequestHandler>();

                    context.Response.ContentType = "application/json";

                    // TODO remove this, make CORS default based on config
                    context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                    context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type";

                    if (context.Request.Method == HttpMethods.Options)
                    {
                        context.Response.StatusCode = 204;

                        context.Response.Headers["Allow"] = "OPTIONS, POST";

                        return Task.FromResult("Allow");
                    }

                    context.Response.ContentType = "application/json";

                    var path = context.Request.Path;

                    var bodyString = ReadBodyFromBeginning(context.Request.Body);

                    var resultString = requestHandler.Handle(new RequestContext
                    {
                        Path = path.Value,
                        BodyAsString = bodyString,
                        ContentType = context.Request.ContentType,
                        Headers = context.Request.Headers
                    });

                    var responseString = Encoding.UTF8.GetBytes(resultString);

                    context.Response.ContentLength = responseString.Length;
                    context.Response.Body.Write(responseString);

                    return Task.FromResult(resultString);
                }

                container.Register<RequestDelegate>((RequestDelegate) DefaultRequestDelegate);
            }

            private string ReadBodyFromBeginning(Stream stream)
            {
                var body = new StreamReader(stream);

                return body.ReadToEnd();
            }
        }
}