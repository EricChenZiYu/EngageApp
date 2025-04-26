namespace EngageApp.Modules.Widget.Services.Interfaces
{
    /// <summary>
    /// Interface for DPI awareness service
    /// </summary>
    public interface IDpiAwarenessService
    {
        /// <summary>
        /// Enables high DPI awareness for the application
        /// </summary>
        void EnableHighDpiSupport();
        
        /// <summary>
        /// Gets the current DPI scale factor for the system
        /// </summary>
        /// <returns>The DPI scale as a double (e.g., 1.0, 1.25, 1.5, etc.)</returns>
        double GetSystemDpiScale();
    }
} 