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
using ColorBlinder.Api.Analysis;

namespace ColorBlinder.Api.Controllers
{
  [Route("v1/ScreenFilter")]
  public class ScreenFilterController : ApiController
  {
    [HttpGet]
    public JObject GetScreenCaptures(string url)
    {
      var requestGuid = Guid.NewGuid().ToString();
      var results = new JObject();

      using (var driver = new ChromeDriver())
      {
        driver.Navigate().GoToUrl(new Uri(url, UriKind.Absolute));
        driver.Manage().Window.Size = new Size(1920, 1080);

        Thread.Sleep(4000);

        var basePath = HttpContext.Current.Server.MapPath($"~/Captures/{requestGuid}");
        Directory.CreateDirectory(basePath);

        var colorBlindRenderer = new ColorBlindRenderer(driver);
        foreach (ColorBlindTypes colorBlindType in Enum.GetValues(typeof(ColorBlindTypes)))
        {
          var afterScreenCapture = colorBlindRenderer.ColorBlindInizePage(colorBlindType);
          afterScreenCapture.Save($"{basePath}/{colorBlindType}.png", ImageFormat.Png);

          results.Add(new JProperty($"{colorBlindType}", ReturnDataBuilder(requestGuid, $"{colorBlindType}.png")));
        }

        var scores = EdgeCorrelate.GetScores(basePath);
        foreach (var score in scores)
        {
          var returnData = (JObject) results[score.Key.ToString()];
          returnData.Add("score", score.Value);
        }
      }

      return results;
    }

    private JObject ReturnDataBuilder(string guid, string fileName)
    {
      UriBuilder uriFilterBuilder = new UriBuilder(Request.RequestUri.Scheme, Request.RequestUri.Host);
      uriFilterBuilder.Port = Request.RequestUri.Port;
      uriFilterBuilder.Path = $"captures/{guid}/{fileName}";

      return new JObject(new JProperty("url", uriFilterBuilder.ToString()));
    }
  }
}
