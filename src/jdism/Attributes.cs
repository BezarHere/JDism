using Util;

namespace JDism;


[RegisterJAttribute(JAttributeType.ConstantValue)]
public class ConstantInfoJAttribute(Constant constant, ushort index) : JAttribute
{
  public readonly Constant Constant = constant;
  public readonly ushort Index = index;


  public override string ToString()
  {
    return $"@Constant({Constant})";
  }
}

[RegisterJAttribute(JAttributeType.Signature)]
public class SignatureJAttribute(JType type, ushort index) : JAttribute
{
  public JType Signature = type;
  public ushort Index = index;

  public override string ToString()
  {
    return $"@Signature({Signature})";
  }
}

public record ExceptionRecord(ushort StartPc, ushort EndPc, ushort HandlerPc, ushort CatchType);

[RegisterJAttribute(JAttributeType.Code)]
class CodeInfoJAttribute : JAttribute
{

  public ushort MaxStack;
  public ushort MaxLocals;
  public Instruction[] Instructions;

  public ExceptionRecord[] ExceptionRecords;
  public JAttribute[] Attributes;

  public CodeInfoJAttribute(ushort max_stack, ushort max_locals,
                            Instruction[] instructions,
                            ExceptionRecord[] exceptions,
                            JAttribute[] attributes)
  {
    MaxStack = max_stack;
    MaxLocals = max_locals;
    Instructions = instructions;
    ExceptionRecords = exceptions;
    Attributes = attributes;
  }

  public override string ToString()
  {
    string instructions_str = string.Join(", ", Instructions);
    return $"@Code(locals={MaxLocals}, stack_size={MaxStack}, code=[{instructions_str}])";
  }

}

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

[RegisterJAttribute(JAttributeType.StackMapTable)]
public class StackMapTableJAttribute(IEnumerable<StackMapFrame> frames) : JAttribute
{
  public StackMapFrame[] Frames = [.. frames];

}

public class UnknownJAttribute(JAttributeType type, byte[] data) : JAttribute
{
  public JAttributeType Type => type;
  public byte[] Data => data;

  public override string ToString()
  {
    string data_str = string.Join(", ", from i in Data select i.ToString("X"));
    return $"@{Type}({data_str})";
  }
}

public class CustomJAttribute(byte[] data) : JAttribute
{
  public byte[] Data => data;

  public override string ToString()
  {
    string data_str = string.Join(", ", from i in Data select i.ToString("X"));
    return $"@CustomAttribute({data_str})";
  }
}


