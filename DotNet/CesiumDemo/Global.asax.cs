using System;
using System.Web;
using AGI.Examples;
using AGI.Foundation.Infrastructure;

namespace CesiumDemo
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // Get leap second data, and use it in the current calculation context.
            LeapSecondsFacetHelper.GetLeapSeconds().UseInCurrentContext();
            CalculationContext.DefaultForNewContexts = CalculationContext.Instance;
        }

        protected void Session_Start(object sender, EventArgs e)
        {
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
        }

        protected void Session_End(object sender, EventArgs e)
        {
        }

        protected void Application_End(object sender, EventArgs e)
        {
        }
    }
}
