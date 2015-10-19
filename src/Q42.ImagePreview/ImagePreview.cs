namespace Q42.ImagePreview
{
  internal struct ImagePreview
  {
    /// <summary>
    /// Public header
    /// </summary>
    public byte[] Header { get; set; }

    /// <summary>
    /// Image body (store this)
    /// </summary>
    public byte[] Body { get; set; }
  }
}