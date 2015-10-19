using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;


namespace Q42.ImagePreview
{
  public static class ImagePreviewConverter
  {
    private static readonly byte[] SOF0Pattern = { 0xFF, 0xC0 };
    private static readonly byte[] SOSPattern = { 0xFF, 0xDA };
    private const int HeaderSizeLength = 4;

    private static byte[] _header;

    /// <summary>
    /// Public header shared by all the images generated with this converter.
    /// </summary>
    public static byte[] Header
    {
      get
      {
        if (_header != null)        
          return _header;        

        var bitmap = new Bitmap(40, 40);
        var preview = ImagePreviewFromImage(bitmap);
        if (!preview.HasValue)        
          throw new ImagePreviewException("Something went wrong generating the header");

        return _header = preview.Value.Header;
      }
    }

    public static string Base64Header
    {
      get { return Convert.ToBase64String(Header); }
    }

    /// <summary>
    /// Get the full preview image as byte[]
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public static byte[] ByteImageFromBody(byte[] body)
    {
      if (body == null)
        return null;

      var indexC0 = IndexOfPattern(Header, SOF0Pattern);
      if (!indexC0.HasValue)
        return null;

      var headerSizeIndexStart = indexC0.Value + 5;

      return new[]
      {
        Header.Take(headerSizeIndexStart).ToArray(),
        body.Take(HeaderSizeLength).ToArray(),
        Header.Skip(headerSizeIndexStart).Take(Header.Length - headerSizeIndexStart).ToArray(),
        body.Skip(HeaderSizeLength).Take(body.Length - HeaderSizeLength).ToArray()
      }.SelectMany(z => z).ToArray();      
    }

    /// <summary>
    /// Get the base64 encoded string of the full preview image.
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public static string Base64ImageFromBody(byte[] body)
    {
      if (body == null)
        return null;

      var byteImage = ByteImageFromBody(body);
      return byteImage == null ? null : Convert.ToBase64String(byteImage);
    }
    
    /// <summary>
    /// Creates a preview image
    /// </summary>
    /// <param name="originalImage"></param>
    /// <returns>The bytes that you need to save. Combine these bytes with the public header to create the full preview image.</returns>
    public static byte[] CreateImagePreview(Image originalImage)
    {
      var previewImage = ImagePreviewFromImage(originalImage);
      return previewImage.HasValue ? previewImage.Value.Body : null;

    }

    private static ImagePreview? ImagePreviewFromImage(Image originalImage)
    {
      const int maxSize = 40;

      var ratio = (double)originalImage.Width / originalImage.Height;
      var width = (int)Math.Min(maxSize, maxSize * ratio);
      var height = ratio >= 0 ? (int)Math.Min(maxSize, maxSize / ratio) : (int)Math.Min(maxSize, maxSize * ratio);

      var destinationRect = new Rectangle(0, 0, width, height);
      var destinationImage = new Bitmap(width, height);

      destinationImage.SetResolution(originalImage.HorizontalResolution, originalImage.VerticalResolution);

      using (var graphics = Graphics.FromImage(destinationImage))
      {
        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.CompositingQuality = CompositingQuality.HighSpeed;
        graphics.InterpolationMode = InterpolationMode.Low;
        graphics.SmoothingMode = SmoothingMode.None;
        graphics.PixelOffsetMode = PixelOffsetMode.None;

        using (var wrapMode = new ImageAttributes())
        {
          wrapMode.SetWrapMode(WrapMode.TileFlipXY);
          graphics.DrawImage(originalImage, destinationRect, 0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, wrapMode);
        }
      }

      using (var memStream = new System.IO.MemoryStream())
      {
        var encoder = ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
        if (encoder == null)
          throw new ImagePreviewException("Jpeg decoder not found");

        var encodingParams = new EncoderParameters(1);
        encodingParams.Param[0] = new EncoderParameter(Encoder.Quality, 50L);

        destinationImage.Save(memStream, encoder, encodingParams);

        return ImagePreviewFromBytes(memStream.ToArray());        
      }
    }


    private static ImagePreview? ImagePreviewFromBytes(byte[] image)
    {
      var indexC0 = IndexOfPattern(image, SOF0Pattern);
      if (!indexC0.HasValue)
        return null;

      var indexBodyStart = IndexOfPattern(image, SOSPattern);
      if (!indexBodyStart.HasValue)
        return null;

      var headerSizeIndexStart = indexC0.Value + 5;
      var headerSizeIndexEnd = headerSizeIndexStart + HeaderSizeLength;

      // copy the first part of the header
      var header = new[]
      {
        image.Take(headerSizeIndexStart).ToArray(),
        image.Skip(headerSizeIndexEnd).Take(indexBodyStart.Value - headerSizeIndexEnd).ToArray()
      }.SelectMany(arr => arr).ToArray();

      var body = new[]
      {
        image.Skip(headerSizeIndexStart).Take(HeaderSizeLength).ToArray(),
        image.Skip(indexBodyStart.Value).Take(image.Length - indexBodyStart.Value).ToArray()
      }.SelectMany(arr => arr).ToArray();

      return new ImagePreview()
      {
        Header = header,
        Body = body
      };
    }

    private static int? IndexOfPattern(IList<byte> bytes, IList<byte> pattern)
    {
      for (var i = 0; i < bytes.Count - 1; i++)
      {
        var current = bytes[i];
        var next = bytes[i + 1];

        if (current == pattern[0] && next == pattern[1])
        {
          return i;
        }
      }

      return null;
    }
  }
}
