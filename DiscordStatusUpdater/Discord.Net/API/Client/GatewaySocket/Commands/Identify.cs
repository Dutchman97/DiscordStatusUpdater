﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord.API.Client.GatewaySocket
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class IdentifyCommand : IWebSocketMessage
    {
        int IWebSocketMessage.OpCode => (int)OpCodes.Identify;
        object IWebSocketMessage.Payload => this;
        bool IWebSocketMessage.IsPrivate => false;

        [JsonProperty("v")]
        public int Version { get; set; }
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("properties")]
        public Dictionary<string, string> Properties { get; set; }
        [JsonProperty("large_threshold", NullValueHandling = NullValueHandling.Ignore)]
        public int? LargeThreshold { get; set; }
        [JsonProperty("compress")]
        public bool UseCompression { get; set; }
    }
}
