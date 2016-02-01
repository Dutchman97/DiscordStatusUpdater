﻿using Newtonsoft.Json;

namespace Discord.API.Client.Rest
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class KickMemberRequest : IRestRequest
    {
        string IRestRequest.Method => "DELETE";
        string IRestRequest.Endpoint => $"guilds/{GuildId}/members/{UserId}";
        object IRestRequest.Payload => null;
        bool IRestRequest.IsPrivate => false;

        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }

        public KickMemberRequest(ulong guildId, ulong userId)
        {
            GuildId = guildId;
            UserId = userId;
        }
    }
}
