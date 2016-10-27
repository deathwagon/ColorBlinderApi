using System;
using System.Drawing;
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
        driver.Manage().Window.Size = new Size(1920, 1080);

        Thread.Sleep(4000);
        
        var basePath = HttpContext.Current.Server.MapPath($"~/Captures/{requestGuid}");
        Directory.CreateDirectory(basePath);

        var colorBlindRenderer = new ColorBlindRenderer(driver);
        foreach(ColorBlindTypes colorBlindType in Enum.GetValues(typeof(ColorBlindTypes)))
        {
          var afterScreenCapture = colorBlindRenderer.ColorBlindInizePage(colorBlindType);
          afterScreenCapture.Save($"{basePath}/{colorBlindType}.png", ImageFormat.Png);
        }
      }

      return new JObject(
        new JProperty("original", PathBuilder(requestGuid, $"{ColorBlindTypes.Normal}.png")),
        new JProperty($"{ColorBlindTypes.Achromatomaly}", PathBuilder(requestGuid, $"{ColorBlindTypes.Achromatomaly}.png")),
        new JProperty($"{ColorBlindTypes.Achromatopsia}", PathBuilder(requestGuid, $"{ColorBlindTypes.Achromatopsia}.png")),
        new JProperty($"{ColorBlindTypes.Deuteranomaly}", PathBuilder(requestGuid, $"{ColorBlindTypes.Deuteranomaly}.png")),
        new JProperty($"{ColorBlindTypes.Deuteranopia}", PathBuilder(requestGuid, $"{ColorBlindTypes.Deuteranopia}.png")),
        new JProperty($"{ColorBlindTypes.Protanomaly}", PathBuilder(requestGuid, $"{ColorBlindTypes.Protanomaly}.png")),
        new JProperty($"{ColorBlindTypes.Protanopia}", PathBuilder(requestGuid, $"{ColorBlindTypes.Protanopia}.png")),
        new JProperty($"{ColorBlindTypes.Tritanomaly}", PathBuilder(requestGuid, $"{ColorBlindTypes.Tritanomaly}.png")),
        new JProperty($"{ColorBlindTypes.Tritanopia}", PathBuilder(requestGuid, $"{ColorBlindTypes.Tritanopia}.png"))
      );
    }

    private string PathBuilder(string guid, string fileName)
    {
      UriBuilder uriFilterBuilder = new UriBuilder(Request.RequestUri.Scheme, Request.RequestUri.Host);
      uriFilterBuilder.Port = Request.RequestUri.Port;
      uriFilterBuilder.Path = $"captures/{guid}/{fileName}";

      return uriFilterBuilder.ToString();
    }
  }
}
