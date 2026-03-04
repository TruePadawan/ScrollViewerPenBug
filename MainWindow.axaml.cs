using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;

namespace ScrollViewerPenBugRepro;

public partial class MainWindow : Window
{
    private Point _prevPoint = new(-1, -1);
    private bool _isDrawing;
    private int _eventCount;
    private readonly List<Line> _currentStroke = new();

    public MainWindow()
    {
        InitializeComponent();

        var version = typeof(Control).Assembly.GetName().Version;
        VersionText.Text = $"Avalonia {version}";
    }

    private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(DrawingCanvas).Properties.IsLeftButtonPressed)
            return;

        _prevPoint = e.GetPosition(DrawingCanvas);
        _isDrawing = true;
        _eventCount = 0;
        _currentStroke.Clear();

        // Log pointer type to help diagnose
        var pointerType = e.Pointer.Type;
        EventLog.Text = $"Pressed ({pointerType}) at ({_prevPoint.X:F0}, {_prevPoint.Y:F0})";

        e.Handled = true;
    }

    private void OnCanvasPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDrawing)
            return;

        var currentPoint = e.GetPosition(DrawingCanvas);

        // Skip sub-pixel jitter (tablet pen workaround from Scribble)
        if (System.Math.Abs(currentPoint.X - _prevPoint.X) < 0.5 &&
            System.Math.Abs(currentPoint.Y - _prevPoint.Y) < 0.5)
            return;

        _eventCount++;

        // Draw a line segment
        var line = new Line
        {
            StartPoint = _prevPoint,
            EndPoint = currentPoint,
            Stroke = Brushes.Black,
            StrokeThickness = 3,
            StrokeLineCap = PenLineCap.Round
        };
        DrawingCanvas.Children.Add(line);
        _currentStroke.Add(line);

        _prevPoint = currentPoint;

        // Show scroll offset to prove panning is happening
        var offset = CanvasScrollViewer.Offset;
        EventLog.Text = $"Drawing... points={_eventCount} scroll=({offset.X:F0},{offset.Y:F0})";

        e.Handled = true;
    }

    private void OnCanvasPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDrawing)
            return;

        _isDrawing = false;
        _prevPoint = new Point(-1, -1);

        var offset = CanvasScrollViewer.Offset;
        EventLog.Text = $"Released. {_eventCount} points drawn. Final scroll=({offset.X:F0},{offset.Y:F0})";

        e.Handled = true;
    }
}
