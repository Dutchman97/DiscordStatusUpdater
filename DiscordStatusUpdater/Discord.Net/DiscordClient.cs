﻿using Discord.API.Client.GatewaySocket;
using Discord.API.Client.Rest;
using Discord.Logging;
using Discord.Net;
using Discord.Net.Rest;
using Discord.Net.WebSockets;
using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Discord
{
    /// <summary> Provides a connection to the DiscordApp service. </summary>
    public partial class DiscordClient
    {
        private readonly AsyncLock _connectionLock;
        private readonly ManualResetEvent _disconnectedEvent;
        private readonly ManualResetEventSlim _connectedEvent;
        private readonly TaskManager _taskManager;
        private readonly ConcurrentDictionary<ulong, Server> _servers;
        private readonly ConcurrentDictionary<ulong, Channel> _channels;
        private readonly ConcurrentDictionary<ulong, Channel> _privateChannels; //Key = RecipientId
        private Dictionary<string, Region> _regions;
        private CancellationTokenSource _cancelTokenSource;

        internal Logger Logger { get; }

        /// <summary> Gets the configuration object used to make this client. </summary>
        public DiscordConfig Config { get; }
        /// <summary> Gets the log manager. </summary>
        public LogManager Log { get; }
        /// <summary> Gets the internal RestClient for the Client API endpoint. </summary>
        public RestClient ClientAPI { get; }
        /// <summary> Gets the internal RestClient for the Status API endpoint. </summary>
        public RestClient StatusAPI { get; }
        /// <summary> Gets the internal WebSocket for the Gateway event stream. </summary>
        public GatewaySocket GatewaySocket { get; }
        /// <summary> Gets the service manager used for adding extensions to this client. </summary>
        public ServiceManager Services { get; }
        /// <summary> Gets the queue used for outgoing messages, if enabled. </summary>
        public MessageQueue MessageQueue { get; }
        /// <summary> Gets the JSON serializer used by this client. </summary>
        public JsonSerializer Serializer { get; }

        /// <summary> Gets the current connection state of this client. </summary>
        public ConnectionState State { get; private set; }
        /// <summary> Gets a cancellation token that triggers when the client is manually disconnected. </summary>
        public CancellationToken CancelToken { get; private set; }
        /// <summary> Gets the current logged-in user used in private channels. </summary>
        internal User PrivateUser { get; private set; }
        /// <summary> Gets information about the current logged-in account. </summary>
        public Profile CurrentUser { get; private set; }
        /// <summary> Gets the session id for the current connection. </summary>
        public string SessionId { get; private set; }
        /// <summary> Gets the status of the current user. </summary>
        public UserStatus Status { get; private set; }
        /// <summary> Gets the game the current user is displayed as playing. </summary>
        public string CurrentGame { get; private set; }

        /// <summary> Gets a collection of all servers this client is a member of. </summary>
        public IEnumerable<Server> Servers => _servers.Select(x => x.Value);
        // /// <summary> Gets a collection of all channels this client is a member of. </summary>
        // public IEnumerable<Channel> Channels => _channels.Select(x => x.Value);
        /// <summary> Gets a collection of all private channels this client is a member of. </summary>
        public IEnumerable<Channel> PrivateChannels => _privateChannels.Select(x => x.Value);
        /// <summary> Gets a collection of all voice regions currently offered by Discord. </summary>
        public IEnumerable<Region> Regions => _regions.Select(x => x.Value);

        /// <summary> Initializes a new instance of the DiscordClient class. </summary>
        public DiscordClient(Action<DiscordConfig> configFunc)
            : this(ProcessConfig(configFunc))
        {
        }
        private static DiscordConfig ProcessConfig(Action<DiscordConfig> func)
        {
            var config = new DiscordConfig();
            func(config);
            return config;
        }

        /// <summary> Initializes a new instance of the DiscordClient class. </summary>
        public DiscordClient()
            : this((DiscordConfig)null)
        {
        }

        /// <summary> Initializes a new instance of the DiscordClient class. </summary>
        public DiscordClient(DiscordConfig config)
        {
            Config = config ?? new DiscordConfig();
            Config.Lock();

            State = (int)ConnectionState.Disconnected;
            Status = UserStatus.Online;

            //Logging
            Log = new LogManager(this);
            Logger = Log.CreateLogger("Discord");

            //Async
            _taskManager = new TaskManager(Cleanup);
            _connectionLock = new AsyncLock();
            _disconnectedEvent = new ManualResetEvent(true);
            _connectedEvent = new ManualResetEventSlim(false);
            CancelToken = new CancellationToken(true);

            //Cache
            _servers = new ConcurrentDictionary<ulong, Server>();
            _channels = new ConcurrentDictionary<ulong, Channel>();
            _privateChannels = new ConcurrentDictionary<ulong, Channel>();

            //Serialization
            Serializer = new JsonSerializer();
            Serializer.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
#if TEST_RESPONSES
            Serializer.CheckAdditionalContent = true;
            Serializer.MissingMemberHandling = MissingMemberHandling.Error;
#else
            Serializer.CheckAdditionalContent = false;
            Serializer.MissingMemberHandling = MissingMemberHandling.Ignore;
#endif
            Serializer.Error += (s, e) =>
            {
                e.ErrorContext.Handled = true;
                Logger.Error("Serialization Failed", e.ErrorContext.Error);
            };

            //Networking
            ClientAPI = new RestClient(Config, DiscordConfig.ClientAPIUrl, Log.CreateLogger("ClientAPI"));
            StatusAPI = new RestClient(Config, DiscordConfig.StatusAPIUrl, Log.CreateLogger("StatusAPI"));
            GatewaySocket = new GatewaySocket(this, Log.CreateLogger("Gateway"));
            GatewaySocket.Connected += (s, e) =>
            {
                if (State == ConnectionState.Connecting)
                    EndConnect();
            };
            GatewaySocket.Disconnected += (s, e) => OnDisconnected(e.WasUnexpected, e.Exception);
            GatewaySocket.ReceivedDispatch += (s, e) => OnReceivedEvent(e);

            if (Config.UseMessageQueue)
                MessageQueue = new MessageQueue(this, Log.CreateLogger("MessageQueue"));

            //Extensibility
            Services = new ServiceManager(this);

            //Import/Export
            //_messageImporter = new JsonSerializer();
            //_messageImporter.ContractResolver = new Message.ImportResolver();
        }

        /// <summary> Connects to the Discord server with the provided email and password. </summary>
        /// <returns> Returns a token that can be optionally stored to speed up future connections. </returns>
        public async Task<string> Connect(string email, string password, string token = null)
        {
            if (email == null) throw new ArgumentNullException(email);
            if (password == null) throw new ArgumentNullException(password);

            await BeginConnect(email, password, null).ConfigureAwait(false);
            return ClientAPI.Token;
        }
        /// <summary> Connects to the Discord server with the provided token. </summary>
        public async Task Connect(string token)
        {
            if (token == null) throw new ArgumentNullException(token);

            await BeginConnect(null, null, token).ConfigureAwait(false);
        }

        private async Task BeginConnect(string email, string password, string token = null)
        {
            try
            {
                using (await _connectionLock.LockAsync().ConfigureAwait(false))
                {
                    if (State != ConnectionState.Disconnected)
                        await Disconnect().ConfigureAwait(false);
                    await _taskManager.Stop().ConfigureAwait(false);
                    _taskManager.ClearException();

                    Stopwatch stopwatch = null;
                    if (Config.LogLevel >= LogSeverity.Verbose)
                        stopwatch = Stopwatch.StartNew();
                    State = ConnectionState.Connecting;
                    _disconnectedEvent.Reset();

                    _cancelTokenSource = new CancellationTokenSource();
                    CancelToken = _cancelTokenSource.Token;
                    GatewaySocket.ParentCancelToken = CancelToken;

                    await Login(email, password, token).ConfigureAwait(false);
                    await GatewaySocket.Connect().ConfigureAwait(false);

                    List<Task> tasks = new List<Task>();
                    tasks.Add(CancelToken.Wait());
                    if (Config.UseMessageQueue)
                        tasks.Add(MessageQueue.Run(CancelToken, Config.MessageQueueInterval));

                    await _taskManager.Start(tasks, _cancelTokenSource).ConfigureAwait(false);
                    GatewaySocket.WaitForConnection(CancelToken);

                    if (Config.LogLevel >= LogSeverity.Verbose)
                    {
                        stopwatch.Stop();
                        double seconds = Math.Round(stopwatch.ElapsedTicks / (double)TimeSpan.TicksPerSecond, 2);
                        Logger.Verbose($"Connection took {seconds} sec");
                    }
                }
            }
            catch (Exception ex)
            {
                await _taskManager.SignalError(ex).ConfigureAwait(false);
                throw;
            }
        }
        private async Task Login(string email = null, string password = null, string token = null)
        {
            string tokenPath = null, oldToken = null;
            byte[] cacheKey = null;

            //Get Token
            if (token == null && Config.CacheToken)
            {
                Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password,
                    new byte[] { 0x5A, 0x2A, 0xF8, 0xCF, 0x78, 0xD3, 0x7D, 0x0D });
                cacheKey = deriveBytes.GetBytes(16);

                tokenPath = GetTokenCachePath(email);
                oldToken = LoadToken(tokenPath, cacheKey);
                ClientAPI.Token = oldToken;
            }
            else
                ClientAPI.Token = token;
                
            var request = new LoginRequest() { Email = email, Password = password };
            var response = await ClientAPI.Send(request).ConfigureAwait(false);
            token = response.Token;
            if (Config.CacheToken && token != oldToken)
                SaveToken(tokenPath, cacheKey, token);

            ClientAPI.Token = token;

            GatewaySocket.Token = token;
            GatewaySocket.SessionId = null;

            //Cache other stuff
            var regionsResponse = (await ClientAPI.Send(new GetVoiceRegionsRequest()).ConfigureAwait(false));
            _regions = regionsResponse.Select(x => new Region(x.Id, x.Name, x.Hostname, x.Port, x.Vip))
                .ToDictionary(x => x.Id);
        }
        private void EndConnect()
        {
            State = ConnectionState.Connected;
            _connectedEvent.Set();

            ClientAPI.CancelToken = CancelToken;
            SendStatus();
            OnConnected();
        }

        /// <summary> Disconnects from the Discord server, canceling any pending requests. </summary>
        public Task Disconnect() => _taskManager.Stop(true);
		private async Task Cleanup()
        {
            var oldState = State;
            State = ConnectionState.Disconnecting;

            if (oldState == ConnectionState.Connected)
                await ClientAPI.Send(new LogoutRequest()).ConfigureAwait(false);

            if (Config.UseMessageQueue)
                MessageQueue.Clear();


            await GatewaySocket.Disconnect().ConfigureAwait(false);
            ClientAPI.Token = null;
            GatewaySocket.Token = null;
            GatewaySocket.SessionId = null;

            _servers.Clear();
            _channels.Clear();
            _privateChannels.Clear();

            PrivateUser = null;
            CurrentUser = null;
            
            State = (int)ConnectionState.Disconnected;
            _connectedEvent.Reset();
            _disconnectedEvent.Set();
        }
        
        public void SetStatus(UserStatus status)
        {
            if (status == null) throw new ArgumentNullException(nameof(status));
            if (status != UserStatus.Online && status != UserStatus.Idle)
                throw new ArgumentException($"Invalid status, must be {UserStatus.Online} or {UserStatus.Idle}", nameof(status));

            Status = status;
            SendStatus();
        }
        public void SetGame(string game)
        {
            CurrentGame = game;
            SendStatus();
        }
        private void SendStatus()
        {
            PrivateUser.Status = Status;
            PrivateUser.CurrentGame = CurrentGame;
            foreach (var server in Servers)
            {
                var current = server.CurrentUser;
                if (current != null)
                {
                    current.Status = Status;
                    current.CurrentGame = CurrentGame;
                }
            }
            GatewaySocket.SendUpdateStatus(Status == UserStatus.Idle ? EpochTime.GetMilliseconds() - (10 * 60 * 1000) : (long?)null, CurrentGame);
        }

        #region Channels
        internal void AddChannel(Channel channel)
        {
            _channels.GetOrAdd(channel.Id, channel);
        }
        private Channel RemoveChannel(ulong id)
        {
            Channel channel;
            if (_channels.TryRemove(id, out channel))
            {
                if (channel.IsPrivate)
                    _privateChannels.TryRemove(channel.Recipient.Id, out channel);
                else
                    channel.Server.RemoveChannel(id);
            }
            return channel;
        }
        public Channel GetChannel(ulong id)
        {
            Channel channel;
            _channels.TryGetValue(id, out channel);
            return channel;
        }

        private Channel AddPrivateChannel(ulong id, ulong recipientId)
        {
            Channel channel;
            if (_channels.TryGetOrAdd(id, x => new Channel(this, x, new User(this, recipientId, null)), out channel))
                _privateChannels[recipientId] = channel;
            return channel;
        }
        internal Channel GetPrivateChannel(ulong recipientId)
        {
            Channel channel;
            _privateChannels.TryGetValue(recipientId, out channel);
            return channel;
        }
        internal Task<Channel> CreatePMChannel(User user)
            => CreatePrivateChannel(user.Id);
        public async Task<Channel> CreatePrivateChannel(ulong userId)
        {
            var channel = GetPrivateChannel(userId);
            if (channel != null) return channel;

            var request = new CreatePrivateChannelRequest() { RecipientId = userId };
            var response = await ClientAPI.Send(request).ConfigureAwait(false);

            channel = AddPrivateChannel(response.Id, userId);
            channel.Update(response);
            return channel;
        }
        #endregion

        #region Invites
        /// <summary> Gets more info about the provided invite code. </summary>
        /// <remarks> Supported formats: inviteCode, xkcdCode, https://discord.gg/inviteCode, https://discord.gg/xkcdCode </remarks>
        /// <returns> The invite object if found, null if not. </returns>
        public async Task<Invite> GetInvite(string inviteIdOrXkcd)
        {
            if (inviteIdOrXkcd == null) throw new ArgumentNullException(nameof(inviteIdOrXkcd));

            //Remove trailing slash
            if (inviteIdOrXkcd.Length > 0 && inviteIdOrXkcd[inviteIdOrXkcd.Length - 1] == '/')
                inviteIdOrXkcd = inviteIdOrXkcd.Substring(0, inviteIdOrXkcd.Length - 1);
            //Remove leading URL
            int index = inviteIdOrXkcd.LastIndexOf('/');
            if (index >= 0)
                inviteIdOrXkcd = inviteIdOrXkcd.Substring(index + 1);

            try
            {
                var response = await ClientAPI.Send(new GetInviteRequest(inviteIdOrXkcd)).ConfigureAwait(false);
                var invite = new Invite(this, response.Code, response.XkcdPass);
                invite.Update(response);
                return invite;
            }
            catch (HttpException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }
        #endregion

        #region Regions
        public Region GetRegion(string id)
        {
            Region region;
            if (_regions.TryGetValue(id, out region))
                return region;
            else
                return new Region(id, id, "", 0, false);
        }
        #endregion

        #region Servers
        private Server AddServer(ulong id)
            => _servers.GetOrAdd(id, x => new Server(this, x));
        private Server RemoveServer(ulong id)
        {
            Server server;
            if (_servers.TryRemove(id, out server))
            {
                foreach (var channel in server.AllChannels)
                    RemoveChannel(channel.Id);
            }
            return server;
        }

        public Server GetServer(ulong id)
        {
            Server server;
            _servers.TryGetValue(id, out server);
            return server;
        }
        public IEnumerable<Server> FindServers(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return _servers.Select(x => x.Value).Find(name);
        }

        /// <summary> Creates a new server with the provided name and region. </summary>
        public async Task<Server> CreateServer(string name, Region region, ImageType iconType = ImageType.None, Stream icon = null)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (region == null) throw new ArgumentNullException(nameof(region));

            var request = new CreateGuildRequest()
            {
                Name = name,
                Region = region.Id,
                IconBase64 = icon.Base64(iconType, null)
            };
            var response = await ClientAPI.Send(request).ConfigureAwait(false);

            var server = AddServer(response.Id);
            server.Update(response);
            return server;
        }
        #endregion

        #region Gateway Events
        private void OnReceivedEvent(WebSocketEventEventArgs e)
        {
            try
            {
                switch (e.Type)
                {
                    //Global
                    case "READY":
                        {
                            Stopwatch stopwatch = null;
                            if (Config.LogLevel >= LogSeverity.Verbose)
                                stopwatch = Stopwatch.StartNew();
                            var data = e.Payload.ToObject<ReadyEvent>(Serializer);
                            GatewaySocket.StartHeartbeat(data.HeartbeatInterval);
                            GatewaySocket.SessionId = data.SessionId;
                            SessionId = data.SessionId;
                            PrivateUser = new User(this, data.User.Id, null);
                            PrivateUser.Update(data.User);
                            CurrentUser = new Profile(this, data.User.Id);
                            CurrentUser.Update(data.User);
                            foreach (var model in data.Guilds)
                            {
                                if (model.Unavailable != true)
                                {
                                    var server = AddServer(model.Id);
                                    server.Update(model);
                                }
                            }
                            foreach (var model in data.PrivateChannels)
                            {
                                var channel = AddPrivateChannel(model.Id, model.Recipient.Id);
                                channel.Update(model);
                            }
                            if (Config.LogLevel >= LogSeverity.Verbose)
                            {
                                stopwatch.Stop();
                                double seconds = Math.Round(stopwatch.ElapsedTicks / (double)TimeSpan.TicksPerSecond, 2);
                                Logger.Verbose($"READY took {seconds} sec");
                            }
                        }
                        break;
                    case "RESUMED":
                        {
                            var data = e.Payload.ToObject<ResumedEvent>(Serializer);
                            GatewaySocket.StartHeartbeat(data.HeartbeatInterval);
                        }
                        break;

                    //Servers
                    case "GUILD_CREATE":
                        {
                            var data = e.Payload.ToObject<GuildCreateEvent>(Serializer);
                            if (data.Unavailable != true)
                            {
                                var server = AddServer(data.Id);
                                server.Update(data);

                                if (Config.LogEvents)
                                {
                                    if (data.Unavailable != false)
                                        Logger.Info($"Server Created: {server.Name}");
                                    else
                                        Logger.Info($"Server Available: {server.Name}");
                                }

                                if (data.Unavailable != false)
                                    OnJoinedServer(server);
                                OnServerAvailable(server);
                            }
                        }
                        break;
                    case "GUILD_UPDATE":
                        {
                            var data = e.Payload.ToObject<GuildUpdateEvent>(Serializer);
                            var server = GetServer(data.Id);
                            if (server != null)
                            {
                                server.Update(data);
                                if (Config.LogEvents)
                                    Logger.Info($"Server Updated: {server.Name}");
                                OnServerUpdated(server);
                            }
                            else
                                Logger.Warning("GUILD_UPDATE referenced an unknown guild.");
                        }
                        break;
                    case "GUILD_DELETE":
                        {
                            var data = e.Payload.ToObject<GuildDeleteEvent>(Serializer);
                            Server server = RemoveServer(data.Id);
                            if (server != null)
                            {
                                if (Config.LogEvents)
                                {
                                    if (data.Unavailable != true)
                                        Logger.Info($"Server Destroyed: {server.Name}");
                                    else
                                        Logger.Info($"Server Unavailable: {server.Name}");
                                }

                                OnServerUnavailable(server);
                                if (data.Unavailable != true)
                                    OnLeftServer(server);
                            }
                            else
                                Logger.Warning("GUILD_DELETE referenced an unknown guild.");
                        }
                        break;

                    //Channels
                    case "CHANNEL_CREATE":
                        {
                            var data = e.Payload.ToObject<ChannelCreateEvent>(Serializer);

                            Channel channel = null;
                            if (data.GuildId != null)
                            {
                                var server = GetServer(data.GuildId.Value);
                                if (server != null)
                                    channel = server.AddChannel(data.Id);
                                else
                                    Logger.Warning("CHANNEL_CREATE referenced an unknown guild.");
                            }
                            else
                                channel = AddPrivateChannel(data.Id, data.Recipient.Id);
                            if (channel != null)
                            {
                                channel.Update(data);
                                if (Config.LogEvents)
                                    Logger.Info($"Channel Created: {channel.Server?.Name ?? "[Private]"}/{channel.Name}");
                                OnChannelCreated(channel);
                            }
                        }
                        break;
                    case "CHANNEL_UPDATE":
                        {
                            var data = e.Payload.ToObject<ChannelUpdateEvent>(Serializer);
                            var channel = GetChannel(data.Id);
                            if (channel != null)
                            {
                                channel.Update(data);
                                if (Config.LogEvents)
                                    Logger.Info($"Channel Updated: {channel.Server?.Name ?? "[Private]"}/{channel.Name}");
                                OnChannelUpdated(channel);
                            }
                            else
                                Logger.Warning("CHANNEL_UPDATE referenced an unknown channel.");
                        }
                        break;
                    case "CHANNEL_DELETE":
                        {
                            var data = e.Payload.ToObject<ChannelDeleteEvent>(Serializer);
                            var channel = RemoveChannel(data.Id);
                            if (channel != null)
                            {
                                if (Config.LogEvents)
                                    Logger.Info($"Channel Destroyed: {channel.Server?.Name ?? "[Private]"}/{channel.Name}");
                                OnChannelDestroyed(channel);
                            }
                            else
                                Logger.Warning("CHANNEL_DELETE referenced an unknown channel.");
                        }
                        break;

                    //Members
                    case "GUILD_MEMBER_ADD":
                        {
                            var data = e.Payload.ToObject<GuildMemberAddEvent>(Serializer);
                            var server = GetServer(data.GuildId.Value);
                            if (server != null)
                            {
                                var user = server.AddUser(data.User.Id);
                                user.Update(data);
                                user.UpdateActivity();
                                if (Config.LogEvents)
                                    Logger.Info($"User Joined: {server.Name}/{user.Name}");
                                OnUserJoined(user);
                            }
                            else
                                Logger.Warning("GUILD_MEMBER_ADD referenced an unknown guild.");
                        }
                        break;
                    case "GUILD_MEMBER_UPDATE":
                        {
                            var data = e.Payload.ToObject<GuildMemberUpdateEvent>(Serializer);
                            var server = GetServer(data.GuildId.Value);
                            if (server != null)
                            {
                                var user = server.GetUser(data.User.Id);
                                if (user != null)
                                {
                                    user.Update(data);
                                    if (Config.LogEvents)
                                        Logger.Info($"User Updated: {server.Name}/{user.Name}");
                                    OnUserUpdated(user);
                                }
                                else
                                    Logger.Warning("GUILD_MEMBER_UPDATE referenced an unknown user.");
                            }
                            else
                                Logger.Warning("GUILD_MEMBER_UPDATE referenced an unknown guild.");
                        }
                        break;
                    case "GUILD_MEMBER_REMOVE":
                        {
                            var data = e.Payload.ToObject<GuildMemberRemoveEvent>(Serializer);
                            var server = GetServer(data.GuildId.Value);
                            if (server != null)
                            {
                                var user = server.RemoveUser(data.User.Id);
                                if (user != null)
                                {
                                    if (Config.LogEvents)
                                        Logger.Info($"User Left: {server.Name}/{user.Name}");
                                    OnUserLeft(user);
                                }
                                else
                                    Logger.Warning("GUILD_MEMBER_REMOVE referenced an unknown user.");
                            }
                            else
                                Logger.Warning("GUILD_MEMBER_REMOVE referenced an unknown guild.");
                        }
                        break;
                    case "GUILD_MEMBERS_CHUNK":
                        {
                            var data = e.Payload.ToObject<GuildMembersChunkEvent>(Serializer);
                            var server = GetServer(data.GuildId);
                            if (server != null)
                            {
                                foreach (var memberData in data.Members)
                                {
                                    var user = server.AddUser(memberData.User.Id);
                                    user.Update(memberData);
                                    //OnUserAdded(user);
                                }
                            }
                            else
                                Logger.Warning("GUILD_MEMBERS_CHUNK referenced an unknown guild.");
                        }
                        break;

                    //Roles
                    case "GUILD_ROLE_CREATE":
                        {
                            var data = e.Payload.ToObject<GuildRoleCreateEvent>(Serializer);
                            var server = GetServer(data.GuildId);
                            if (server != null)
                            {
                                var role = server.AddRole(data.Data.Id);
                                role.Update(data.Data);
                                if (Config.LogEvents)
                                    Logger.Info($"Role Created: {server.Name}/{role.Name}");
                                OnRoleUpdated(role);
                            }
                            else
                                Logger.Warning("GUILD_ROLE_CREATE referenced an unknown guild.");
                        }
                        break;
                    case "GUILD_ROLE_UPDATE":
                        {
                            var data = e.Payload.ToObject<GuildRoleUpdateEvent>(Serializer);
                            var server = GetServer(data.GuildId);
                            if (server != null)
                            {
                                var role = server.GetRole(data.Data.Id);
                                if (role != null)
                                {
                                    role.Update(data.Data);
                                    if (Config.LogEvents)
                                        Logger.Info($"Role Updated: {server.Name}/{role.Name}");
                                    OnRoleUpdated(role);
                                }
                                else
                                    Logger.Warning("GUILD_ROLE_UPDATE referenced an unknown role.");
                            }
                            else
                                Logger.Warning("GUILD_ROLE_UPDATE referenced an unknown guild.");
                        }
                        break;
                    case "GUILD_ROLE_DELETE":
                        {
                            var data = e.Payload.ToObject<GuildRoleDeleteEvent>(Serializer);
                            var server = GetServer(data.GuildId);
                            if (server != null)
                            {
                                var role = server.RemoveRole(data.RoleId);
                                if (role != null)
                                {
                                    if (Config.LogEvents)
                                        Logger.Info($"Role Deleted: {server.Name}/{role.Name}");
                                    OnRoleDeleted(role);
                                }
                                else
                                    Logger.Warning("GUILD_ROLE_DELETE referenced an unknown role.");
                            }
                            else
                                Logger.Warning("GUILD_ROLE_DELETE referenced an unknown guild.");
                        }
                        break;

                    //Bans
                    case "GUILD_BAN_ADD":
                        {
                            var data = e.Payload.ToObject<GuildBanAddEvent>(Serializer);
                            var server = GetServer(data.GuildId.Value);
                            if (server != null)
                            {
                                var user = server.GetUser(data.User.Id);
                                if (user != null)
                                {
                                    if (Config.LogEvents)
                                        Logger.Info($"User Banned: {server.Name}/{user.Name}");
                                    OnUserBanned(user);
                                }
                                else
                                    Logger.Warning("GUILD_BAN_ADD referenced an unknown user.");
                            }
                            else
                                Logger.Warning("GUILD_BAN_ADD referenced an unknown guild.");
                        }
                        break;
                    case "GUILD_BAN_REMOVE":
                        {
                            var data = e.Payload.ToObject<GuildBanRemoveEvent>(Serializer);
                            var server = GetServer(data.GuildId.Value);
                            if (server != null)
                            {
                                var user = new User(this, data.User.Id, server);
                                user.Update(data.User);
                                if (Config.LogEvents)
                                    Logger.Info($"User Unbanned: {server.Name}/{user.Name}");
                                OnUserUnbanned(user);
                            }
                            else
                                Logger.Warning("GUILD_BAN_REMOVE referenced an unknown guild.");
                        }
                        break;

                    //Messages
                    case "MESSAGE_CREATE":
                        {
                            var data = e.Payload.ToObject<MessageCreateEvent>(Serializer);

                            Channel channel = GetChannel(data.ChannelId);
                            if (channel != null)
                            {
                                var user = channel.GetUser(data.Author.Id);
                                if (user != null)
                                {
                                    Message msg = null;
                                    bool isAuthor = data.Author.Id == CurrentUser.Id;
                                    //ulong nonce = 0;

                                    /*if (data.Author.Id == _privateUser.Id && Config.UseMessageQueue)
                                    {
                                        if (data.Nonce != null && ulong.TryParse(data.Nonce, out nonce))
                                            msg = _messages[nonce];
                                    }*/
                                    if (msg == null)
                                    {
                                        msg = channel.AddMessage(data.Id, user, data.Timestamp.Value);
                                        //nonce = 0;
                                    }

                                    //Remapped queued message
                                    /*if (nonce != 0)
                                    {
                                        msg = _messages.Remap(nonce, data.Id);
                                        msg.Id = data.Id;
                                        RaiseMessageSent(msg);
                                    }*/

                                    msg.Update(data);
                                    user.UpdateActivity();

                                    if (Config.LogEvents)
                                        Logger.Verbose($"Message Received: {channel.Server?.Name ?? "[Private]"}/{channel.Name}");
                                    OnMessageReceived(msg);
                                }
                            }
                            else
                                Logger.Warning("MESSAGE_CREATE referenced an unknown channel.");
                        }
                        break;
                    case "MESSAGE_UPDATE":
                        {
                            var data = e.Payload.ToObject<MessageUpdateEvent>(Serializer);
                            var channel = GetChannel(data.ChannelId);
                            if (channel != null)
                            {
                                var msg = channel.GetMessage(data.Id, data.Author?.Id);
                                msg.Update(data);
                                if (Config.LogEvents)
                                    Logger.Verbose($"Message Update: {channel.Server?.Name ?? "[Private]"}/{channel.Name}");
                                OnMessageUpdated(msg);
                            }
                            else
                                Logger.Warning("MESSAGE_UPDATE referenced an unknown channel.");
                        }
                        break;
                    case "MESSAGE_DELETE":
                        {
                            var data = e.Payload.ToObject<MessageDeleteEvent>(Serializer);
                            var channel = GetChannel(data.ChannelId);
                            if (channel != null)
                            {
                                var msg = channel.RemoveMessage(data.Id);
                                if (Config.LogEvents)
                                    Logger.Verbose($"Message Deleted: {channel.Server?.Name ?? "[Private]"}/{channel.Name}");
                                OnMessageDeleted(msg);
                            }
                            else
                                Logger.Warning("MESSAGE_DELETE referenced an unknown channel.");
                        }
                        break;
                    case "MESSAGE_ACK":
                        {
                            if (Config.MessageCacheSize > 0)
                            {
                                var data = e.Payload.ToObject<MessageAckEvent>(Serializer);
                                var channel = GetChannel(data.ChannelId);
                                if (channel != null)
                                {
                                    var msg = channel.GetMessage(data.MessageId, null);
                                    if (msg != null)
                                    {
                                        if (Config.LogEvents)
                                            Logger.Verbose($"Message Ack: {channel.Server?.Name ?? "[Private]"}/{channel.Name}");
                                        OnMessageAcknowledged(msg);
                                    }
                                }
                                else
                                    Logger.Warning("MESSAGE_ACK referenced an unknown channel.");
                            }
                        }
                        break;

                    //Statuses
                    case "PRESENCE_UPDATE":
                        {
                            var data = e.Payload.ToObject<PresenceUpdateEvent>(Serializer);
                            User user;
                            Server server;
                            if (data.GuildId == null)
                            {
                                server = null;
                                user = GetPrivateChannel(data.User.Id)?.Recipient;
                            }
                            else
                            {
                                server = GetServer(data.GuildId.Value);
                                if (server == null)
                                {
                                    Logger.Warning("PRESENCE_UPDATE referenced an unknown server.");
                                    break;
                                }
                                else
                                {
                                    if (Config.UseLargeThreshold)
                                        user = server.AddUser(data.User.Id);
                                    else
                                        user = server.GetUser(data.User.Id);
                                }
                            }

                            if (user != null)
                            {
                                user.Update(data);
                                //Logger.Verbose($"Presence Updated: {server.Name}/{user.Name}");
                                OnUserPresenceUpdated(user);
                            }
                            /*else //Occurs when a user leaves a server
                                Logger.Warning("PRESENCE_UPDATE referenced an unknown user.");*/
                        }
                        break;
                    case "TYPING_START":
                        {
                            var data = e.Payload.ToObject<TypingStartEvent>(Serializer);
                            var channel = GetChannel(data.ChannelId);
                            if (channel != null)
                            {
                                User user;
                                if (channel.IsPrivate)
                                {
                                    if (channel.Recipient.Id == data.UserId)
                                        user = channel.Recipient;
                                    else
                                        return; ;
                                }
                                else
                                    user = channel.Server.GetUser(data.UserId);
                                if (user != null)
                                {
                                    //Logger.Verbose($"Is Typing: {channel.Server?.Name ?? "[Private]"}/{channel.Name}/{user.Name}");
                                    OnUserIsTypingUpdated(channel, user);
                                    user.UpdateActivity();
                                }
                            }
                            else
                                Logger.Warning("TYPING_START referenced an unknown channel.");
                        }
                        break;

                    //Voice
                    case "VOICE_STATE_UPDATE":
                        {
                            var data = e.Payload.ToObject<VoiceStateUpdateEvent>(Serializer);
                            var server = GetServer(data.GuildId);
                            if (server != null)
                            {
                                var user = server.GetUser(data.UserId);
                                if (user != null)
                                {
                                    user.Update(data);
                                    //Logger.Verbose($"Voice Updated: {server.Name}/{user.Name}");
                                    OnUserVoiceStateUpdated(user);
                                }
                                /*else //Occurs when a user leaves a server
                                    Logger.Warning("VOICE_STATE_UPDATE referenced an unknown user.");*/
                            }
                            else
                                Logger.Warning("VOICE_STATE_UPDATE referenced an unknown server.");
                        }
                        break;

                    //Settings
                    case "USER_UPDATE":
                        {
                            var data = e.Payload.ToObject<UserUpdateEvent>(Serializer);
                            if (data.Id == CurrentUser.Id)
                            {
                                CurrentUser.Update(data);
                                PrivateUser.Update(data);
                                foreach (var server in _servers)
                                    server.Value.CurrentUser.Update(data);
                                if (Config.LogEvents)
                                    Logger.Info("Profile Updated");
                                OnProfileUpdated(CurrentUser);
                            }
                        }
                        break;

                    //Ignored
                    case "USER_SETTINGS_UPDATE":
                    case "GUILD_INTEGRATIONS_UPDATE":
                    case "VOICE_SERVER_UPDATE":
                    case "GUILD_EMOJIS_UPDATE":
                        break;

                    //Others
                    default:
                        Logger.Warning($"Unknown message type: {e.Type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error handling {e.Type} event", ex);
            }
        }
        #endregion

        #region Async Wrapper
        /// <summary> Blocking call that will not return until client has been stopped. This is mainly intended for use in console applications. </summary>
        public void Run(Func<Task> asyncAction)
        {
            try
            {
                AsyncContext.Run(asyncAction);
            }
            catch (TaskCanceledException) { }
            _disconnectedEvent.WaitOne();
        }
        /// <summary> Blocking call that will not return until client has been stopped. This is mainly intended for use in console applications. </summary>
        public void Wait()
        {
            _disconnectedEvent.WaitOne();
        }
        #endregion

        #region IDisposable
        private bool _isDisposed = false;

        protected virtual void Dispose(bool isDisposing)
        {
            if (!_isDisposed)
            {
                if (isDisposing)
                {
                    _disconnectedEvent.Dispose();
                    _connectedEvent.Dispose();
                }
                _isDisposed = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        //Helpers
        private string GetTokenCachePath(string email)
        {
            using (var md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(email.ToLowerInvariant()));
                StringBuilder filenameBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                    filenameBuilder.Append(data[i].ToString("x2"));
                return Path.Combine(Path.GetTempPath(), Config.AppName ?? "Discord.Net", filenameBuilder.ToString());
            }
        }
        private string LoadToken(string path, byte[] key)
        {
            if (File.Exists(path))
            {
                try
                {
                    using (var fileStream = File.Open(path, FileMode.Open))
                    using (var aes = Aes.Create())
                    {
                        byte[] iv = new byte[aes.BlockSize / 8];
                        fileStream.Read(iv, 0, iv.Length);
                        aes.IV = iv;
                        aes.Key = key;
                        using (var cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            byte[] tokenBuffer = new byte[64];
                            int length = cryptoStream.Read(tokenBuffer, 0, tokenBuffer.Length);
                            return Encoding.UTF8.GetString(tokenBuffer, 0, length);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning("Failed to load cached token. Wrong/changed password?", ex);
                }
            }
            return null;
        }
        private void SaveToken(string path, byte[] key, string token)
        {
            byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
            try
            {
                string parentDir = Path.GetDirectoryName(path);
                if (!Directory.Exists(parentDir))
                    Directory.CreateDirectory(parentDir);

                using (var fileStream = File.Open(path, FileMode.Create))
                using (var aes = Aes.Create())
                {
                    aes.GenerateIV();
                    aes.Key = key;
                    using (var cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        fileStream.Write(aes.IV, 0, aes.IV.Length);
                        cryptoStream.Write(tokenBytes, 0, tokenBytes.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning("Failed to cache token", ex);
            }
        }
    }
}