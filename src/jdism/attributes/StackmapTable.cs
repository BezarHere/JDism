using Util;

namespace JDism.Attributes;

public record VerificationTypeRecord(byte Tag, ushort Value)
{

  public int ReadLength => DoTagRequireValue(Tag) ? 3 : 1;

  public static bool DoTagRequireValue(byte tag) => tag == 7 || tag == 8;

  public static VerificationTypeRecord Parse(byte[] data, int index)
  {
    byte tag = data[index];
    index++;

    ushort value = 0;
    if (DoTagRequireValue(tag))
    {
      value = ByteConverter.ToUshort_Big(data, index);
      index += 2;
    }

    return new(tag, value);
  }

  public static IEnumerable<VerificationTypeRecord> ParseAll(byte[] data, int index, int count)
  {
    for (int i = 0; i < count; i++)
    {
      var result = Parse(data, index);
      yield return result;
      index += result.ReadLength;
    }
  }
}

public readonly struct StackMapFrame(byte tag, ushort offset_delta,
                            IEnumerable<VerificationTypeRecord> locals,
                            IEnumerable<VerificationTypeRecord> stack)
{
  public readonly byte Tag => tag;
  public readonly ushort OffsetDelta => offset_delta;
  public readonly VerificationTypeRecord[] Locals = [.. locals];
  public readonly VerificationTypeRecord[] Stack = [.. stack];

  public static StackMapFrame Parse(byte[] data, ref int index)
  {
    byte tag = data[index];
    index++;


    foreach (var specifier in sSpecifiers)
    {
      if (!specifier.TagRange.Contains(tag))
      {
        continue;
      }

      ushort offset_delta = 0;
      if (specifier.HasOffsetDelta)
      {
        offset_delta = ByteConverter.ToUshort_Big(data, index);
        index += 2;
      }

      var locals = ParseVTRs(data, ref index, specifier.LocalsCount);
      var stack = ParseVTRs(data, ref index, specifier.StackCount);
      return new(
        tag, offset_delta,
        locals,
        stack
      );
    }

    return default;
  }

  private static VerificationTypeRecord[] ParseVTRs(byte[] data, ref int index, int count)
  {
    if (count == -1)
    {
      count = ByteConverter.ToUshort_Big(data, index);
      index += 2;
    }

    var locals = VerificationTypeRecord.ParseAll(
      data, index, count
    ).ToArray();

    index += locals.Aggregate(0, (acc, vtr) => acc + vtr.ReadLength);
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
public class StackMapTableJAttribute(IEnumerable<StackMapFrame> frames) : JAttribute
{
  public StackMapFrame[] Frames = [.. frames];
}