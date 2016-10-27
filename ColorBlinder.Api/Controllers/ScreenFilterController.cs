using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Http;
using ColorBlinder.Api.Models;
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

        var colorBlindRenderer = new ColorBlindRenderer(driver);
        colorBlindRenderer.ColorBlindInizePage(ColorBlindTypes.Tritanomaly);

        var afterScreenCapture = ((ITakesScreenshot)driver).GetScreenshot();
        afterScreenCapture.SaveAsFile($"{basePath}/filter.png", ImageFormat.Png);
      }

      UriBuilder uriOriginalBuilder = new UriBuilder(Request.RequestUri.Scheme, Request.RequestUri.Host);
      uriOriginalBuilder.Port = Request.RequestUri.Port;
      uriOriginalBuilder.Path = $"captures/{requestGuid}/original.png";

      UriBuilder uriFilterBuilder = new UriBuilder(Request.RequestUri.Scheme, Request.RequestUri.Host);
      uriFilterBuilder.Port = Request.RequestUri.Port;
      uriFilterBuilder.Path = $"captures/{requestGuid}/filter.png";

      var originalUrl = uriOriginalBuilder.ToString();
      var filterUrl = uriFilterBuilder.ToString();

      return new JObject(
        new JProperty("original", originalUrl),
        new JProperty("filterOne", filterUrl)
      );
    }
  }
}
