using System;
using System.Runtime.InteropServices;
using System.Windows;
using EngageApp.Modules.Widget.Services.Interfaces;

namespace EngageApp.Modules.Widget.Services
{
    /// <summary>
    /// Implementation of the widget logger service
    /// </summary>
    public class WidgetLoggerService : IWidgetLoggerService
    {
        private bool _isInitialized;
        private readonly bool _enableConsoleLogging;
        private readonly bool _enableFileLogging;
        private readonly string _logFilePath;
        
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WidgetLoggerService"/> class.
        /// </summary>
        /// <param name="enableConsoleLogging">Enable console logging</param>
        /// <param name="enableFileLogging">Enable file logging</param>
        /// <param name="logFilePath">Path to log file (if file logging enabled)</param>
        public WidgetLoggerService(bool enableConsoleLogging = true, bool enableFileLogging = false, string logFilePath = "widget.log")
        {
            _enableConsoleLogging = enableConsoleLogging;
            _enableFileLogging = enableFileLogging;
            _logFilePath = logFilePath;
        }
        
        /// <inheritdoc/>
        public void Initialize()
        {
            if (_isInitialized)
                return;
                
            if (_enableConsoleLogging)
            {
                try
                {
                    AllocConsole();
                    Info("Console logging initialized");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to initialize console logging: {ex.Message}");
                }
            }
            
            if (_enableFileLogging)
            {
                try
                {
                    // Clear the log file on startup
                    System.IO.File.WriteAllText(_logFilePath, $"=== Widget Log Started {DateTime.Now} ===\r\n");
                    Info("File logging initialized");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to initialize file logging: {ex.Message}");
                }
            }
            
            _isInitialized = true;
        }
        
        /// <inheritdoc/>
        public void Info(string message)
        {
            LogMessage("INFO", message);
        }
        
        /// <inheritdoc/>
        public void Warning(string message)
        {
            LogMessage("WARN", message);
        }
        
        /// <inheritdoc/>
        public void Error(string message)
        {
            LogMessage("ERROR", message);
        }
        
        /// <inheritdoc/>
        public void Error(string message, Exception exception)
        {
            LogMessage("ERROR", $"{message} - {exception.Message}\r\nStackTrace: {exception.StackTrace}");
        }
        
        /// <inheritdoc/>
        public void Debug(string message)
        {
            LogMessage("DEBUG", message);
        }
        
        private void LogMessage(string level, string message)
        {
            string formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
            
            if (_enableConsoleLogging)
            {
                try
                {
                    Console.WriteLine(formattedMessage);
                }
                catch
                {
                    // Ignore console errors
                }
            }
            
            if (_enableFileLogging)
            {
                try
                {
                    System.IO.File.AppendAllText(_logFilePath, formattedMessage + "\r\n");
                }
                catch
                {
                    // Ignore file errors
                }
            }
        }
    }
} 