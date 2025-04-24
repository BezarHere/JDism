using System.Net.Http.Headers;
using System.Text;

namespace JDism;

readonly struct Instruction(JVMOpCode op_code, IEnumerable<InstructionParameter> parameters, int address)
{
  public const string AddressFormat = "X4";

  public readonly JVMOpCode OpCode = op_code;
  public readonly InstructionParameter[] Parameters = [.. parameters];
  public readonly int Address = address;

  public readonly int Size => Parameters.Aggregate(1, (a, i) => a + i.Size);

  public Instruction()
    : this(JVMOpCode._Invalid, [], 0)
  {
  }

  public static JVMOpCode ReadOpCode(byte value)
  {
    byte byte_type = value;
    if (byte_type >= (byte)JVMOpCode._Max)
    {
      return JVMOpCode._Invalid;
    }

    return (JVMOpCode)byte_type;
  }

  public static JVMOpCode ReadOpCode(ByteSource reader)
  {
    return ReadOpCode(reader.GetByte());
  }

  public static Instruction Read(ByteSource source)
  {
    int start_address = source.Position;
    JVMOpCode type = ReadOpCode(source.GetByte());

    if (type == JVMOpCode._Invalid)
    {
      return new();
    }

    var parameters = InstructionParameter.Parse(
      type, source
    );

    return new Instruction(type, parameters, start_address);
  }

  public static IEnumerable<Instruction> ReadAll(ByteSource source)
  {
    while (!source.Depleted)
    {
      yield return Read(source);
    }
  }

  public string ToString(JContextView context = default)
  {
    if (OpCode >= JVMOpCode._Max)
    {
      return "????";
    }

    StringBuilder builder = new();
    builder.Append(OpCode.ToString());

    if (Parameters.Length > 0)
    {
      builder.Append(' ');

      var param_info = InstructionParameter.sParametersPreviews[OpCode];

      if (param_info is null)
      {
        param_info = [ ..from p in Parameters
          select new InstructionParameter.ParameterPreview(
            p.Size, InstructionParameter.PreviewType.Basic
          )
        ];
      }


      foreach ((int index, InstructionParameter p) in Parameters.Iterate())
      {
        if (index != 0)
        {
          builder.Append(", ");
        }
        builder.Append(p.ToString(param_info[index].Type, Address, context));
      }

    }

    return builder.ToString();
  }


  public static bool IsJumpOpCode(JVMOpCode op_code)
  {
    var p = InstructionParameter.sParametersPreviews[op_code];
    return p is not null && p.Length > 0 && p[0].Type == InstructionParameter.PreviewType.JumpOffset;
  }

  public static bool IsAlwaysJumpOpCode(JVMOpCode op_code)
  {
    return op_code is
      JVMOpCode.Goto or JVMOpCode.GotoW
      or JVMOpCode.Jsr or JVMOpCode.JsrW;
  }

}
