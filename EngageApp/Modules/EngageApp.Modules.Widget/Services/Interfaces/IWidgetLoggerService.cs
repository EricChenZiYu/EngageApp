using System;

namespace EngageApp.Modules.Widget.Services.Interfaces
{
    /// <summary>
    /// Interface for widget logging service
    /// </summary>
    public interface IWidgetLoggerService
    {
        /// <summary>
        /// Initialize the logger (create console if needed)
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Log an informational message
        /// </summary>
        void Info(string message);
        
        /// <summary>
        /// Log a warning message
        /// </summary>
        void Warning(string message);
        
        /// <summary>
        /// Log an error message
        /// </summary>
        void Error(string message);
        
        /// <summary>
        /// Log an exception
        /// </summary>
        void Error(string message, Exception exception);
        
        /// <summary>
        /// Log a debug message
        /// </summary>
        void Debug(string message);
    }
} 