# EngageApp - Desktop Widget Transition System

A WPF/Prism application featuring a sleek desktop interaction flow where the main window can collapse into a persistent top-right corner widget, with smooth animations and intuitive controls.

## Features

- **Custom Window Chrome**: Styled minimize and close buttons with animations
- **Persistent Corner Widget**: Semi-transparent widget appears when main window is minimized
- **Touch and Mouse Support**: Drag-and-drop, hover effects, and touch gestures
- **Multi-Monitor Awareness**: Correctly positions on any screen
- **Magnetic Edge Snapping**: Widget snaps to screen edges when dragged nearby

## Implementation Details

### Core Components

1. **MainWindow** - Main application window with custom chrome
   - Minimizes with fade/shrink animation
   - Can be restored from widget

2. **WidgetView** - Persistent corner widget
   - Expands on hover
   - Restores main window on click
   - Snaps to screen edges when dragged nearby

3. **Event Communication**
   - `WindowMinimizedEvent` - Fired when main window is minimized
   - `WindowRestoreRequestedEvent` - Fired when widget is clicked

### Services

- **ScreenPositionService** - Handles multi-monitor positioning and screen edge detection

## Extending the Widget

### Adding Tray Menu Integration

To add a context menu to the widget:

1. Modify `WidgetView.xaml`:
```xml
<Border x:Name="WidgetBorder" ... ContextMenuService.IsEnabled="True">
    <Border.ContextMenu>
        <ContextMenu>
            <MenuItem Header="Settings" Command="{Binding SettingsCommand}" />
            <MenuItem Header="About" Command="{Binding AboutCommand}" />
            <Separator />
            <MenuItem Header="Exit" Command="{Binding ExitCommand}" />
        </ContextMenu>
    </Border.ContextMenu>
</Border>
```

2. Update `WidgetViewModel.cs` to add the necessary commands.

### Customizing Widget Appearance

The widget appearance can be customized by modifying `WidgetView.xaml`:

- Change the color by updating the `Background` property of `WidgetBorder`
- Adjust the glow effect by modifying the `DropShadowEffect` properties
- Change size by updating the `Width` and `Height` properties and animation values

### Adding Widget Functionality

The widget can be extended with additional functionality:

1. Add click or gesture handlers in `WidgetView.xaml.cs`
2. Create new events in the `EngageApp.Core.Events` namespace
3. Update the ViewModel to handle new interactions

## Performance Considerations

- Animations are GPU-accelerated via `RenderOptions.BitmapScalingMode="HighQuality"`
- Touch input uses `ManipulationDelta` for smooth interactions with inertia
- Widget uses minimal resources when inactive 