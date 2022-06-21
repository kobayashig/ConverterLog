namespace CandidateTesting.GabrielKobayashiBarboza.ConvertLog.Models
{
    public class LogModelNow
    {
        public LogModelNow(string httpMethod, int statusCode, string uriPath, int timeTaken, int responseSize, string cacheStatus)
        {
            HttpMethod = httpMethod;
            StatusCode = statusCode;
            UriPath = uriPath;
            TimeTaken = timeTaken;
            ResponseSize = responseSize;
            CacheStatus = cacheStatus;
        }

        public string Provider { get; private set; } = "MINHA CDN";
        public string HttpMethod { get; private set; }
        public int StatusCode { get; private set; }
        public string UriPath { get; private set; }
        public int TimeTaken { get; private set; }
        public int ResponseSize { get; private set; }
        public string CacheStatus { get; private set; }
    }
}
