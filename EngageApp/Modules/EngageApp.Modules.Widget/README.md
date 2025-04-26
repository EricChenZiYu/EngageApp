# EngageApp Widget Module

A reusable WPF widget system providing floating desktop widgets that can be minimized to and restored from the screen edge.

## Features

- **Customizable Widget Appearance**: Change colors, text, and glow effects
- **High DPI Support**: Works correctly on 4K and high-resolution displays
- **Screen Edge Snapping**: Widgets snap to screen edges when dragged nearby
- **Smooth Animations**: Hover effects and transitions with GPU acceleration
- **Multi-Monitor Support**: Widgets correctly position on any screen
- **Event-Based Interaction**: Subscribe to click and double-click events
- **Comprehensive Logging**: Debug, info, warning, and error logging to console and file

## Services

The widget module includes the following services:

### IWidgetService

The main service for managing widgets:

```csharp
// Show the widget
widgetService.ShowWidget();

// Hide the widget
widgetService.HideWidget();

// Check if widget is visible
bool isVisible = widgetService.IsWidgetVisible;

// Customize appearance
widgetService.CustomizeWidget(
    backgroundColor: "#FF0000",   // Red background
    text: "A",                    // Display letter "A"
    textColor: "#FFFFFF",         // White text
    glowColor: "#FF0000"          // Red glow
);

// Subscribe to events
widgetService.WidgetClicked += (s, e) => 
{
    Console.WriteLine("Widget was clicked");
};

widgetService.WidgetDoubleClicked += (s, e) => 
{
    Console.WriteLine("Widget was double-clicked");
};
```

### IWidgetLoggerService

Service for logging widget-related events:

```csharp
// Initialize the logger
loggerService.Initialize();

// Log messages
loggerService.Info("Information message");
loggerService.Debug("Debug message");
loggerService.Warning("Warning message");
loggerService.Error("Error message");
loggerService.Error("Error with exception", exception);
```

### IWidgetScreenService

Service for positioning widgets on screen:

```csharp
// Position at top-right
screenService.PositionWindowTopRight(window);

// Position at center
screenService.PositionWindowCenter(window);

// Get screen working area
Rect workingArea = screenService.GetScreenWorkingArea(window);

// Snap to nearest edge
bool wasSnapped = screenService.SnapToNearestEdge(window, snapDistance: 20);
```

### IDpiAwarenessService

Service for handling high DPI displays:

```csharp
// Enable high DPI support
dpiService.EnableHighDpiSupport();

// Get the current system DPI scale (e.g., 1.0, 1.25, 1.5, etc.)
double dpiScale = dpiService.GetSystemDpiScale();
```

## Usage

1. **Add the module reference** to your Prism application:

```csharp
protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
{
    moduleCatalog.AddModule<WidgetModule>();
}
```

2. **Inject the widget service** where needed:

```csharp
public class MainWindowViewModel
{
    private readonly IWidgetService _widgetService;
    
    public MainWindowViewModel(IWidgetService widgetService)
    {
        _widgetService = widgetService;
    }
    
    private void MinimizeWindow()
    {
        // Hide main window
        Application.Current.MainWindow.Hide();
        
        // Show widget
        _widgetService.ShowWidget();
    }
}
```

3. **Subscribe to widget events**:

```csharp
_widgetService.WidgetDoubleClicked += (s, e) =>
{
    // Restore main window
    Application.Current.MainWindow.Show();
    Application.Current.MainWindow.Activate();
    
    // Hide widget
    _widgetService.HideWidget();
};
```

## Customization

You can customize the widget's appearance by calling:

```csharp
_widgetService.CustomizeWidget(
    backgroundColor: "#4070FF", // Blue background
    text: "M",                  // Letter "M"
    textColor: "#FFFFFF",       // White text
    glowColor: "#4070FF"        // Blue glow
);
``` 