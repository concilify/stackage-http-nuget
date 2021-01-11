using System;
using System.Net.Http;

namespace Stackage.Http
{
   public class HttpServiceException : Exception
   {
      public HttpServiceException(Exception innerException, string serviceName, string url)
         : base("Failed to invoke service", innerException)
      {
         ServiceName = serviceName;
         Url = url;
      }

      public string ServiceName { get; }

      public string Url { get; }
   }
}
