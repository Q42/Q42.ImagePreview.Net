using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.Remoting.Messaging;


namespace Q42.ImagePreview
{
  public static class ImagePreviewConverter
  {
    private static readonly byte[] SOF0Pattern = { 0xFF, 0xC0 };
    private static readonly byte[] SOSPattern = { 0xFF, 0xDA };
    private const int HeaderSizeLength = 4;

    private static byte[] header;
    public static byte[] Header
    {
      get
      {
        if (header != null)        
          return header;        

        var bitmap = new Bitmap(40, 40);
        var preview = CreatePreviewImage(bitmap);
        if (!preview.HasValue)        
          throw new ImagePreviewException("Something went wrong generating the header");

        return header = preview.Value.Header;
      }
    }

    public static string Base64ImageFromBody(byte[] body)
    {
      var indexC0 = IndexOfPattern(Header, SOF0Pattern);
      if (!indexC0.HasValue)
        return null;

      var headerSizeIndexStart = indexC0.Value + 5;   
        

      var bytes = new[]
      {
        Header.Take(headerSizeIndexStart).ToArray(),
        body.Take(HeaderSizeLength).ToArray(),
        Header.Skip(headerSizeIndexStart).Take(Header.Length - headerSizeIndexStart).ToArray(),
        body.Skip(HeaderSizeLength).Take(body.Length - HeaderSizeLength).ToArray()
      }.SelectMany(z => z).ToArray();
       
      return Convert.ToBase64String(bytes);
    }


    private static PreviewImage? PreviewImageFromBytes(byte[] image)
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
      
      return new PreviewImage()
      {
        Header = header,
        Body = body
      };      
    }

    public static PreviewImage? CreatePreviewImage(Image originalImage)
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
        var encodingParams = new EncoderParameters(1);
        encodingParams.Param[0] = new EncoderParameter(Encoder.Quality, 50L);

        destinationImage.Save(memStream, encoder, encodingParams);

        return PreviewImageFromBytes(memStream.ToArray());
      }

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
