using Polychan.GUI.Widgets;
using SkiaSharp;
using System.Reflection;

namespace Polychan.GUI.Styles.Phantom;

public class PhantomStyle : Style
{
    private readonly PHSwatch m_swatch = new();

    #region Adjustments

    internal const float PushButton_Rounding = 3.0f;
    internal const bool Scrollbar_Shadows = true;
    internal const int Num_ShadowSteps = 3;

    #endregion

    public PhantomStyle()
    {
        m_swatch.LoadFromPalette(Application.Palette);
    }

    public static void FillRectEdges(SKCanvas canvas, SKRect rect, Edges edges, Margins margins, SKColor color)
    {
        using var paint = new SKPaint { Color = color, IsAntialias = false, Style = SKPaintStyle.Fill };

        if (edges.HasFlag(Edges.Left))
        {
            float ml = margins.Left;
            var r0 = new SKRect(rect.Left, rect.Top, rect.Left + ml, rect.Bottom);
            canvas.DrawRect(SKRect.Intersect(rect, r0), paint);
        }

        if (edges.HasFlag(Edges.Top))
        {
            float mt = margins.Top;
            var r1 = new SKRect(rect.Left, rect.Top, rect.Right, rect.Top + mt);
            canvas.DrawRect(SKRect.Intersect(rect, r1), paint);
        }

        if (edges.HasFlag(Edges.Right))
        {
            float mr = margins.Right;
            var r2 = new SKRect(rect.Right - mr, rect.Top, rect.Right, rect.Bottom);
            canvas.DrawRect(SKRect.Intersect(rect, r2), paint);
        }

        if (edges.HasFlag(Edges.Bottom))
        {
            float mb = margins.Bottom;
            var r3 = new SKRect(rect.Left, rect.Bottom - mb, rect.Right, rect.Bottom);
            canvas.DrawRect(SKRect.Intersect(rect, r3), paint);
        }
    }

    public static void FillRectOutline(SKCanvas canvas, SKRect rect, Margins margins, SKColor color)
    {
        using var paint = new SKPaint { Color = color, IsAntialias = false, Style = SKPaintStyle.Fill };

        float x = rect.Left;
        float y = rect.Top;
        float w = rect.Width;
        float h = rect.Height;

        float ml = margins.Left;
        float mt = margins.Top;
        float mr = margins.Right;
        float mb = margins.Bottom;

        // Top
        var r0 = new SKRect(x, y, x + w, y + mt);
        canvas.DrawRect(SKRect.Intersect(rect, r0), paint);

        // Left
        var r1 = new SKRect(x, y + mt, x + ml, y + h - mb);
        canvas.DrawRect(SKRect.Intersect(rect, r1), paint);

        // Right
        var r2 = new SKRect(x + w - mr, y + mt, x + w, y + h - mb);
        canvas.DrawRect(SKRect.Intersect(rect, r2), paint);

        // Bottom
        var r3 = new SKRect(x, y + h - mb, x + w, y + h);
        canvas.DrawRect(SKRect.Intersect(rect, r3), paint);
    }

    public static void DrawArrow(SKCanvas canvas, SKRect rect, ArrowType direction, SKColor color)
    {
        const float ArrowBaseRatio = 0.70f;

        float irx = rect.Left;
        float iry = rect.Top;
        float irw = rect.Width;
        float irh = rect.Height;

        if (irw < 1f || irh < 1f)
            return;

        float dw, dh;
        if (direction == ArrowType.Left || direction == ArrowType.Right)
        {
            dw = ArrowBaseRatio;
            dh = 1f;
        }
        else
        {
            dw = 1f;
            dh = ArrowBaseRatio;
        }

        // Maintain aspect ratio
        float scale = Math.Min(irw / dw, irh / dh);
        float aw = dw * scale;
        float ah = dh * scale;

        float ax = irx + (irw - aw) / 2f;
        float ay = iry + (irh - ah) / 2f;
        var arrowRect = new SKRect(ax, ay, ax + aw, ay + ah);

        SKPoint[] points = new SKPoint[3];

        switch (direction)
        {
            case ArrowType.Down:
                arrowRect.Top = (float)Math.Round(arrowRect.Top);
                points[0] = new SKPoint(arrowRect.Left, arrowRect.Top);
                points[1] = new SKPoint(arrowRect.Right, arrowRect.Top);
                points[2] = new SKPoint(arrowRect.MidX, arrowRect.Bottom);
                break;

            case ArrowType.Right:
                arrowRect.Left = (float)Math.Round(arrowRect.Left);
                points[0] = new SKPoint(arrowRect.Left, arrowRect.Top);
                points[1] = new SKPoint(arrowRect.Left, arrowRect.Bottom);
                points[2] = new SKPoint(arrowRect.Right, arrowRect.MidY);
                break;

            case ArrowType.Left:
                arrowRect.Right = (float)Math.Round(arrowRect.Right);
                points[0] = new SKPoint(arrowRect.Right, arrowRect.Top);
                points[1] = new SKPoint(arrowRect.Right, arrowRect.Bottom);
                points[2] = new SKPoint(arrowRect.Left, arrowRect.MidY);
                break;

            case ArrowType.Up:
            default:
                arrowRect.Bottom = (float)Math.Round(arrowRect.Bottom);
                points[0] = new SKPoint(arrowRect.Left, arrowRect.Bottom);
                points[1] = new SKPoint(arrowRect.Right, arrowRect.Bottom);
                points[2] = new SKPoint(arrowRect.MidX, arrowRect.Top);
                break;
        }

        using var paint = new SKPaint
        {
            Color = color,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        using var path = new SKPath();
        path.MoveTo(points[0]);
        path.LineTo(points[1]);
        path.LineTo(points[2]);
        path.Close();

        canvas.DrawPath(path, paint);
    }

    public override void DrawPushButton(SKCanvas canvas, PushButton button, StyleOptionButton option)
    {
        using var paint = new SKPaint();

        var isDefault = false;
        var isOn = option.State.HasFlag(StateFlag.On);
        var isHovering = option.State.HasFlag(StateFlag.MouseOver);
        var isDown = option.State.HasFlag(StateFlag.Sunken);
        var hasFocus = option.State.HasFlag(StateFlag.HasFocus);
        var isEnabled = option.State.HasFlag(StateFlag.Enabled);

        var outline = SwatchColor.Window_Outline;
        var fill = SwatchColor.Button;
        var specular = SwatchColor.Button_Specular;

        // Paint background
        {
            if (isDown || !isEnabled || isHovering)
            {
                fill = SwatchColor.Button_Pressed;
                specular = SwatchColor.Button_Pressed_Specular;
            }
            else if (isOn)
            {
                fill = SwatchColor.ScrollbarGutter;
                specular = SwatchColor.Button_Pressed_Specular;
            }
            if (hasFocus || isDefault)
            {
                outline = SwatchColor.Highlight_Outline;
            }

            paint.IsAntialias = true;

            // Fill
            paint.Color = m_swatch.GetColor(fill);
            canvas.DrawRoundRect(new SKRect(0, 0, button.Width, button.Height), new SKSize(PushButton_Rounding * 2, PushButton_Rounding * 2), paint);

            // Stroke
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1.0f;
            paint.Color = m_swatch.GetColor(outline);

            var inset = paint.StrokeWidth / 2.0f;
            canvas.DrawRoundRect(new SKRect(inset, inset, button.Width - inset, button.Height - inset), new SKSize(PushButton_Rounding, PushButton_Rounding), paint);

            // Specular
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1.0f;
            paint.Color = m_swatch.GetColor(specular);

            inset += 1;
            canvas.DrawRoundRect(new SKRect(inset, inset, button.Width - inset, button.Height - inset), new SKSize(PushButton_Rounding, PushButton_Rounding), paint);
        }

        // Paint label
        {
            paint.Reset();
            paint.Color = m_swatch.GetColor(isEnabled ? SwatchColor.Text : SwatchColor.WindowText_Disabled);

            var metrics = Application.DefaultFont.Metrics;
            var textHeight = (-metrics.Ascent) + metrics.Descent;
            Application.DefaultFont.MeasureText(option.Text, out var bounds);

            var labelX = (button.Width - bounds.Width) / 2;
            var labelY = (button.Height / 2) - (textHeight / 2) - metrics.Ascent;

            if (isDown)
            {
                labelY += 1;
            }

            canvas.DrawText(option.Text, new SKPoint(labelX, labelY), Application.DefaultFont, paint);
        }
    }

    public override void DrawPanelButton(SKCanvas canvas, PanelButton button, StyleOptionPanelButton option)
    {
        using var paint = new SKPaint();

        var isDefault = false;
        var isOn = option.State.HasFlag(StateFlag.On);
        var isHovering = option.State.HasFlag(StateFlag.MouseOver);
        var isDown = option.State.HasFlag(StateFlag.Sunken);
        var hasFocus = option.State.HasFlag(StateFlag.HasFocus);
        var isEnabled = option.State.HasFlag(StateFlag.Enabled);

        var outline = SwatchColor.Window_Outline;
        var fill = SwatchColor.Button;
        var specular = SwatchColor.Button_Specular;

        // Paint background
        if (isHovering || isDown)
        {
            if (isDown || !isEnabled || isHovering)
            {
                fill = SwatchColor.Button_Pressed;
                specular = SwatchColor.Button_Pressed_Specular;
            }
            else if (isOn)
            {
                fill = SwatchColor.ScrollbarGutter;
                specular = SwatchColor.Button_Pressed_Specular;
            }
            if (hasFocus || isDefault)
            {
                outline = SwatchColor.Highlight_Outline;
            }

            paint.IsAntialias = true;

            // Fill
            paint.Color = m_swatch.GetColor(fill);
            canvas.DrawRoundRect(new SKRect(0, 0, button.Width, button.Height), new SKSize(PushButton_Rounding * 2, PushButton_Rounding * 2), paint);

            // Stroke
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1.0f;
            paint.Color = m_swatch.GetColor(outline);

            var inset = paint.StrokeWidth / 2.0f;
            canvas.DrawRoundRect(new SKRect(inset, inset, button.Width - inset, button.Height - inset), new SKSize(PushButton_Rounding, PushButton_Rounding), paint);

            // Specular
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1.0f;
            paint.Color = m_swatch.GetColor(specular);

            inset += 1;
            canvas.DrawRoundRect(new SKRect(inset, inset, button.Width - inset, button.Height - inset), new SKSize(PushButton_Rounding, PushButton_Rounding), paint);
        }

        canvas.Save();
        
        if (isDown)
        {
            canvas.Translate(1, 1);
        }
        
        // Paint icon
        if (option.Icon != null)
        {
            var icon = option.Icon;
            
            var metrics = Application.FontIconBig.Metrics;
            var textHeight = (-metrics.Ascent) + metrics.Descent;
            
            paint.Reset();
            paint.Color = m_swatch.GetColor(isEnabled ? SwatchColor.Text : SwatchColor.WindowText_Disabled);
            
            canvas.DrawText(icon, button.Width * 0.5f, textHeight + 2, SKTextAlign.Center, Application.FontIconBig, paint);
        }

        // Paint label
        {
            paint.Reset();
            paint.Color = m_swatch.GetColor(isEnabled ? SwatchColor.Text : SwatchColor.WindowText_Disabled);

            var metrics = Application.DefaultFont.Metrics;
            var textHeight = (-metrics.Ascent) + metrics.Descent;
            Application.DefaultFont.MeasureText(option.Text, out var bounds);

            var labelX = (button.Width - bounds.Width) / 2;
            var labelY = button.Height - 6;

            canvas.DrawText(option.Text, new SKPoint(labelX, labelY), Application.DefaultFont, paint);
        }
        
        canvas.Restore();
    }
    
    public override void DrawScrollBar(SKCanvas canvas, ScrollBar scrollBar, StyleOptionScrollBar option)
    {
        // Rects
        var scrollBarSubLine = option.SubControlRects[ScrollBar.SubControl.SubLine];
        var scrollBarAddLine = option.SubControlRects[ScrollBar.SubControl.AddLine];
        var scrollBarSlider = option.SubControlRects[ScrollBar.SubControl.Slider];
        var scrollBarGroove = option.SubControlRects[ScrollBar.SubControl.Groove];

        bool isHorizontal = scrollBar.Orientation == ScrollBar.ScrollOrientation.Horizontal;
        bool isLeftToRight = true;

        bool isSunken = option.State.HasFlag(StateFlag.Sunken);
        bool isEnabled = option.State.HasFlag(StateFlag.Enabled);
        bool hasRange = scrollBar.Minimum != scrollBar.Maximum;

        bool scrollBarGrooveShown = true;

        using var paint = new SKPaint();

        // Groove/gutter/trench area
        if (scrollBarGrooveShown)
        {
            var r = scrollBarGroove;
            Edges edges;
            if (isHorizontal)
            {
                edges = Edges.Top;
                r = r.SetY(r.Top + 1);
            }
            else
            {
                if (isLeftToRight)
                {
                    edges = Edges.Left;
                    r.Left += 1;
                }
                else
                {
                    edges = Edges.Right;
                    r.Right -= 1;
                }
            }

            var grooveColor = isEnabled ? SwatchColor.ScrollbarGutter : SwatchColor.ScrollbarGutter_Disabled;

            // Top or left dark edge
            FillRectEdges(canvas, scrollBarGroove, edges, new(1), m_swatch.GetColor(SwatchColor.Window_Outline));

            // Ring shadow
            paint.IsStroke = true;

            // I don't know why, but if rendered using the GPU,
            // setting this to '1' fucks up rendering strokes...?
            // Is this a bug with SkiaSharp? Idk, I should probably report.
            // For right now, just don't do this...!
            // paint.StrokeWidth = 1;
            
            if (Scrollbar_Shadows && isEnabled)
            {
                for (int i = 0; i < Num_ShadowSteps; ++i)
                {
                    paint.Color = m_swatch.ScrollbarShadowColors[i];
                    canvas.DrawRect(new SKRectI(r.Left, r.Top, r.Right - 1, r.Bottom - 1), paint);
                    r = r.Adjusted(1, 1, -1, -1);
                }
            }

            // General BG fill
            paint.IsStroke = false;
            paint.Color = m_swatch.GetColor(grooveColor);
            canvas.DrawRect(r, paint);
        }

        // Slider thumb
        {
            SwatchColor thumbFill, thumbSpecular;

            if (isSunken && (option.ActiveSubControls == ScrollBar.SubControl.Slider))
            {
                thumbFill = SwatchColor.Button_Pressed;
                thumbSpecular = SwatchColor.Button_Pressed_Specular;
            }
            else if (hasRange)
            {
                thumbFill = SwatchColor.Button;
                thumbSpecular = SwatchColor.Button_Specular;
            }
            else
            {
                thumbFill = SwatchColor.Window;
                thumbSpecular = SwatchColor.None;
            }

            Edges edges;
            var edgeRect = scrollBarSlider;
            var mainRect = scrollBarSlider;

            if (isHorizontal)
            {
                edges = Edges.Left | Edges.Top | Edges.Right;
                edgeRect = edgeRect.Adjusted(-1, 0, 1, 0);
                mainRect = mainRect.SetY(mainRect.Top + 1);
            }
            else
            {
                edgeRect = edgeRect.Adjusted(0, -1, 0, 1);
                if (isLeftToRight)
                {
                    edges = Edges.Left | Edges.Top | Edges.Bottom;
                    mainRect.Left += 1;
                }
                else
                {
                    edges = Edges.Top | Edges.Bottom | Edges.Right;
                    mainRect.Right -= 1;
                }
            }

            FillRectEdges(canvas, edgeRect, edges, new(1), m_swatch.GetColor(SwatchColor.Window_Outline));

            paint.Color = m_swatch.GetColor(thumbFill);
            canvas.DrawRect(mainRect, paint);

            // Thumb specular
            if (thumbSpecular != SwatchColor.None)
            {
                /*
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = 1.0f;
                paint.Color = m_swatch.GetColor(thumbSpecular);
                */

                FillRectOutline(canvas, mainRect, new(1), m_swatch.GetColor(thumbSpecular));
            }

            // What do you call these?
            // Idk, but they look cool!
            const int lineCount = 0;

            if (lineCount > 0)
            {
                var padding = 4;
                var spacing = 1;

                var linePxHeight = 2;
                var linesHeight = ((linePxHeight * lineCount) + (spacing * lineCount)) - spacing;

                var centerThumb = (mainRect.Bottom - mainRect.Top) / 2;
                var centerLines = linesHeight / 2;

                var linesY = mainRect.Top + (centerThumb - centerLines);
                var linesRect = new SKRectI(mainRect.Left + padding, linesY, mainRect.Right - padding, linesY + linesHeight);

                var lineY = centerThumb - centerLines;

                var c1 = m_swatch.GetColor(SwatchColor.Window_Outline);
                var c2 = m_swatch.GetColor(thumbSpecular);

                for (int i = 0; i < lineCount; i++)
                {
                    if (i > 0)
                    {
                        lineY += linePxHeight;
                        lineY += spacing;
                    }

                    var topy = mainRect.Top + lineY;

                    paint.Color = c1;
                    canvas.DrawLine(new SKPoint(linesRect.Left, topy), new SKPoint(linesRect.Right, topy), paint);

                    // paint.Color = c2;
                    // canvas.DrawLine(new SKPoint(linesRect.Left, topy + 1), new SKPoint(linesRect.Right, topy + 1), paint);
                }

                // paint.Color = SKColors.Red.WithAlpha(100);
                // canvas.DrawRect(linesRect, paint);
            }
        }

        // The SubLine (up/left) button
        {
            SwatchColor fill, specular;
            if (isSunken && (option.ActiveSubControls == ScrollBar.SubControl.SubLine))
            {
                fill = SwatchColor.Button_Pressed;
                specular = SwatchColor.Button_Pressed_Specular;
            }
            else if (hasRange)
            {
                fill = SwatchColor.Button;
                specular = SwatchColor.Button_Specular;
            }
            else
            {
                fill = SwatchColor.Window;
                specular = SwatchColor.None;
            }

            var btnRect = scrollBarSubLine;
            var bgRect = btnRect;
            Edges edges;
            if (isHorizontal)
            {
                edges = Edges.None;
            }
            else
            {
                if (isLeftToRight)
                {
                    edges = Edges.Left | Edges.Bottom;
                    bgRect = bgRect.Adjusted(1, 0, 0, -1);
                }
                else
                {
                    edges = Edges.Right | Edges.Bottom;
                    bgRect = bgRect.Adjusted(0, 0, -1, -1);
                }
            }

            // Outline, fill, specular
            FillRectEdges(canvas, btnRect, edges, new(1), m_swatch.GetColor(SwatchColor.Window_Outline));

            paint.Reset();
            paint.Color = m_swatch.GetColor(fill);
            canvas.DrawRect(bgRect, paint);

            if (specular != SwatchColor.None)
            {
                FillRectOutline(canvas, bgRect, new(1), m_swatch.GetColor(specular));
            }

            // Arrows
            ArrowType arrowType;
            if (isHorizontal)
            {
                arrowType = isLeftToRight ? ArrowType.Left : ArrowType.Right;
            }
            else
            {
                arrowType = ArrowType.Up;
            }
            int adj = Math.Min(bgRect.Width, bgRect.Height) / 4;
            DrawArrow(canvas, bgRect.Adjusted(adj, adj, -adj, -adj), arrowType, m_swatch.GetColor(hasRange ? SwatchColor.Indicator_Current : SwatchColor.Indicator_Disabled));
        }

        // The AddLine (down/right) button
        {
            SwatchColor fill, specular;
            if (isSunken && (option.ActiveSubControls == ScrollBar.SubControl.AddLine))
            {
                fill = SwatchColor.Button_Pressed;
                specular = SwatchColor.Button_Pressed_Specular;
            }
            else if (hasRange)
            {
                fill = SwatchColor.Button;
                specular = SwatchColor.Button_Specular;
            }
            else
            {
                fill = SwatchColor.Window;
                specular = SwatchColor.None;
            }
            var btnRect = scrollBarAddLine;
            var bgRect = btnRect;
            Edges edges;

            if (isLeftToRight)
            {
                edges = Edges.Left | Edges.Top;
                bgRect = bgRect.Adjusted(1, 1, 0, 0);
            }
            else
            {
                edges = Edges.Top | Edges.Right;
                bgRect = bgRect.Adjusted(0, 1, -1, 0);
            }

            // Outline, fill, specular
            FillRectEdges(canvas, btnRect, edges, new(1), m_swatch.GetColor(SwatchColor.Window_Outline));

            paint.Color = m_swatch.GetColor(fill);
            canvas.DrawRect(bgRect, paint);

            if (specular != SwatchColor.None)
            {
                FillRectOutline(canvas, bgRect, new(1), m_swatch.GetColor(specular));
            }

            // Arrows
            ArrowType arrowType;
            if (isHorizontal)
            {
                arrowType = isLeftToRight ? ArrowType.Right : ArrowType.Left;
            }
            else
            {
                arrowType = ArrowType.Down;
            }
            int adj = Math.Min(bgRect.Width, bgRect.Height) / 4;
            DrawArrow(canvas, bgRect.Adjusted(adj, adj, -adj, -adj), arrowType, m_swatch.GetColor(hasRange ? SwatchColor.Indicator_Current : SwatchColor.Indicator_Disabled));
        }
    }

    public override void DrawShapedFrame(SKCanvas canvas, ShapedFrame frame, StyleOptionShapedFrame option)
    {
        using var paint = new SKPaint();
        paint.IsStroke = true;
        // paint.StrokeWidth = 1;
        paint.Color = m_swatch.GetColor(SwatchColor.Window_Outline);

        var r = new SKRectI(0, 0, frame.Width, frame.Height);
        switch (frame.FrameShape)
        {
            case ShapedFrame.Shape.Box:
                canvas.DrawRect(new(0, 0, frame.Width - 1, frame.Height - 1), paint);
                break;
            case ShapedFrame.Shape.HLine:
                r = r.SetY(r.GetY() + r.Height / 2);
                r = r.SetHeight(1);
                canvas.DrawRect(r, paint);
                break;
            case ShapedFrame.Shape.VLine:
                r = r.SetX(r.GetX() + r.Width / 2);
                r = r.SetWidth(1);
                canvas.DrawRect(r, paint);
                break;
            default:
                throw new Exception("I'm lazy!");
        }
    }

    public override SKColor GetFrameColor()
    {
        return m_swatch.GetColor(SwatchColor.Window_Outline);
    }

    public override SKColor GetButtonHoverColor()
    {
        return m_swatch.GetColor(SwatchColor.Button_Pressed);
    }
}