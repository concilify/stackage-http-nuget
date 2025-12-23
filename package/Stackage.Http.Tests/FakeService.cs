namespace Stackage.Http.Tests;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

public class FakeService : IAsyncDisposable
{
   private readonly WebApplication _app;

   public FakeService()
   {
      var builder = WebApplication.CreateBuilder();
      builder.WebHost.UseKestrel();
      builder.WebHost.UseUrls("http://127.0.0.1:0");

      _app = builder.Build();
   }

   public void Get(string path, int statusCode, string content)
   {
      _app.MapGet(path, () => Results.Content(content, statusCode: statusCode));
   }

   public async Task<string> StartAsync()
   {
      await _app.StartAsync();

      return _app.Urls.First();
   }

   public async ValueTask DisposeAsync()
   {
      await _app.StopAsync();
      await _app.DisposeAsync();
   }
}
