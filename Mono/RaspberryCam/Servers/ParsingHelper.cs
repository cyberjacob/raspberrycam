using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace RaspberryCam.Servers
{
    public static class ParsingHelper
    {
        private static readonly Regex HttpMethodQueryRegex = 
            new Regex(@"(?<verb>GET|POST|PUT|DELETE)\s*(?<query>.*)\s* HTTP/(?<version>\d[.]\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // I do that instead of HttpUtility.ParseQueryString, because I don't want System.Web as depency
        public static NameValueCollection ParseUrlParameters(this string rawQuery)
        {
            var collection = new NameValueCollection();

            if (string.IsNullOrWhiteSpace(rawQuery))
                return collection;

            var query = rawQuery.StartsWith("?") ? rawQuery.Substring(1) : rawQuery;

            foreach (var item in query.Split('&'))
            {
                var parts = item.Split('=');
                collection.Add(parts[0], parts[1]);
            }

            return collection;
        }

        public static HttpMethodQuery ParseHttpMethodQuery(this string rawRequest)
        {
            var match = HttpMethodQueryRegex.Match(rawRequest);
            if (!match.Success)
                return null;

            var verb = match.Groups["verb"].Value;
            var query = match.Groups["query"].Value;
            var version = match.Groups["version"].Value;

            return new HttpMethodQuery
                {
                    Verb = verb,
                    PathAndQuery = query,
                    Version = version
                };
        }
    }
}