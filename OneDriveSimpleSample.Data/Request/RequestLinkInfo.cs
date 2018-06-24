using Newtonsoft.Json;

namespace OneDriveSimpleSample.Request
{
    public class RequestLinkInfo
    {
        [JsonProperty("type")]
        public string Type
        {
            get;
            set;
        }
    }
}