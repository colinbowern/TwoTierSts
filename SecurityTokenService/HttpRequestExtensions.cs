using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SecurityTokenService
{
    public static class HttpRequestExtensions
    {
        public static string ToApplicationUri(this HttpRequestBase request)
        {
            var result = new StringBuilder();
            result.Append(request.Url.Scheme);
            result.Append("://");
            result.Append(request.Headers["Host"] ?? request.Url.Authority);
            result.Append(request.ApplicationPath);
            if (!request.ApplicationPath.EndsWith("/"))
            {
                result.Append("/");
            }
            return result.ToString();
        }

        public static string ToApplicationUri(this HttpRequest request)
        {
            var result = new StringBuilder();
            result.Append(request.Url.Scheme);
            result.Append("://");
            result.Append(request.Headers["Host"] ?? request.Url.Authority);
            result.Append(request.ApplicationPath);
            if (!request.ApplicationPath.EndsWith("/"))
            {
                result.Append("/");
            }
            return result.ToString();
        }
    }
}
