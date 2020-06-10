using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blazm.Bluetooth
{
    [Serializable]
    public class RequestDeviceQuery
    {
        [JsonProperty(propertyName:"filters")]
        public List<Filter> Filters { get; set; } = new List<Filter>();
        [JsonProperty(propertyName: "acceptAllDevices")]
        public bool? AcceptAllDevices { get; set; } = null;
        [JsonProperty(propertyName: "optionalServices")]
        public List<string> OptionalServices { get; set; } = new List<string>();
        public bool ShouldSerializeFilters()
        {
            return Filters.Count > 0;
        }
        public bool ShouldSerializeOptionalServices()
        {
            return OptionalServices.Count > 0;
        }
    }

    [Serializable]
    public class Filter
    {
        [JsonProperty(propertyName: "services")]
        public List<object> Services { get; set; } = new List<object>();
        [JsonProperty(propertyName: "name")]
        public string Name  { get; set; }
        public bool ShouldSerializeServices()
        {
            return Services.Count > 0;
        }
    }
}
