using FactionsAtTheEnd.Interfaces;
using Serilog;

namespace FactionsAtTheEnd.Core
{
    /// <summary>
    /// Provides a wrapper for injecting Serilog ILogger into services.
    /// </summary>
    public class AppLogger(ILogger logger) : IAppLogger
    {
        public ILogger Logger { get; } = logger;

        public void Information(string messageTemplate, params object[] propertyValues) =>
            Logger.Information(messageTemplate, propertyValues);

        public void Warning(string messageTemplate, params object[] propertyValues) =>
            Logger.Warning(messageTemplate, propertyValues);

        public void Error(
            Exception exception,
            string messageTemplate,
            params object[] propertyValues
        ) => Logger.Error(exception, messageTemplate, propertyValues);

        public void Error(string messageTemplate, params object[] propertyValues) =>
            Logger.Error(messageTemplate, propertyValues);

        public void Debug(string messageTemplate, params object[] propertyValues) =>
            Logger.Debug(messageTemplate, propertyValues);
    }
}
