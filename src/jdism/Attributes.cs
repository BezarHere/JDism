using Util;

namespace JDism;


[Register(JAttributeType.ConstantValue)]
public class ConstantInfoJAttribute(Constant constant, ushort index) : JAttribute
{
  public readonly Constant Constant = constant;
  public readonly ushort Index = index;


  public override string ToString()
  {
    return $"@Constant({Constant})";
  }
}

[Register(JAttributeType.Signature)]
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

[Register(JAttributeType.Code)]
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



[Register(JAttributeType.Exceptions)]
public class ExceptionJAttribute(IEnumerable<ExceptionJAttribute.ExceptionNameInfo> infos)
  : JAttribute
{
  public record struct ExceptionNameInfo(string Name, ushort Index);
  public ExceptionNameInfo[] ExceptionNames = [.. infos];

  public override string ToString()
  {
    return $"@Throws({string.Join(", ", from i in ExceptionNames select i.Name)})";
  }
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


