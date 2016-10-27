using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace ColorBlinder.Api.Controllers
{
  [Route("v1/ScreenFilter")]
  public class ScreenFilterController : ApiController
  {
    [HttpGet]
    public JObject GetScreenCaptures(string url)
    {
      var requestGuid = Guid.NewGuid().ToString();

      using (var driver = new ChromeDriver())
      {
        driver.Navigate().GoToUrl(new Uri(url, UriKind.Absolute));

        Thread.Sleep(4000);
        
        var basePath = HttpContext.Current.Server.MapPath($"~/Captures/{requestGuid}");
        Directory.CreateDirectory(basePath);

        var originalScreenCapture = ((ITakesScreenshot)driver).GetScreenshot();
        originalScreenCapture.SaveAsFile($"{basePath}/original.png", ImageFormat.Png);
      }

      UriBuilder uriBuilder = new UriBuilder(Request.RequestUri.Scheme, Request.RequestUri.Host);
      uriBuilder.Port = Request.RequestUri.Port;
      uriBuilder.Path = $"captures/{requestGuid}/original.png";

      var originalUrl = uriBuilder.ToString();

      return new JObject(
        new JProperty("original", originalUrl)  
      );
    }
  }
}
