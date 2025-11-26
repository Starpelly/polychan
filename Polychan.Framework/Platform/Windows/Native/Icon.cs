using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Polychan.GUI.Framework.Platform.Windows.Native;

[SupportedOSPlatform("windows")]
internal class Icon : IDisposable
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(nint hIcon);

    private bool m_disposed = false;

    public nint Handle { get; private set; }

    public readonly int Width;
    public readonly int Height;

    internal Icon(nint handle, int width, int height)
    {
        Handle = handle;
        Width = width;
        Height = height;
    }

    ~Icon()
    {
        Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (m_disposed)
            return;

        if (Handle != nint.Zero)
        {
            DestroyIcon(Handle);
            Handle = nint.Zero;
        }

        m_disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}