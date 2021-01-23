using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Stackage.Http
{
   public class ExceptionHandler : DelegatingHandler
   {
      private readonly string _httpServiceName;

      public ExceptionHandler(string httpServiceName)
      {
         _httpServiceName = httpServiceName;
      }

      protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
      {
         try
         {
            var response = await base.SendAsync(request, cancellationToken);
            return response;
         }
         catch (Exception e)
         {
            throw new HttpServiceException(e, _httpServiceName, request.RequestUri!.ToString());
         }
      }
   }
}
