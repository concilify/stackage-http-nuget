namespace Stackage.Http
{
   public interface IHttpServiceConfiguration
   {
      string Url { get; }

      int TimeoutMs { get; }
   }
}
