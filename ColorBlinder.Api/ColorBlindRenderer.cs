using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using ColorBlinder.Api.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace ColorBlinder.Api
{
  public class ColorBlindRenderer
  {
    private readonly ChromeDriver _chromeDriver;
    private const string ColorBlindExecuteScript = "executeColorBlind('{0}');";
    private const string DestroyTimers = "for(var highestTimeoutId=setTimeout(';'),i=0;i<highestTimeoutId;i++)clearTimeout(i);";

    private readonly string _colorBlindScript;

    public ColorBlindRenderer(ChromeDriver chromeDriver)
    {
      _chromeDriver = chromeDriver;
      using (var sr = new StreamReader(VirtualPathProvider.OpenFile("/Scripts/ColorBlind.js")))
      {
        _colorBlindScript = sr.ReadToEnd();
      }
      DestroyPageTimers();
    }

    public Bitmap ColorBlindInizePage(ColorBlindTypes colorBlindType)
    {
      StringBuilder assembledScript = new StringBuilder();
      assembledScript.Append(_colorBlindScript);
      assembledScript.AppendLine(string.Format(ColorBlindExecuteScript, colorBlindType));

      IJavaScriptExecutor js = _chromeDriver;
      js.ExecuteScript(assembledScript.ToString());

      return Screenshot();
    }

    private Bitmap Screenshot()
    {
      long totalwidth1 = (long) ((IJavaScriptExecutor) _chromeDriver).ExecuteScript("return document.body.offsetWidth");
        //documentElement.scrollWidth”);
      long totalHeight1 =
        (long) ((IJavaScriptExecutor) _chromeDriver).ExecuteScript("return document.body.parentNode.scrollHeight");

      int totalWidth = (int) totalwidth1;
      int totalHeight = (int) totalHeight1;

      // Get the Size of the Viewport
      long viewportWidth1 =
        (long) ((IJavaScriptExecutor) _chromeDriver).ExecuteScript("return document.body.clientWidth");
        //documentElement.scrollWidth”);
      long viewportHeight1 = (long) ((IJavaScriptExecutor) _chromeDriver).ExecuteScript("return window.innerHeight");
        //documentElement.scrollWidth”);
      int viewportWidth = (int) viewportWidth1;
      int viewportHeight = (int) viewportHeight1;

      // Split the Screen in multiple Rectangles
      List<Rectangle> rectangles = new List<Rectangle>();
      // Loop until the Total Height is reached
      for (int i = 0; i < totalHeight; i += viewportHeight)
      {
        int newHeight = viewportHeight;
        // Fix if the Height of the Element is too big
        if (i + viewportHeight > totalHeight)
        {
          newHeight = totalHeight - i;
        }
        // Loop until the Total Width is reached
        for (int ii = 0; ii < totalWidth; ii += viewportWidth)
        {
          int newWidth = viewportWidth;
          // Fix if the Width of the Element is too big
          if (ii + viewportWidth > totalWidth)
          {
            newWidth = totalWidth - ii;
          }

          // Create and add the Rectangle
          Rectangle currRect = new Rectangle(ii, i, newWidth, newHeight);
          rectangles.Add(currRect);
        }
      }

      // Build the Image
      var stitchedImage = new Bitmap(totalWidth, totalHeight);
      // Get all Screenshots and stitch them together
      Rectangle previous = Rectangle.Empty;
      foreach (var rectangle in rectangles)
      {
        // Calculate the Scrolling (if needed)
        if (previous != Rectangle.Empty)
        {
          int xDiff = rectangle.Right - previous.Right;
          int yDiff = rectangle.Bottom - previous.Bottom;
          // Scroll
          ((IJavaScriptExecutor) _chromeDriver).ExecuteScript(String.Format("window.scrollBy({0}, {1})", xDiff, yDiff));
          System.Threading.Thread.Sleep(200);
        }

        // Take Screenshot
        var screenshot = ((ITakesScreenshot) _chromeDriver).GetScreenshot();

        // Build an Image out of the Screenshot
        Image screenshotImage;
        using (MemoryStream memStream = new MemoryStream(screenshot.AsByteArray))
        {
          screenshotImage = Image.FromStream(memStream);
        }

        // Calculate the Source Rectangle
        Rectangle sourceRectangle = new Rectangle(viewportWidth - rectangle.Width, viewportHeight - rectangle.Height,
          rectangle.Width, rectangle.Height);

        // Copy the Image
        using (Graphics g = Graphics.FromImage(stitchedImage))
        {
          g.DrawImage(screenshotImage, rectangle, sourceRectangle, GraphicsUnit.Pixel);
        }

        // Set the Previous Rectangle
        previous = rectangle;
      }

      //Scroll back to the top
      ((IJavaScriptExecutor)_chromeDriver).ExecuteScript(String.Format("window.scrollTo({0}, {1})", 0, 0));

      return stitchedImage;
    }

    public void DestroyPageTimers()
    {
      IJavaScriptExecutor js = _chromeDriver;
      js.ExecuteScript(DestroyTimers);
    }
  }
}