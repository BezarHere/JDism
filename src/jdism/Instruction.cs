

using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace JDism;

public enum VMOpCode : byte
{
  Nop = 0,
  AconstNull,
  IconstM1,
  Iconst0,
  Iconst1,
  Iconst2,
  Iconst3,
  Iconst4,
  Iconst5,
  Lconst0,
  Lconst1,
  Fconst0,
  Fconst1,
  Fconst2,
  Dconst0,
  Dconst1,
  Bipush,
  Sipush,
  Ldc,
  LdcW,
  Ldc2W,
  Iload,
  Lload,
  Fload,
  Dload,
  Aload,
  Iload0,
  Iload1,
  Iload2,
  Iload3,
  Lload0,
  Lload1,
  Lload2,
  Lload3,
  Fload0,
  Fload1,
  Fload2,
  Fload3,
  Dload0,
  Dload1,
  Dload2,
  Dload3,
  Aload0,
  Aload1,
  Aload2,
  Aload3,
  Iaload,
  Laload,
  Faload,
  Daload,
  Aaload,
  Baload,
  Caload,
  Saload,
  Istore,
  Lstore,
  Fstore,
  Dstore,
  Astore,
  Istore0,
  Istore1,
  Istore2,
  Istore3,
  Lstore0,
  Lstore1,
  Lstore2,
  Lstore3,
  Fstore0,
  Fstore1,
  Fstore2,
  Fstore3,
  Dstore0,
  Dstore1,
  Dstore2,
  Dstore3,
  Astore0,
  Astore1,
  Astore2,
  Astore3,
  Iastore,
  Lastore,
  Fastore,
  Dastore,
  Aastore,
  Bastore,
  Castore,
  Sastore,
  Pop,
  Pop2,
  Dup,
  DupX1,
  DupX2,
  Dup2,
  Dup2X1,
  Dup2X2,
  Swap,
  Iadd,
  Ladd,
  Fadd,
  Dadd,
  Isub,
  Lsub,
  Fsub,
  Dsub,
  Imul,
  Lmul,
  Fmul,
  Dmul,
  Idiv,
  Ldiv,
  Fdiv,
  Ddiv,
  Irem,
  Lrem,
  Frem,
  Drem,
  Ineg,
  Lneg,
  Fneg,
  Dneg,
  Ishl,
  Lshl,
  Ishr,
  Lshr,
  Iushr,
  Lushr,
  Iand,
  Land,
  Ior,
  Lor,
  Ixor,
  Lxor,
  Iinc,
  I2l,
  I2f,
  I2d,
  L2i,
  L2f,
  L2d,
  F2i,
  F2l,
  F2d,
  D2i,
  D2l,
  D2f,
  I2b,
  I2c,
  I2s,
  Lcmp,
  Fcmpl,
  Fcmpg,
  Dcmpl,
  Dcmpg,
  Ifeq,
  Ifne,
  Iflt,
  Ifge,
  Ifgt,
  Ifle,
  IfIcmpeq,
  IfIcmpne,
  IfIcmplt,
  IfIcmpge,
  IfIcmpgt,
  IfIcmple,
  IfAcmpeq,
  IfAcmpne,
  Goto,
  Jsr,
  Ret,
  Tableswitch,
  Lookupswitch,
  Ireturn,
  Lreturn,
  Freturn,
  Dreturn,
  Areturn,
  Return,
  Getstatic,
  Putstatic,
  Getfield,
  Putfield,
  Invokevirtual,
  Invokespecial,
  Invokestatic,
  Invokeinterface,
  Invokedynamic,
  New,
  Newarray,
  Anewarray,
  Arraylength,
  Athrow,
  Checkcast,
  Instanceof,
  Monitorenter,
  Monitorexit,
  Wide,
  Multianewarray,
  Ifnull,
  Ifnonnull,
  GotoW,
  JsrW,

  _Max,
  _Invalid = 0xff,
}


readonly struct InstructionParameter(int value, byte size)
{
  public enum PreviewType
  {
    Hidden = -1,
    Basic, // just the value integer
    Constant, // value is an index to a constant
    Local, // value is an index to a local
    JumpIndex,
    FieldRef, // value is an index to a field ref in the constants
    MethodRef, // value is an index to a method ref in the constants
  }

  public record ParameterPreview(byte Size, PreviewType Type)
  {
    public static implicit operator ParameterPreview((byte size, PreviewType type) tuple)
    {
      return new(tuple.size, tuple.type);
    }

    public static implicit operator ParameterPreview(byte size)
    {
      return new(size, PreviewType.Basic);
    }
  }

  public readonly int Value = value;
  public readonly byte Size = size;

  // null size map array indicate a complex instuction that can have dynamic size
  // depnding on the bytecode
  public static readonly Dictionary<VMOpCode, ParameterPreview[]> sParametersPreviews = new(){
    { VMOpCode.Nop,        []},
    { VMOpCode.AconstNull, []},
    { VMOpCode.IconstM1,   []},
    { VMOpCode.Iconst0,    []},
    { VMOpCode.Iconst1,    []},
    { VMOpCode.Iconst2,    []},
    { VMOpCode.Iconst3,    []},
    { VMOpCode.Iconst4,    []},
    { VMOpCode.Iconst5,    []},
    { VMOpCode.Lconst0,    []},
    { VMOpCode.Lconst1,    []},
    { VMOpCode.Fconst0,    []},
    { VMOpCode.Fconst1,    []},
    { VMOpCode.Fconst2,    []},
    { VMOpCode.Dconst0,    []},
    { VMOpCode.Dconst1,    []},

    { VMOpCode.Bipush,     [1]},
    { VMOpCode.Sipush,     [2]},
    { VMOpCode.Ldc,        [(1, PreviewType.Constant)]},
    { VMOpCode.LdcW,       [(2, PreviewType.Constant)]},
    { VMOpCode.Ldc2W,      [(2, PreviewType.Constant)]},

    {VMOpCode.Iload,       [(1, PreviewType.Local)]},
    {VMOpCode.Lload,       [(1, PreviewType.Local)]},
    {VMOpCode.Fload,       [(1, PreviewType.Local)]},
    {VMOpCode.Dload,       [(1, PreviewType.Local)]},
    {VMOpCode.Aload,       [(1, PreviewType.Local)]},

    {VMOpCode.Iload0,      []},
    {VMOpCode.Iload1,      []},
    {VMOpCode.Iload2,      []},
    {VMOpCode.Iload3,      []},
    {VMOpCode.Lload0,      []},
    {VMOpCode.Lload1,      []},
    {VMOpCode.Lload2,      []},
    {VMOpCode.Lload3,      []},
    {VMOpCode.Fload0,      []},
    {VMOpCode.Fload1,      []},
    {VMOpCode.Fload2,      []},
    {VMOpCode.Fload3,      []},
    {VMOpCode.Dload0,      []},
    {VMOpCode.Dload1,      []},
    {VMOpCode.Dload2,      []},
    {VMOpCode.Dload3,      []},
    {VMOpCode.Aload0,      []},
    {VMOpCode.Aload1,      []},
    {VMOpCode.Aload2,      []},
    {VMOpCode.Aload3,      []},
    {VMOpCode.Iaload,      []},
    {VMOpCode.Laload,      []},
    {VMOpCode.Faload,      []},
    {VMOpCode.Daload,      []},
    {VMOpCode.Aaload,      []},
    {VMOpCode.Baload,      []},
    {VMOpCode.Caload,      []},
    {VMOpCode.Saload,      []},

    {VMOpCode.Istore,      [(1, PreviewType.Local)]},
    {VMOpCode.Lstore,      [(1, PreviewType.Local)]},
    {VMOpCode.Fstore,      [(1, PreviewType.Local)]},
    {VMOpCode.Dstore,      [(1, PreviewType.Local)]},
    {VMOpCode.Astore,      [(1, PreviewType.Local)]},

    {VMOpCode.Istore0,     []},
    {VMOpCode.Istore1,     []},
    {VMOpCode.Istore2,     []},
    {VMOpCode.Istore3,     []},
    {VMOpCode.Lstore0,     []},
    {VMOpCode.Lstore1,     []},
    {VMOpCode.Lstore2,     []},
    {VMOpCode.Lstore3,     []},
    {VMOpCode.Fstore0,     []},
    {VMOpCode.Fstore1,     []},
    {VMOpCode.Fstore2,     []},
    {VMOpCode.Fstore3,     []},
    {VMOpCode.Dstore0,     []},
    {VMOpCode.Dstore1,     []},
    {VMOpCode.Dstore2,     []},
    {VMOpCode.Dstore3,     []},
    {VMOpCode.Astore0,     []},
    {VMOpCode.Astore1,     []},
    {VMOpCode.Astore2,     []},
    {VMOpCode.Astore3,     []},
    {VMOpCode.Iastore,     []},
    {VMOpCode.Lastore,     []},
    {VMOpCode.Fastore,     []},
    {VMOpCode.Dastore,     []},
    {VMOpCode.Aastore,     []},
    {VMOpCode.Bastore,     []},
    {VMOpCode.Castore,     []},
    {VMOpCode.Sastore,     []},
    {VMOpCode.Pop,         []},
    {VMOpCode.Pop2,        []},
    {VMOpCode.Dup,         []},
    {VMOpCode.DupX1,       []},
    {VMOpCode.DupX2,       []},
    {VMOpCode.Dup2,        []},
    {VMOpCode.Dup2X1,      []},
    {VMOpCode.Dup2X2,      []},
    {VMOpCode.Swap,        []},
    {VMOpCode.Iadd,        []},
    {VMOpCode.Ladd,        []},
    {VMOpCode.Fadd,        []},
    {VMOpCode.Dadd,        []},
    {VMOpCode.Isub,        []},
    {VMOpCode.Lsub,        []},
    {VMOpCode.Fsub,        []},
    {VMOpCode.Dsub,        []},
    {VMOpCode.Imul,        []},
    {VMOpCode.Lmul,        []},
    {VMOpCode.Fmul,        []},
    {VMOpCode.Dmul,        []},
    {VMOpCode.Idiv,        []},
    {VMOpCode.Ldiv,        []},
    {VMOpCode.Fdiv,        []},
    {VMOpCode.Ddiv,        []},
    {VMOpCode.Irem,        []},
    {VMOpCode.Lrem,        []},
    {VMOpCode.Frem,        []},
    {VMOpCode.Drem,        []},
    {VMOpCode.Ineg,        []},
    {VMOpCode.Lneg,        []},
    {VMOpCode.Fneg,        []},
    {VMOpCode.Dneg,        []},
    {VMOpCode.Ishl,        []},
    {VMOpCode.Lshl,        []},
    {VMOpCode.Ishr,        []},
    {VMOpCode.Lshr,        []},
    {VMOpCode.Iushr,       []},
    {VMOpCode.Lushr,       []},
    {VMOpCode.Iand,        []},
    {VMOpCode.Land,        []},
    {VMOpCode.Ior,         []},
    {VMOpCode.Lor,         []},
    {VMOpCode.Ixor,        []},
    {VMOpCode.Lxor,        []},

    {VMOpCode.Iinc,        [(1, PreviewType.Local), 1]},

    {VMOpCode.I2l,         []},
    {VMOpCode.I2f,         []},
    {VMOpCode.I2d,         []},
    {VMOpCode.L2i,         []},
    {VMOpCode.L2f,         []},
    {VMOpCode.L2d,         []},
    {VMOpCode.F2i,         []},
    {VMOpCode.F2l,         []},
    {VMOpCode.F2d,         []},
    {VMOpCode.D2i,         []},
    {VMOpCode.D2l,         []},
    {VMOpCode.D2f,         []},
    {VMOpCode.I2b,         []},
    {VMOpCode.I2c,         []},
    {VMOpCode.I2s,         []},
    {VMOpCode.Lcmp,        []},
    {VMOpCode.Fcmpl,       []},
    {VMOpCode.Fcmpg,       []},
    {VMOpCode.Dcmpl,       []},
    {VMOpCode.Dcmpg,       []},

    {VMOpCode.Ifeq,        [(2, PreviewType.JumpIndex)]},
    {VMOpCode.Ifne,        [(2, PreviewType.JumpIndex)]},
    {VMOpCode.Iflt,        [(2, PreviewType.JumpIndex)]},
    {VMOpCode.Ifge,        [(2, PreviewType.JumpIndex)]},
    {VMOpCode.Ifgt,        [(2, PreviewType.JumpIndex)]},
    {VMOpCode.Ifle,        [(2, PreviewType.JumpIndex)]},
    {VMOpCode.IfIcmpeq,    [(2, PreviewType.JumpIndex)]},
    {VMOpCode.IfIcmpne,    [(2, PreviewType.JumpIndex)]},
    {VMOpCode.IfIcmplt,    [(2, PreviewType.JumpIndex)]},
    {VMOpCode.IfIcmpge,    [(2, PreviewType.JumpIndex)]},
    {VMOpCode.IfIcmpgt,    [(2, PreviewType.JumpIndex)]},
    {VMOpCode.IfIcmple,    [(2, PreviewType.JumpIndex)]},
    {VMOpCode.IfAcmpeq,    [(2, PreviewType.JumpIndex)]},
    {VMOpCode.IfAcmpne,    [(2, PreviewType.JumpIndex)]},
    {VMOpCode.Goto,        [(2, PreviewType.JumpIndex)]},
    {VMOpCode.Jsr,         [(2, PreviewType.JumpIndex)]},
    {VMOpCode.Ret,         [(1, PreviewType.Local)]},

    {VMOpCode.Tableswitch,   null},
    {VMOpCode.Lookupswitch,  null},

    {VMOpCode.Ireturn,         []},
    {VMOpCode.Lreturn,         []},
    {VMOpCode.Freturn,         []},
    {VMOpCode.Dreturn,         []},
    {VMOpCode.Areturn,         []},
    {VMOpCode.Return,          []},

    {VMOpCode.Getstatic,       [(2, PreviewType.FieldRef)]},
    {VMOpCode.Putstatic,       [(2, PreviewType.FieldRef)]},
    {VMOpCode.Getfield,        [(2, PreviewType.FieldRef)]},
    {VMOpCode.Putfield,        [(2, PreviewType.FieldRef)]},
    {VMOpCode.Invokevirtual,   [(2, PreviewType.MethodRef)]},
    {VMOpCode.Invokespecial,   [(2, PreviewType.MethodRef)]},
    {VMOpCode.Invokestatic,    [(2, PreviewType.MethodRef)]},

    {VMOpCode.Invokeinterface, [(2, PreviewType.MethodRef), 1, (1, PreviewType.Hidden)]},
    {VMOpCode.Invokedynamic,   [(2, PreviewType.MethodRef), (2, PreviewType.Hidden)]},

    {VMOpCode.New,             [(2, PreviewType.Constant)]},
    {VMOpCode.Newarray,        [1]}, // <- primitve type index, too lazy...
    {VMOpCode.Anewarray,       [(2, PreviewType.Constant)]},

    {VMOpCode.Arraylength,     []},
    {VMOpCode.Athrow,          []},

    {VMOpCode.Checkcast,       [(2, PreviewType.Constant)]},
    {VMOpCode.Instanceof,      [(2, PreviewType.Constant)]},

    {VMOpCode.Monitorenter,    []},
    {VMOpCode.Monitorexit,     []},

    {VMOpCode.Wide,            null},

    {VMOpCode.Multianewarray,  [(2, PreviewType.Constant), 1]},

    {VMOpCode.Ifnull,          [(2, PreviewType.JumpIndex)]},
    {VMOpCode.Ifnonnull,       [(2, PreviewType.JumpIndex)]},

    {VMOpCode.GotoW,           [(4, PreviewType.JumpIndex)]},
    {VMOpCode.JsrW,            [(4, PreviewType.JumpIndex)]},
  };

  public string ToString(PreviewType type, JContextView context = default)
  {
    if (type == PreviewType.Hidden)
    {
      return "";
    }

    switch (type)
    {
      case PreviewType.Constant:
      case PreviewType.FieldRef:
      case PreviewType.MethodRef:
        if (context.Constants.Length >= Value)
        {
          return context.Constants[Value - 1].ToString();
        }
        else
        {
          return ToString(PreviewType.Basic);
        }
      case PreviewType.JumpIndex:
        return Value.ToString(Instruction.AddressFormat);
      case PreviewType.Local:
        return $"Locals[{Value}]";
      default:
        return Value.ToString();
    }
  }

  public static IEnumerable<InstructionParameter> Parse(
    VMOpCode op_code, ByteSource source)
  {
    ParameterPreview[] size_map = sParametersPreviews[op_code];
    int alignment = OpCodeDataAlignment(op_code);
    int padding = alignment == 0 ? 0 : (alignment - (source.Position % alignment)) % alignment;
    source.Seek(padding, SeekOrigin.Current);

    if (size_map is null)
    {
      return ParseDynamic(op_code, source);
    }

    return ParseSimple(size_map, source);
  }

  public static InstructionParameter ParseOne(ByteSource source, byte size)
  {
    switch (size)
    {
      case 1:
        return new(source.GetByte(), size);
      case 2:
        return new(source.GetShort(), size);
      case 4:
        return new(source.GetInt(), size);
      default: throw new ArgumentException("size should be 1, 2 or 4", nameof(size));
    }
  }

  private static int OpCodeDataAlignment(VMOpCode op_code)
  {
    if (op_code == VMOpCode.Lookupswitch || op_code == VMOpCode.Tableswitch)
    {
      return 4;
    }
    return 0;
  }

  private static IEnumerable<InstructionParameter> ParseDynamic(
    VMOpCode op_code, ByteSource source)
  {
    if (op_code == VMOpCode.Tableswitch)
    {
      return ParseDynamic_TableSwitch(source);
    }

    if (op_code == VMOpCode.Lookupswitch)
    {
      return ParseDynamic_LookupSwitch(source);
    }

    if (op_code == VMOpCode.Wide)
    {
      return ParseDynamic_Wide(source);
    }

    return null;
  }

  private static IEnumerable<InstructionParameter> ParseDynamic_TableSwitch(
    ByteSource source)
  {
    // default byte
    yield return ParseOne(source, 4);

    InstructionParameter low = ParseOne(source, 4);
    yield return low;

    InstructionParameter high = ParseOne(source, 4);
    yield return high;

    Debug.Assert(low.Value <= high.Value);

    int entries_count = high.Value - low.Value + 1;

    for (int i = 0; i < entries_count; i++)
    {
      yield return ParseOne(source, 4);
    }
  }

  private static IEnumerable<InstructionParameter> ParseDynamic_LookupSwitch(
    ByteSource source)
  {
    // default byte
    yield return ParseOne(source, 4);

    InstructionParameter npairs_count = ParseOne(source, 4);
    yield return npairs_count;

    Debug.Assert(npairs_count.Value > 0);


    for (int i = 0; i < npairs_count.Value; i++)
    {
      yield return ParseOne(source, 4); // match
      yield return ParseOne(source, 4); // offset
    }
  }

  private static IEnumerable<InstructionParameter> ParseDynamic_Wide(
    ByteSource source)
  {
    InstructionParameter op_code_param = ParseOne(source, 1);
    yield return op_code_param;

    VMOpCode op_code = Instruction.ReadOpCode((byte)op_code_param.Value);
    Debug.Assert(op_code != VMOpCode._Invalid);

    yield return ParseOne(source, 2);

    if (op_code == VMOpCode.Iinc)
    {
      yield return ParseOne(source, 2);
    }
  }

  private static IEnumerable<InstructionParameter> ParseSimple(
    ParameterPreview[] previews, ByteSource source)
  {
    foreach (ParameterPreview preview in previews)
    {
      yield return ParseOne(source, preview.Size);
    }
  }

}

readonly struct Instruction(VMOpCode op_code, IEnumerable<InstructionParameter> parameters,
                            int address = 0)
{
  public const string AddressFormat = "X4";

  public readonly VMOpCode OpCode = op_code;
  public readonly InstructionParameter[] Parameters = [.. parameters];
  public readonly int Address = address;

  public Instruction()
    : this(VMOpCode._Invalid, [])
  {
  }

  public static VMOpCode ReadOpCode(byte value)
  {
    byte byte_type = value;
    if (byte_type >= (byte)VMOpCode._Max)
    {
      return VMOpCode._Invalid;
    }

    return (VMOpCode)byte_type;
  }

  public static VMOpCode ReadOpCode(ByteSource reader)
  {
    return ReadOpCode(reader.GetByte());
  }

  public static Instruction Read(ByteSource source)
  {
    int address = source.Position;
    VMOpCode type = ReadOpCode(source.GetByte());

    if (type == VMOpCode._Invalid)
    {
      return new();
    }

    var parameters = InstructionParameter.Parse(
      type, source
    );

    return new Instruction(type, parameters, address);
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
    if (OpCode >= VMOpCode._Max)
    {
      return "????";
    }

    StringBuilder builder = new();
    builder.Append(Address.ToString(AddressFormat));
    builder.Append(' ');
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
        builder.Append(p.ToString(param_info[index].Type, context));
      }

    }

    return builder.ToString();
  }

}
