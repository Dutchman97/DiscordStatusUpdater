﻿using System;

namespace Discord.Logging
{
    public sealed class Logger
    {
        private readonly LogManager _manager;

        public string Name { get; }
        public LogSeverity Level => _manager.Level;

        internal Logger(LogManager manager, string name)
        {
            _manager = manager;
            Name = name;
        }

        public void Log(LogSeverity severity, string message, Exception exception = null)
            => _manager.Log(severity, Name, message, exception);
#if !NET45
        public void Log(LogSeverity severity, FormattableString message, Exception exception = null)
            => _manager.Log(severity, Name, message, exception);
#endif

        public void Error(string message, Exception exception = null)
            => _manager.Error(Name, message, exception);
#if !NET45
        public void Error(FormattableString message, Exception exception = null)
            => _manager.Error(Name, message, exception);
#endif
        public void Error(Exception exception)
            => _manager.Error(Name, exception);

        public void Warning(string message, Exception exception = null)
            => _manager.Warning(Name, message, exception);
#if !NET45
        public void Warning(FormattableString message, Exception exception = null)
            => _manager.Warning(Name, message, exception);
#endif
        public void Warning(Exception exception)
            => _manager.Warning(Name, exception);

        public void Info(string message, Exception exception = null)
            => _manager.Info(Name, message, exception);
#if !NET45
        public void Info(FormattableString message, Exception exception = null)
            => _manager.Info(Name, message, exception);
#endif
        public void Info(Exception exception)
            => _manager.Info(Name, exception);

        public void Verbose(string message, Exception exception = null)
            => _manager.Verbose(Name, message, exception);
#if !NET45
        public void Verbose(FormattableString message, Exception exception = null)
            => _manager.Verbose(Name, message, exception);
#endif
        public void Verbose(Exception exception)
            => _manager.Verbose(Name, exception);

        public void Debug(string message, Exception exception = null)
            => _manager.Debug(Name, message, exception);
#if !NET45
        public void Debug(FormattableString message, Exception exception = null)
            => _manager.Debug(Name, message, exception);
#endif
        public void Debug(Exception exception)
            => _manager.Debug(Name, exception);
    }
}
