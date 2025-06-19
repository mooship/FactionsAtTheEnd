using Serilog;

namespace FactionsAtTheEnd.Interfaces
{
    /// <summary>
    /// Interface for application-wide logger abstraction.
    /// </summary>
    public interface IAppLogger
    {
        ILogger Logger { get; }
        void Information(string messageTemplate, params object[] propertyValues);
        void Warning(string messageTemplate, params object[] propertyValues);
        void Error(Exception exception, string messageTemplate, params object[] propertyValues);
        void Error(string messageTemplate, params object[] propertyValues);
        void Debug(string messageTemplate, params object[] propertyValues);
    }
}
