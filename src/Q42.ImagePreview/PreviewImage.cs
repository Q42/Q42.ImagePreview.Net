using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Q42.ImagePreview
{
  public struct PreviewImage
  {
    public byte[] Header { get; set; }
    public byte[] Body { get; set; }

    public static PreviewImage? FromBytes(byte[] image)
    {
      var pattern = new byte[] { 0xFF, 0xDA };
      var index = IndexOfPattern(image, pattern);

      if (!index.HasValue)
        return null;

      return new PreviewImage()
      {
        Header = image.Take(index.Value + 1).ToArray(),
        Body = image.Skip(index.Value).ToArray()
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
          return i + 2;
        }
      }

      return null;
    }
  }
}
