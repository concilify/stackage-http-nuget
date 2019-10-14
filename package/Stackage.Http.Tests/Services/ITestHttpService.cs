using System.Net;
using System.Threading.Tasks;

namespace Stackage.Http.Tests.Services
{
   public interface ITestHttpService
   {
      Task<(HttpStatusCode statusCode, string content)> Passthrough();

      Task<(HttpStatusCode statusCode, string content)> Passthrough(string baseAddress);
   }
}
