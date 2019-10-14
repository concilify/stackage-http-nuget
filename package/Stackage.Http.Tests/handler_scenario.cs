using System;
using FakeItEasy;
using Hornbill;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Core.Abstractions;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Extensions;

namespace Stackage.Http.Tests
{
   public abstract class handler_scenario
   {
      protected IGuidGenerator GuidGenerator { get; private set; }

      protected StubMetricSink MetricSink { get; private set; }

      protected ServiceProvider ServiceProvider { get; private set; }

      protected string StubBaseAddress { get; private set; }

      private FakeService _stubHttpService;

      protected void setup_handler_scenario(Action<FakeService> configureStubHttpService = null, Action<string, ServiceCollection> configureServices = null)
      {
         _stubHttpService = new FakeService();

         configureStubHttpService?.Invoke(_stubHttpService);

         GuidGenerator = A.Fake<IGuidGenerator>();
         MetricSink = new StubMetricSink();

         var services = new ServiceCollection();

         services.AddDefaultServices();

         services.AddSingleton(GuidGenerator);
         services.AddSingleton<IMetricSink>(MetricSink);

         StubBaseAddress = _stubHttpService.Start();

         configureServices?.Invoke(StubBaseAddress, services);

         ServiceProvider = services.BuildServiceProvider();
      }

      protected void teardown_handler_scenario()
      {
         _stubHttpService.Dispose();
         ServiceProvider.Dispose();
      }
   }
}
