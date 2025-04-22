using Util;

namespace JDism.Attributes;

record VerificationTypeRecord(byte Tag, ushort Value)
{

  public int ReadLength => DoTagRequireValue(Tag) ? 3 : 1;

  public static bool DoTagRequireValue(byte tag) => tag == 7 || tag == 8;

  public static VerificationTypeRecord Parse(ByteSource source)
  {
    byte tag = source.GetByte();

    ushort value = 0;
    if (DoTagRequireValue(tag))
    {
      value = source.GetUShort();
    }

    return new(tag, value);
  }

  public static IEnumerable<VerificationTypeRecord> ParseAll(ByteSource source, int count)
  {
    for (int i = 0; i < count; i++)
    {
      yield return Parse(source);
    }
  }
}

readonly struct StackMapFrame(byte tag, ushort offset_delta,
                            IEnumerable<VerificationTypeRecord> locals,
                            IEnumerable<VerificationTypeRecord> stack)
{
  public readonly byte Tag => tag;
  public readonly ushort OffsetDelta => offset_delta;
  public readonly VerificationTypeRecord[] Locals = [.. locals];
  public readonly VerificationTypeRecord[] Stack = [.. stack];

  public static StackMapFrame Parse(ByteSource source)
  {
    byte tag = source.GetByte();

    foreach (var specifier in sSpecifiers)
    {
      if (!specifier.TagRange.Contains(tag))
      {
        continue;
      }

      ushort offset_delta = 0;
      if (specifier.HasOffsetDelta)
      {
        offset_delta = source.GetUShort();
      }

      var locals = ParseVTRs(source, specifier.LocalsCount);
      var stack = ParseVTRs(source, specifier.StackCount);
      return new(
        tag, offset_delta,
        locals,
        stack
      );
    }

    return default;
  }

  private static VerificationTypeRecord[] ParseVTRs(ByteSource source, int count)
  {
    if (count == -1)
    {
      count = source.GetUShort();
    }

    var locals = VerificationTypeRecord.ParseAll(
       source, count
    ).ToArray();
    return locals;
  }

  private record LoadSpecifier(
    IndexRange TagRange,
    bool HasOffsetDelta = false,
    int LocalsCount = 0, // negative for dynamic count
    int StackCount = 0 // negative for dynamic count
  );

  private static readonly LoadSpecifier[] sSpecifiers = [
    new( 0..63 ),
    new( 64..127, false, 0, 1 ),
    new( 247..247, true, 0, 1 ),
    new( 248..250, true ),
    new( 251..251, true ),
    new( 252..252, true, 1 ),
    new( 253..253, true, 2 ),
    new( 254..254, true, 3 ),
    new( 255..255, true, -1, -1 ),
  ];

}

[Register(JAttributeType.StackMapTable)]
class StackMapTableJAttribute(IEnumerable<StackMapFrame> frames) : JAttribute
{
  public StackMapFrame[] Frames = [.. frames];
}