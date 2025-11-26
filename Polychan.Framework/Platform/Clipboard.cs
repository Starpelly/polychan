using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace Polychan.GUI.Framework.Platform
{
    /// <summary>
    /// This class allows placing and retrieving data from the clipboard
    /// </summary>
    public abstract class Clipboard
    {
        /// <summary>
        /// Retrieve text from the clipboard.
        /// </summary>
        public abstract string? GetText();

        /// <summary>
        /// Copy text to the clipboard.
        /// </summary>
        /// <param name="text">Text to copy to the clipboard</param>
        public abstract void SetText(string text);

        /// <summary>
        /// Retrieve an image from the clipboard.
        /// </summary>
        public abstract Image<TPixel>? GetImage<TPixel>()
            where TPixel : unmanaged, IPixel<TPixel>;

        /// <summary>
        /// Copy the image to the clipboard.
        /// </summary>
        /// <param name="image">The image to copy to the clipboard</param>
        /// <returns>Whether the image was successfully copied or not</returns>
        public abstract bool SetImage(Image image);
    }
}
