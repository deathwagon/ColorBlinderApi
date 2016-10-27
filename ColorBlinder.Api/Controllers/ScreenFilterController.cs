using System.Drawing.Imaging;
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
      var driver = new ChromeDriver();
      driver.Navigate().GoToUrl(url);

      var originalScreenCapture = ((ITakesScreenshot) driver).GetScreenshot();
      originalScreenCapture.SaveAsFile("C:\\original.jpg", ImageFormat.Jpeg);

      return new JObject(
        new JProperty("original", "http://localhost/original.jpg")  
      );
    }
  }
}
