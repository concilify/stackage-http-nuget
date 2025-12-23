using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
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

      protected async Task setup_handler_scenario_async(Action<FakeService> configureStubHttpService = null, Action<string, ServiceCollection> configureServices = null)
      {
         _stubHttpService = new FakeService();

         configureStubHttpService?.Invoke(_stubHttpService);

         GuidGenerator = A.Fake<IGuidGenerator>();
         MetricSink = new StubMetricSink();

         var services = new ServiceCollection();

         services.AddDefaultServices(A.Fake<IConfiguration>());

         services.AddSingleton(GuidGenerator);
         services.AddSingleton<IMetricSink>(MetricSink);

         StubBaseAddress = await _stubHttpService.StartAsync();

         configureServices?.Invoke(StubBaseAddress, services);

         ServiceProvider = services.BuildServiceProvider();
      }

      protected async Task teardown_handler_scenario_async()
      {
         await _stubHttpService.DisposeAsync();
         await ServiceProvider.DisposeAsync();
      }
   }
}
