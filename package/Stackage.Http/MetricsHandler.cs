using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Stackage.Core.Abstractions.Polly;

namespace Stackage.Http
{
   public class MetricsHandler : DelegatingHandler
   {
      private readonly IPolicyFactory _policyFactory;
      private readonly string _httpServiceName;

      public MetricsHandler(
         IPolicyFactory policyFactory,
         string httpServiceName)
      {
         _policyFactory = policyFactory ?? throw new ArgumentNullException(nameof(policyFactory));
         _httpServiceName = httpServiceName ?? throw new ArgumentNullException(nameof(httpServiceName));
      }

      protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
      {
         Task OnSuccessAsync(Context policyContext, HttpResponseMessage result)
         {
            policyContext.Add("statusCode", (int) result.StatusCode);
            return Task.CompletedTask;
         }

         Task OnExceptionAsync(Context policyContext, Exception exception)
         {
            policyContext.Add("exception", exception.GetType().Name);
            return Task.CompletedTask;
         }

         var metricsPolicy = _policyFactory.CreateAsyncMetricsPolicy<HttpResponseMessage>(
            "http_client",
            onSuccessAsync: OnSuccessAsync,
            onExceptionAsync: OnExceptionAsync);

         var dimensions = new Dictionary<string, object>
         {
            {"name", _httpServiceName},
            {"method", request.Method.Method},
            {"path", request.RequestUri!.LocalPath}
         };

         var response = await metricsPolicy
            .ExecuteAsync(_ => base.SendAsync(request, cancellationToken), dimensions)
            .ConfigureAwait(false);
         return response;
      }
   }
}
