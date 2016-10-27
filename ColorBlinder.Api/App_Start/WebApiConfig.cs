using System.Web.Http;

namespace ColorBlinder.Api
{
  public static class WebApiConfig
  {
    public static void Register(HttpConfiguration config)
    {
      config.MapHttpAttributeRoutes();
    }
  }
}
