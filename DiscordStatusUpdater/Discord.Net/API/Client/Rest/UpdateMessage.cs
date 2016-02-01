﻿using Discord.API.Converters;
using Newtonsoft.Json;

namespace Discord.API.Client.Rest
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class UpdateMessageRequest : IRestRequest<Message>
    {
        string IRestRequest.Method => "PATCH";
        string IRestRequest.Endpoint => $"channels/{ChannelId}/messages/{MessageId}";
        object IRestRequest.Payload => this;
        bool IRestRequest.IsPrivate => false;

        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; } = "";

        public UpdateMessageRequest(ulong channelId, ulong messageId)
        {
            ChannelId = channelId;
            MessageId = messageId;
        }
    }
}
