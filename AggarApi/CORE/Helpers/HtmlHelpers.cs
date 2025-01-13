using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Helpers
{
    public static class HtmlHelpers
    {
        public static string GenerateAccountActivationHtmlBody(string code)
        {
            return $@"
            <html>
            <body>
                <h1>Welcome to Aggar</h1>
                <h2>Activate your account</h2>
                <p>Activate your account by entering the code below to complete the activation process:</p>
                <h3>{System.Web.HttpUtility.HtmlEncode(code)}</h3>
                <p>The code will expire after 5 minutes.</p>
            </body>
            </html>";
        }
    }
}
