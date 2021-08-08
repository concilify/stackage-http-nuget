using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hornbill;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Http.Extensions;
using Stackage.Http.Tests.Services;

namespace Stackage.Http.Tests.MetricsScenarios
{
   public class get_request_from_service_with_no_base_address : handler_scenario
   {
      private HttpStatusCode _statusCode;
      private string _content;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         setup_handler_scenario(
            stubHttpService =>
            {
               stubHttpService.AddResponse("/passthrough", Method.GET, Response.WithBody(200, "bar"));
            },
            (stubBaseAddress, services) =>
            {
               var configuration = new ConfigurationBuilder()
                  .AddInMemoryCollection(new Dictionary<string, string>
                  {
                     {"TESTHTTPSERVICE:TIMEOUTMS", "500"}
                  }).Build();

               services.AddHttpService<ITestHttpService, TestHttpService, TestHttpServiceConfiguration>(configuration);
            });

         var api = ServiceProvider.GetRequiredService<ITestHttpService>();

         (_statusCode, _content) = await api.Passthrough(StubBaseAddress);
      }

      [OneTimeTearDown]
      public void teardown_scenario()
      {
         teardown_handler_scenario();
      }

      [Test]
      public void should_return_status_code_200()
      {
         _statusCode.ShouldBe(HttpStatusCode.OK);
      }

      [Test]
      public void should_return_content()
      {
         _content.ShouldBe("bar");
      }

      [Test]
      public void should_write_two_metrics()
      {
         Assert.That(MetricSink.Metrics.Count, Is.EqualTo(2));
      }

      [Test]
      public void should_write_start_metric()
      {
         var metric = (Counter) MetricSink.Metrics.First();

         Assert.That(metric.Name, Is.EqualTo("http_client_start"));
         Assert.That(metric.Dimensions["name"], Is.EqualTo("TestHttpService"));
         Assert.That(metric.Dimensions["method"], Is.EqualTo("GET"));
         Assert.That(metric.Dimensions["path"], Is.EqualTo("/passthrough"));
      }

      [Test]
      public void should_write_end_metric()
      {
         var metric = (Gauge) MetricSink.Metrics.Last();

         Assert.That(metric.Name, Is.EqualTo("http_client_end"));
         Assert.That(metric.Dimensions["name"], Is.EqualTo("TestHttpService"));
         Assert.That(metric.Dimensions["method"], Is.EqualTo("GET"));
         Assert.That(metric.Dimensions["path"], Is.EqualTo("/passthrough"));
         Assert.That(metric.Dimensions["statusCode"], Is.EqualTo(200));
      }
   }
}
