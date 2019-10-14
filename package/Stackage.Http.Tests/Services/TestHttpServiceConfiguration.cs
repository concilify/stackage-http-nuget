namespace Stackage.Http.Tests.Services
{
   public class TestHttpServiceConfiguration : IHttpServiceConfiguration
   {
      public string Url { get; set; }

      public int TimeoutMs { get; set; }
   }
}
