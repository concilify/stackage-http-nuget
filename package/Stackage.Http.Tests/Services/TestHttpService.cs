using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Stackage.Http.Tests.Services
{
   public class TestHttpService : ITestHttpService
   {
      private readonly HttpClient _httpClient;

      public TestHttpService(HttpClient httpClient)
      {
         _httpClient = httpClient;
      }

      public async Task<(HttpStatusCode statusCode, string content)> Passthrough()
      {
         var response = await _httpClient.GetAsync("passthrough");

         return (response.StatusCode, await response.Content.ReadAsStringAsync());
      }

      public async Task<(HttpStatusCode statusCode, string content)> Passthrough(string baseAddress)
      {
         var response = await _httpClient.GetAsync($"{baseAddress}/passthrough");

         return (response.StatusCode, await response.Content.ReadAsStringAsync());
      }
   }
}
