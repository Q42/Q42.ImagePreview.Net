using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;


namespace Q42.ImagePreview
{

  public class ImageConverter
  {
    public PreviewImage? GetPreviewImage(Image originalImage, int width, int height)
    {
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

          var x = 0;
          var y = 0;
          var w = 0;
          var h = 0;
          if (originalImage.Width > originalImage.Height)
          {
            x = (originalImage.Width - originalImage.Height) / 2;
            w = h = originalImage.Height;
          }
          else if (originalImage.Height > originalImage.Width)
          {
            y = (originalImage.Height - originalImage.Width) / 2;
            w = h = originalImage.Width;
          }

          graphics.DrawImage(originalImage, destinationRect, x, y, w, h, GraphicsUnit.Pixel, wrapMode);
        }
      }

      using (var memStream = new System.IO.MemoryStream())
      {
        var encoder = ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
        var encodingParams = new EncoderParameters(1);
        encodingParams.Param[0] = new EncoderParameter(Encoder.Quality, 50L);

        destinationImage.Save(memStream, encoder, encodingParams);

        return PreviewImage.FromBytes(memStream.ToArray());
      }

    }


  }
}
