using Newtonsoft.Json;

namespace UppsalaApi.Models
{
    public abstract class Resource : Link
    {
        [JsonIgnore]
        public Link Self { get; set; }   
    }
}