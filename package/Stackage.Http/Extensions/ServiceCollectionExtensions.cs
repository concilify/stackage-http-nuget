using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Abstractions.Polly;

namespace Stackage.Http.Extensions
{
   public static class ServiceCollectionExtensions
   {
      public static IServiceCollection RemoveDefaultHttpClientLogging(this IServiceCollection services)
      {
         services.RemoveAll<IHttpMessageHandlerBuilderFilter>();

         return services;
      }

      public static IServiceCollection AddHttpService<TService, TImplementation, TConfiguration>(this IServiceCollection services, IConfiguration configuration)
         where TService : class
         where TImplementation : class, TService
         where TConfiguration : class, IHttpServiceConfiguration, new()
      {
         const string removeSuffix = "Configuration";

         var httpServiceName = typeof(TConfiguration).Name;

         if (httpServiceName.EndsWith(removeSuffix, StringComparison.InvariantCultureIgnoreCase))
         {
            httpServiceName = httpServiceName.Substring(0, httpServiceName.Length - removeSuffix.Length);
         }

         var httpServiceConfiguration = new TConfiguration();
         configuration.Bind(httpServiceName, httpServiceConfiguration);

         services.AddSingleton(httpServiceConfiguration);

         services.AddHttpClient<TService, TImplementation>()
            .ConfigureHttpClient((_, httpClient) => ConfigureHttpClient(httpClient, httpServiceConfiguration))
            .AddHttpMessageHandler(() => CreateExceptionHandler(httpServiceName))
            .AddHttpMessageHandler(sp => CreateTimingHandler(sp, httpServiceName));

         return services;
      }

      private static void ConfigureHttpClient(HttpClient httpClient, IHttpServiceConfiguration httpServiceConfiguration)
      {
         if (!string.IsNullOrEmpty(httpServiceConfiguration.Url))
         {
            httpClient.BaseAddress = new Uri(httpServiceConfiguration.Url);
         }
      }

      private static ExceptionHandler CreateExceptionHandler(string httpServiceName)
      {
         return new ExceptionHandler(httpServiceName);
      }

      private static TimingHandler CreateTimingHandler(IServiceProvider sp, string httpServiceName)
      {
         var policyFactory = sp.GetRequiredService<IPolicyFactory>();
         var metricSink = sp.GetRequiredService<IMetricSink>();

         return new TimingHandler(policyFactory, metricSink, httpServiceName);
      }
   }
}
