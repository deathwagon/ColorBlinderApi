using System;
using System.Collections.Generic;
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
    private string _colorBlindScript;

    public ColorBlindRenderer(ChromeDriver chromeDriver)
    {
      _chromeDriver = chromeDriver;
      using (var sr = new StreamReader(VirtualPathProvider.OpenFile("/Scripts/ColorBlind.js")))
      {
        _colorBlindScript = sr.ReadToEnd();
      }
    }

    public Screenshot ColorBlindInizePage(ColorBlindTypes colorBlindType)
    {
      StringBuilder assembledScript = new StringBuilder();
      assembledScript.Append(_colorBlindScript);
      assembledScript.AppendLine(string.Format(ColorBlindExecuteScript, colorBlindType));
 
      IJavaScriptExecutor js = _chromeDriver;
      js.ExecuteScript(assembledScript.ToString());

      return ((ITakesScreenshot)_chromeDriver).GetScreenshot();
    }
  }
}