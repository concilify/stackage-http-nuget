using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Http.Extensions;
using Stackage.Http.Tests.Services;

namespace Stackage.Http.Tests.TimingScenarios
{
   public class get_request_to_invalid_address : handler_scenario
   {
      private Exception _thrownException;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         setup_handler_scenario(
            stubHttpService => { },
            (stubBaseAddress, services) =>
            {
               var configuration = new ConfigurationBuilder()
                  .AddInMemoryCollection(new Dictionary<string, string>
                  {
                     {"TESTHTTPSERVICE:URL", "http://does-not-exist"},
                     {"TESTHTTPSERVICE:TIMEOUTMS", "500"}
                  }).Build();

               services.AddHttpService<ITestHttpService, TestHttpService, TestHttpServiceConfiguration>(configuration);
            });

         var api = ServiceProvider.GetRequiredService<ITestHttpService>();

         try
         {
            await api.Passthrough();
         }
         catch (Exception e)
         {
            _thrownException = e;
         }
      }

      [OneTimeTearDown]
      public void teardown_scenario()
      {
         teardown_handler_scenario();
      }

      [Test]
      public void exception_is_bubbled_up_through_service()
      {
         var httpServiceException = _thrownException.ShouldBeOfType<HttpServiceException>();

         httpServiceException.ServiceName.ShouldBe("TestHttpService");
         httpServiceException.Url.ShouldBe("http://does-not-exist/passthrough");
         httpServiceException.InnerException.ShouldBeOfType<HttpRequestException>();
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
         Assert.That(metric.Value, Is.GreaterThanOrEqualTo(0));
         Assert.That(metric.Dimensions["name"], Is.EqualTo("TestHttpService"));
         Assert.That(metric.Dimensions["method"], Is.EqualTo("GET"));
         Assert.That(metric.Dimensions["path"], Is.EqualTo("/passthrough"));
         Assert.That(metric.Dimensions["exception"], Is.EqualTo("HttpRequestException"));
      }
   }
}
