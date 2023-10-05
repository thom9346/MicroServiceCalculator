using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;

namespace Monitoring
{

    public static class Monitoring
    {
        public static readonly ActivitySource ActivitySource = new("Calculator", "1.0.0");
        private static TracerProvider _tracerProvider;
        public static ILogger Log => Serilog.Log.Logger;

        static Monitoring()
        {
            Console.WriteLine("Static constructor of Monitoring class invoked.");

            // Configure tracing
            var serviceName = Assembly.GetExecutingAssembly().GetName().Name;
            var version = "1.0.0";

            _tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddZipkinExporter(options =>
                {
                    options.Endpoint = new Uri("http://zipkin:9411/api/v2/spans");
                })
                .SetSampler(new AlwaysOnSampler())
                .AddConsoleExporter()
                .AddSource(ActivitySource.Name)
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: serviceName, serviceVersion: version))
                .Build();

            // Configure logging
            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithSpan()
                .WriteTo.Seq("http://seq:5341")
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}