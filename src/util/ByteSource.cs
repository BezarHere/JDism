

ref struct ByteSource
{
  public readonly ReadOnlySpan<byte> Source { get; init; }
  public readonly Endianness Endianness { get; init; } = Endianness.Big;
  public int Position { get; private set; } = 0;

  public readonly bool Good => Position >= 0 && Position < Source.Length;

  public readonly byte this[int index] {
    get => Source[Position + index];
  }

  public ByteSource()
  {
    Source = [];
  }

  public ByteSource(ReadOnlySpan<byte> source, Endianness endianness = Endianness.Big)
  {
    Source = source;
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
        Position = Source.Length - pos;
        return;
    }
  }

  public void Seek(Index index)
  {
    Position = Translate(index);
  }

  public readonly ReadOnlySpan<byte> Get(Range range)
  {
    (int start, int end) = Translate(range);
    return Get(start, end - start);
  }

  public readonly ReadOnlySpan<byte> Get(int start, int length)
  {
    return Source.Slice(start, length);
  }

  public byte GetByte()
  {
    return Source[Position++];
  }

  public sbyte GetSByte()
  {
    return (sbyte)Source[Position++];
  }

  public short GetShort()
  {
    Position += 2;
    if (Endianness == Endianness.Big)
      return ByteConverter.ToShort_Big(Source, Position - 2);
    return ByteConverter.ToShort_Little(Source, Position - 2);
  }

  public ushort GetUShort()
  {
    Position += 2;
    if (Endianness == Endianness.Big)
      return ByteConverter.ToUshort_Big(Source, Position - 2);
    return ByteConverter.ToUshort_Big(Source, Position - 2);
  }

  public int GetInt()
  {
    Position += 4;
    if (Endianness == Endianness.Big)
      return ByteConverter.ToInt_Big(Source, Position - 4);
    return ByteConverter.ToInt_Little(Source, Position - 4);
  }

  public uint GetUInt()
  {
    Position += 4;
    if (Endianness == Endianness.Big)
      return ByteConverter.ToUint_Big(Source, Position - 4);
    return ByteConverter.ToUint_Big(Source, Position - 4);
  }

  public long GetLong()
  {
    Position += 8;
    if (Endianness == Endianness.Big)
      return ByteConverter.ToLong_Big(Source, Position - 8);
    return ByteConverter.ToLong_Little(Source, Position - 8);
  }

  public ulong GetULong()
  {
    Position += 8;
    if (Endianness == Endianness.Big)
      return ByteConverter.ToUlong_Big(Source, Position - 8);
    return ByteConverter.ToUlong_Big(Source, Position - 8);
  }


  private readonly int Translate(Index index)
  {
    return index.IsFromEnd ? Source.Length - index.Value : index.Value;
  }

  private readonly (int, int) Translate(Range range)
  {
    return (Translate(range.Start), Translate(range.End));
  }
}

