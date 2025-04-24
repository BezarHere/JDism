using Util;

namespace JDism;


[Register(JVMAttributeType.ConstantValue)]
public class ConstantInfoAnnotation(Constant constant, ushort index) : JVMAttribute
{
  public readonly Constant Constant = constant;
  public readonly ushort Index = index;


  public override string ToString()
  {
    return $"@Constant({Constant})";
  }
}

[Register(JVMAttributeType.Signature)]
public class SignatureAnnotation(JType type, ushort index) : JVMAttribute
{
  public JType Signature = type;
  public ushort Index = index;

  public override string ToString()
  {
    return $"@Signature({Signature})";
  }
}

public record ExceptionRecord(ushort StartPc, ushort EndPc, ushort HandlerPc, ushort CatchType);

[Register(JVMAttributeType.Code)]
class CodeInfoAnnotation : JVMAttribute
{

  public ushort MaxStack;
  public ushort MaxLocals;
  public Instruction[] Instructions;

  public ExceptionRecord[] ExceptionRecords;
  public JVMAttribute[] Attributes;

  public CodeInfoAnnotation(ushort max_stack, ushort max_locals,
                            Instruction[] instructions,
                            ExceptionRecord[] exceptions,
                            JVMAttribute[] attributes)
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



[Register(JVMAttributeType.Exceptions)]
public class ExceptionAnnotation(IEnumerable<ExceptionAnnotation.ExceptionNameInfo> infos)
  : JVMAttribute
{
  public record struct ExceptionNameInfo(string Name, ushort Index);
  public ExceptionNameInfo[] ExceptionNames = [.. infos];

  public override string ToString()
  {
    return $"@Throws({string.Join(", ", from i in ExceptionNames select i.Name)})";
  }
}

public class UnknownAnnotation(JVMAttributeType type, byte[] data) : JVMAttribute
{
  public JVMAttributeType Type => type;
  public byte[] Data => data;

  public override string ToString()
  {
    string data_str = string.Join(", ", from i in Data select i.ToString("X"));
    return $"@{Type}({data_str})";
  }
}

public class CustomAnnotation(byte[] data) : JVMAttribute
{
  public byte[] Data => data;

  public override string ToString()
  {
    string data_str = string.Join(", ", from i in Data select i.ToString("X"));
    return $"@CustomAttribute({data_str})";
  }
}


