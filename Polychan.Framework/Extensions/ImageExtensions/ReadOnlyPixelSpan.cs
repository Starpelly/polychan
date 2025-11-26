using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Buffers;

namespace Polychan.GUI.Framework.Extensions.ImageExtensions;

public readonly ref struct ReadOnlyPixelSpan<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
{
    /// <summary>
    /// The span of pixels.
    /// </summary>
    public readonly ReadOnlySpan<TPixel> Span;

    private readonly IMemoryOwner<TPixel>? owner;

    internal ReadOnlyPixelSpan(Image<TPixel> image)
    {
        if (image.DangerousTryGetSinglePixelMemory(out var memory))
        {
            owner = null;
            Span = memory.Span;
        }
        else
        {
            owner = image.CreateContiguousMemory();
            Span = owner.Memory.Span;
        }
    }

    public void Dispose()
    {
        owner?.Dispose();
    }
}