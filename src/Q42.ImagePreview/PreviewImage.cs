using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Q42.ImagePreview
{
  public struct PreviewImage
  {
    public byte[] Header { get; set; }
    public byte[] Body { get; set; }
  }
}