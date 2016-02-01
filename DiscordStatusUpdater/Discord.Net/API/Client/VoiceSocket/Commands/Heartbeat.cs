﻿namespace Discord.API.Client.VoiceSocket
{
    public sealed class HeartbeatCommand : IWebSocketMessage
    {
        int IWebSocketMessage.OpCode => (int)OpCodes.Heartbeat;
        object IWebSocketMessage.Payload => EpochTime.GetMilliseconds();
        bool IWebSocketMessage.IsPrivate => false;
    }
}
