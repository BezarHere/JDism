
class ByteSource
{
  public ArraySegment<byte> Content { get; init; }
  public Endianness Endianness { get; init; } = Endianness.Big;
  public int Position { get; private set; } = 0;

  public bool Good => Position >= 0 && Position < Content.Count;
  public bool Depleted => Position >= Content.Count;

  public byte this[int index]
  {
    get => Content[Position + index];
  }

  public ByteSource()
  {
    Content = [];
  }

  public ByteSource(ReadOnlySpan<byte> source, Endianness endianness = Endianness.Big)
  {
    Content = source.ToArray();
    Endianness = endianness;
  }

  public ByteSource(byte[] source, Endianness endianness = Endianness.Big)
  {
    Content = source;
    Endianness = endianness;
  }

  public void Seek(int pos, SeekOrigin origin = SeekOrigin.Begin)
  {
    switch (origin)
    {
      case SeekOrigin.Begin:
        Position = pos;
        return;
      case SeekOrigin.Current:
        Position += pos;
        return;
      case SeekOrigin.End:
        Position = Content.Count - pos;
        return;
    }
  }

  public void Seek(Index index)
  {
    Position = Translate(index);
  }

  public ByteSource Slice(Range range)
  {
    (int start, int end) = Translate(range);
    return Slice(start, end - start);
  }

  public ByteSource Slice(int start, int length)
  {
    return new(Content.Slice(start, length), Endianness);
  }

  public ReadOnlySpan<byte> Get(int length)
  {
    Position += length;
    return Content.Slice(Position - length, length);
  }

  public byte GetByte()
  {
    return Content[Position++];
  }

  public sbyte GetSByte()
  {
    return (sbyte)Content[Position++];
  }

  public short GetShort()
  {
    Position += 2;
    if (Endianness == Endianness.Big)
      return ByteConverter.ToShort_Big(Content, Position - 2);
    return ByteConverter.ToShort_Little(Content, Position - 2);
  }

  public ushort GetUShort()
  {
    Position += 2;
    if (Endianness == Endianness.Big)
      return ByteConverter.ToUshort_Big(Content, Position - 2);
    return ByteConverter.ToUshort_Big(Content, Position - 2);
  }

  public int GetInt()
  {
    Position += 4;
    if (Endianness == Endianness.Big)
      return ByteConverter.ToInt_Big(Content, Position - 4);
    return ByteConverter.ToInt_Little(Content, Position - 4);
  }

  public uint GetUInt()
  {
    Position += 4;
    if (Endianness == Endianness.Big)
      return ByteConverter.ToUint_Big(Content, Position - 4);
    return ByteConverter.ToUint_Big(Content, Position - 4);
  }

  public long GetLong()
  {
    Position += 8;
    if (Endianness == Endianness.Big)
      return ByteConverter.ToLong_Big(Content, Position - 8);
    return ByteConverter.ToLong_Little(Content, Position - 8);
  }

  public ulong GetULong()
  {
    Position += 8;
    if (Endianness == Endianness.Big)
      return ByteConverter.ToUlong_Big(Content, Position - 8);
    return ByteConverter.ToUlong_Big(Content, Position - 8);
  }


  private int Translate(Index index)
  {
    return index.IsFromEnd ? Content.Count - index.Value : index.Value;
  }

  private (int, int) Translate(Range range)
  {
    return (Translate(range.Start), Translate(range.End));
  }
}

