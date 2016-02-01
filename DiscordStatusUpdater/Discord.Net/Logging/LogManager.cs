﻿using System;

namespace Discord.Logging
{
    public sealed class LogManager
    {
        private readonly DiscordClient _client;

        public LogSeverity Level { get; }

        public event EventHandler<LogMessageEventArgs> Message = delegate { };

        internal LogManager(DiscordClient client)
		{
            _client = client;
            Level = client.Config.LogLevel;
        }

#if !NET45
        public void Log(LogSeverity severity, string source, FormattableString message, Exception exception = null)
        {
            if (severity <= Level)
            {
                try { Message(this, new LogMessageEventArgs(severity, source, message.ToString(), exception)); }
                catch { } //We dont want to log on log errors
            }
        }
#endif
        public void Log(LogSeverity severity, string source, string message, Exception exception = null)
        {
            if (severity <= Level)
            {
                try { Message(this, new LogMessageEventArgs(severity, source, message, exception)); }
                catch { } //We dont want to log on log errors
            }
        }

        public void Error(string source, string message, Exception ex = null)
            => Log(LogSeverity.Error, source, message, ex);
#if !NET45
        public void Error(string source, FormattableString message, Exception ex = null)
            => Log(LogSeverity.Error, source, message, ex);
#endif
        public void Error(string source, Exception ex)
            => Log(LogSeverity.Error, source, (string)null, ex);

        public void Warning(string source, string message, Exception ex = null)
            => Log(LogSeverity.Warning, source, message, ex);
#if !NET45
        public void Warning(string source, FormattableString message, Exception ex = null)
            => Log(LogSeverity.Warning, source, message, ex);
#endif
        public void Warning(string source, Exception ex)
            => Log(LogSeverity.Warning, source, (string)null, ex);

        public void Info(string source, string message, Exception ex = null)
            => Log(LogSeverity.Info, source, message, ex);
#if !NET45
        public void Info(string source, FormattableString message, Exception ex = null)
            => Log(LogSeverity.Info, source, message, ex);
#endif
        public void Info(string source, Exception ex)
            => Log(LogSeverity.Info, source, (string)null, ex);

        public void Verbose(string source, string message, Exception ex = null)
            => Log(LogSeverity.Verbose, source, message, ex);
#if !NET45
        public void Verbose(string source, FormattableString message, Exception ex = null)
            => Log(LogSeverity.Verbose, source, message, ex);
#endif
        public void Verbose(string source, Exception ex)
            => Log(LogSeverity.Verbose, source, (string)null, ex);

        public void Debug(string source, string message, Exception ex = null)
            => Log(LogSeverity.Debug, source, message, ex);
#if !NET45
        public void Debug(string source, FormattableString message, Exception ex = null)
            => Log(LogSeverity.Debug, source, message, ex);
#endif
        public void Debug(string source, Exception ex)
            => Log(LogSeverity.Debug, source, (string)null, ex);

        public Logger CreateLogger(string name) => new Logger(this, name);
    }
}
