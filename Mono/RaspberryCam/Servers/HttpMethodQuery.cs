namespace RaspberryCam.Servers
{
    public class HttpMethodQuery
    {
        public string Verb;
        public string PathAndQuery;
        public string Version;

        public string Path
        {
            get
            {
                var indexOf = PathAndQuery.IndexOf('?');
                if (indexOf <= 0)
                    return PathAndQuery;

                return PathAndQuery.Substring(0, indexOf);
            }
        }

        public string Query
        {
            get
            {
                var indexOf = PathAndQuery.IndexOf('?');
                if (indexOf <= 0)
                    return string.Empty;

                return PathAndQuery.Substring(indexOf+1);
            }
        }
    }
}