using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;

namespace Polychan.GUI;

internal static class LayoutQueue
{
    // private static readonly HashSet<DirtyWidget> s_dirtyWidgets = [];


    // @NOTE
    // I used to use ConcurrentDictionary, but that was causing problems because sorting wasn't guaranteed anymore.
    // Maybe I should just make it so you can't update widgets in other threads...
    // And I'll throw an exception or whatever.
    private static readonly Dictionary<Guid, DirtyWidget> s_dirtyWidgets = [];

    public static bool IsFlushing { get; private set; } = false;

    private const bool LogChanges = false;

    private struct DirtyWidget
    {
        public LayoutFlushType FlushType;
        public Widget Widget;

        public DirtyWidget(LayoutFlushType flushType, Widget widget)
        {
            FlushType = flushType;
            Widget = widget;
        }

        public readonly override bool Equals(object? obj)
        {
            if (obj is DirtyWidget other)
            {
                return FlushType == other.FlushType && Widget == other.Widget;
            }
            return false;
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(FlushType, Widget);
        }
    }

    public static void Enqueue(Widget widget, LayoutFlushType flushType)
    {
        // Why would this be the case? Idk...
        if (widget == null)
            throw new Exception("huh?");
        if (widget.Layout == null)
            return;

        if (s_dirtyWidgets.TryAdd(widget.Guid, new(flushType, widget)))
        {
            if (LogChanges)
            {
                Console.WriteLine($"Enqued: {widget.Name}, frame: {Application.CurrentFrame}");
            }
        }
    }

    public static void Flush()
    {
        bool hadWorkAll = s_dirtyWidgets.Count > 0;
        if (hadWorkAll && LogChanges)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("========================Started flush!========================");
            Console.ResetColor();
        }

        IsFlushing = true;
        while (true)
        {
            if (!doOneFlush())
                break;
        }
        IsFlushing = false;

        if (hadWorkAll && LogChanges)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=========================Ended flush!=========================");
            Console.ResetColor();
        }
    }

    private static bool doOneFlush()
    {
        if (s_dirtyWidgets.Count == 0)
            return false;

        bool hadWork = s_dirtyWidgets.Count > 0;
        if (hadWork && LogChanges)
            Console.WriteLine("------------------Layout Start------------------");
        
        var work = s_dirtyWidgets.Values.ToList();
        s_dirtyWidgets.Clear();
        
        foreach (var dirty in work)
        {
            // Sometimes this can be null? I don't know how or why
            // but I guess we'll handle it in that case???
            dirty.Widget?.PerformLayoutUpdate(dirty.FlushType);
        }

        if (hadWork && LogChanges)
            Console.WriteLine("------------------Layout End------------------");

        return true;
    }
}