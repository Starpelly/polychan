using System.Drawing;
using System.Numerics;
using Polychan.GUI.Input;

namespace Polychan.GUI.Framework.Platform
{
    [Flags]
    public enum WindowFlags
    {
        None = 0,
        Popup = 1 << 0,
        Tool = 1 << 1,
        Dialog = 1 << 2,
        Modal = 1 << 3,
        TopMost = 1 << 4,
        SysMenu = 1 << 5,
        Resizable = 1 << 6,
        // ReSharper disable once InconsistentNaming
        OpenGL = 1 << 7,
    }

    public interface IWindow : IDisposable
    {
        /// <summary>
        /// Creates the concrete window implementation.
        /// </summary>
        void Create(IWindow? parent, WindowFlags flags);

        /// <summary>
        /// Forcefully closes the window.
        /// </summary>
        void Close();

        /// <summary>
        /// Attempts to raise the window, bringing it above other windows and requesting input focus.
        /// </summary>
        void Raise();

        /// <summary>
        /// Attempts to show the window, making it visible.
        /// </summary>
        void Show();

        /// <summary>
        /// Attempts to hide the window, making it invisible and hidden from the taskbar.
        /// </summary>
        void Hide();

        /// <summary>
        /// Sets the window icon by copying it from another window.
        /// </summary>
        /// <param name="window"></param>
        void CopyIconFromWindow(IWindow window);

        /// <summary>
        /// Sets the window icon to the provided <paramref name="imageStream"/>.
        /// </summary>
        void SetIconFromStream(Stream imageStream);

        /// <summary>
        /// Whether this <see cref="IWindow"/> is active (in the foreground).
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Invoked when the window close (X) button or another platform-native exit action has been pressed.
        /// </summary>
        event Action? ExitRequested;

        /// <summary>
        /// Invoked when the <see cref="IWindow"/> has closed.
        /// </summary>
        event Action? Exited;

        /// <summary>
        /// Invoked when the <see cref="IWindow"/> client size has changed.
        /// </summary>
        event Action? Resized;

        /// <summary>
        /// Invoked when the user moves the mouse cursor within the window.
        /// </summary>
        event Action<Vector2> MouseMove;

        /// <summary>
        /// Invoked when the user presses a mouse button.
        /// </summary>
        event Action<Vector2, MouseButton> MouseDown;

        /// <summary>
        /// Invoked when the user releases a mouse button.
        /// </summary>
        event Action<Vector2, MouseButton> MouseUp;

        /// <summary>
        /// Invoked when the mouse cursor enters the window.
        /// </summary>
        public event Action? MouseEntered;

        /// <summary>
        /// Invoked when the mouse cursor leaves the window.
        /// </summary>
        public event Action? MouseLeft;

        /// <summary>
        /// Invoked when the user scrolls the mouse wheel over the window.
        /// </summary>
        /// <remarks>
        /// Delta is positive when mouse wheel scrolled to the up or left, in non-"natural" scroll mode (ie. the classic way).
        /// </remarks>
        public event Action<Vector2, Vector2, bool> MouseWheel;

        /// <summary>
        /// Controls the state of the window.
        /// </summary>
        WindowState WindowState { get; set; }

        /// <summary>
        /// Invoked when <see cref="WindowState"/> changes.
        /// </summary>
        event Action<WindowState> WindowStateChanged;

        /// <summary>
        /// Invoked when the window moves.
        /// </summary>
        public event Action<Point> Moved;

        /// <summary>
        /// The client size of the window in pixels (excluding any window decoration/border).
        /// </summary>
        Size ClientSize { get; }

        /// <summary>
        /// The position of the window.
        /// </summary>
        Point Position { get; set; }

        /// <summary>
        /// The size of the window in scaled pixels (excluding any window decoration/border).
        /// </summary>
        Size Size { get; set; }

        /// <summary>
        /// The ratio of <see cref="ClientSize"/> and <see cref="Size"/>.
        /// </summary>
        float Scale { get; }

        /// <summary>
        /// The minimum size of the window.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when setting a negative size, or a size greater than <see cref="MaxSize"/>.</exception>
        Size MinSize { get; set; }

        /// <summary>
        /// The maximum size of the window.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when setting a negative or zero size, or a size less than <see cref="MinSize"/>.</exception>
        Size MaxSize { get; set; }

        /// <summary>
        /// Gets or sets whether the window is user-resizable.
        /// </summary>
        bool Resizable { get; set; }

        /// <summary>
        /// The window title.
        /// </summary>
        string Title { get; set; }
    }
}
