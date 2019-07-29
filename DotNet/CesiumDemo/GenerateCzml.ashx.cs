using System.Web;

namespace CesiumDemo
{
    /// <summary>
    /// This HTTP handler acts as a basic web service. It expects
    /// a NORAD satellite identifier to be passed as a query parameter, and
    /// writes CZML for the demonstration directly to the response.
    /// </summary>
    /// <remarks>
    /// Note that the capabilities being demonstrated don't depend on ASP.NET,
    /// and can be used with any web application framework.
    /// </remarks>
    public class GenerateCzml : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            string satelliteIdentifier = context.Request.Params["id"];

            // construct the objects for the demonstration.
            var demo = new CesiumDemo(satelliteIdentifier);

            // Write the CZML document directly to the response.
            context.Response.ContentType = "application/json";
            demo.WriteDocument(context.Response.Output);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
