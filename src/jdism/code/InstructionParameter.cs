

using System.ComponentModel;
using System.Diagnostics;

namespace JDism;

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
  public static readonly Dictionary<JVMOpCode, ParameterPreview[]> sParametersPreviews = new(){
    { JVMOpCode.Nop,        []},
    { JVMOpCode.AconstNull, []},
    { JVMOpCode.IconstM1,   []},
    { JVMOpCode.Iconst0,    []},
    { JVMOpCode.Iconst1,    []},
    { JVMOpCode.Iconst2,    []},
    { JVMOpCode.Iconst3,    []},
    { JVMOpCode.Iconst4,    []},
    { JVMOpCode.Iconst5,    []},
    { JVMOpCode.Lconst0,    []},
    { JVMOpCode.Lconst1,    []},
    { JVMOpCode.Fconst0,    []},
    { JVMOpCode.Fconst1,    []},
    { JVMOpCode.Fconst2,    []},
    { JVMOpCode.Dconst0,    []},
    { JVMOpCode.Dconst1,    []},

    { JVMOpCode.Bipush,     [1]},
    { JVMOpCode.Sipush,     [2]},
    { JVMOpCode.Ldc,        [(1, PreviewType.Constant)]},
    { JVMOpCode.LdcW,       [(2, PreviewType.Constant)]},
    { JVMOpCode.Ldc2W,      [(2, PreviewType.Constant)]},

    {JVMOpCode.Iload,       [(1, PreviewType.Local)]},
    {JVMOpCode.Lload,       [(1, PreviewType.Local)]},
    {JVMOpCode.Fload,       [(1, PreviewType.Local)]},
    {JVMOpCode.Dload,       [(1, PreviewType.Local)]},
    {JVMOpCode.Aload,       [(1, PreviewType.Local)]},

    {JVMOpCode.Iload0,      []},
    {JVMOpCode.Iload1,      []},
    {JVMOpCode.Iload2,      []},
    {JVMOpCode.Iload3,      []},
    {JVMOpCode.Lload0,      []},
    {JVMOpCode.Lload1,      []},
    {JVMOpCode.Lload2,      []},
    {JVMOpCode.Lload3,      []},
    {JVMOpCode.Fload0,      []},
    {JVMOpCode.Fload1,      []},
    {JVMOpCode.Fload2,      []},
    {JVMOpCode.Fload3,      []},
    {JVMOpCode.Dload0,      []},
    {JVMOpCode.Dload1,      []},
    {JVMOpCode.Dload2,      []},
    {JVMOpCode.Dload3,      []},
    {JVMOpCode.Aload0,      []},
    {JVMOpCode.Aload1,      []},
    {JVMOpCode.Aload2,      []},
    {JVMOpCode.Aload3,      []},
    {JVMOpCode.Iaload,      []},
    {JVMOpCode.Laload,      []},
    {JVMOpCode.Faload,      []},
    {JVMOpCode.Daload,      []},
    {JVMOpCode.Aaload,      []},
    {JVMOpCode.Baload,      []},
    {JVMOpCode.Caload,      []},
    {JVMOpCode.Saload,      []},

    {JVMOpCode.Istore,      [(1, PreviewType.Local)]},
    {JVMOpCode.Lstore,      [(1, PreviewType.Local)]},
    {JVMOpCode.Fstore,      [(1, PreviewType.Local)]},
    {JVMOpCode.Dstore,      [(1, PreviewType.Local)]},
    {JVMOpCode.Astore,      [(1, PreviewType.Local)]},

    {JVMOpCode.Istore0,     []},
    {JVMOpCode.Istore1,     []},
    {JVMOpCode.Istore2,     []},
    {JVMOpCode.Istore3,     []},
    {JVMOpCode.Lstore0,     []},
    {JVMOpCode.Lstore1,     []},
    {JVMOpCode.Lstore2,     []},
    {JVMOpCode.Lstore3,     []},
    {JVMOpCode.Fstore0,     []},
    {JVMOpCode.Fstore1,     []},
    {JVMOpCode.Fstore2,     []},
    {JVMOpCode.Fstore3,     []},
    {JVMOpCode.Dstore0,     []},
    {JVMOpCode.Dstore1,     []},
    {JVMOpCode.Dstore2,     []},
    {JVMOpCode.Dstore3,     []},
    {JVMOpCode.Astore0,     []},
    {JVMOpCode.Astore1,     []},
    {JVMOpCode.Astore2,     []},
    {JVMOpCode.Astore3,     []},
    {JVMOpCode.Iastore,     []},
    {JVMOpCode.Lastore,     []},
    {JVMOpCode.Fastore,     []},
    {JVMOpCode.Dastore,     []},
    {JVMOpCode.Aastore,     []},
    {JVMOpCode.Bastore,     []},
    {JVMOpCode.Castore,     []},
    {JVMOpCode.Sastore,     []},
    {JVMOpCode.Pop,         []},
    {JVMOpCode.Pop2,        []},
    {JVMOpCode.Dup,         []},
    {JVMOpCode.DupX1,       []},
    {JVMOpCode.DupX2,       []},
    {JVMOpCode.Dup2,        []},
    {JVMOpCode.Dup2X1,      []},
    {JVMOpCode.Dup2X2,      []},
    {JVMOpCode.Swap,        []},
    {JVMOpCode.Iadd,        []},
    {JVMOpCode.Ladd,        []},
    {JVMOpCode.Fadd,        []},
    {JVMOpCode.Dadd,        []},
    {JVMOpCode.Isub,        []},
    {JVMOpCode.Lsub,        []},
    {JVMOpCode.Fsub,        []},
    {JVMOpCode.Dsub,        []},
    {JVMOpCode.Imul,        []},
    {JVMOpCode.Lmul,        []},
    {JVMOpCode.Fmul,        []},
    {JVMOpCode.Dmul,        []},
    {JVMOpCode.Idiv,        []},
    {JVMOpCode.Ldiv,        []},
    {JVMOpCode.Fdiv,        []},
    {JVMOpCode.Ddiv,        []},
    {JVMOpCode.Irem,        []},
    {JVMOpCode.Lrem,        []},
    {JVMOpCode.Frem,        []},
    {JVMOpCode.Drem,        []},
    {JVMOpCode.Ineg,        []},
    {JVMOpCode.Lneg,        []},
    {JVMOpCode.Fneg,        []},
    {JVMOpCode.Dneg,        []},
    {JVMOpCode.Ishl,        []},
    {JVMOpCode.Lshl,        []},
    {JVMOpCode.Ishr,        []},
    {JVMOpCode.Lshr,        []},
    {JVMOpCode.Iushr,       []},
    {JVMOpCode.Lushr,       []},
    {JVMOpCode.Iand,        []},
    {JVMOpCode.Land,        []},
    {JVMOpCode.Ior,         []},
    {JVMOpCode.Lor,         []},
    {JVMOpCode.Ixor,        []},
    {JVMOpCode.Lxor,        []},

    {JVMOpCode.Iinc,        [(1, PreviewType.Local), 1]},

    {JVMOpCode.I2l,         []},
    {JVMOpCode.I2f,         []},
    {JVMOpCode.I2d,         []},
    {JVMOpCode.L2i,         []},
    {JVMOpCode.L2f,         []},
    {JVMOpCode.L2d,         []},
    {JVMOpCode.F2i,         []},
    {JVMOpCode.F2l,         []},
    {JVMOpCode.F2d,         []},
    {JVMOpCode.D2i,         []},
    {JVMOpCode.D2l,         []},
    {JVMOpCode.D2f,         []},
    {JVMOpCode.I2b,         []},
    {JVMOpCode.I2c,         []},
    {JVMOpCode.I2s,         []},
    {JVMOpCode.Lcmp,        []},
    {JVMOpCode.Fcmpl,       []},
    {JVMOpCode.Fcmpg,       []},
    {JVMOpCode.Dcmpl,       []},
    {JVMOpCode.Dcmpg,       []},

    {JVMOpCode.Ifeq,        [(2, PreviewType.JumpIndex)]},
    {JVMOpCode.Ifne,        [(2, PreviewType.JumpIndex)]},
    {JVMOpCode.Iflt,        [(2, PreviewType.JumpIndex)]},
    {JVMOpCode.Ifge,        [(2, PreviewType.JumpIndex)]},
    {JVMOpCode.Ifgt,        [(2, PreviewType.JumpIndex)]},
    {JVMOpCode.Ifle,        [(2, PreviewType.JumpIndex)]},
    {JVMOpCode.IfIcmpeq,    [(2, PreviewType.JumpIndex)]},
    {JVMOpCode.IfIcmpne,    [(2, PreviewType.JumpIndex)]},
    {JVMOpCode.IfIcmplt,    [(2, PreviewType.JumpIndex)]},
    {JVMOpCode.IfIcmpge,    [(2, PreviewType.JumpIndex)]},
    {JVMOpCode.IfIcmpgt,    [(2, PreviewType.JumpIndex)]},
    {JVMOpCode.IfIcmple,    [(2, PreviewType.JumpIndex)]},
    {JVMOpCode.IfAcmpeq,    [(2, PreviewType.JumpIndex)]},
    {JVMOpCode.IfAcmpne,    [(2, PreviewType.JumpIndex)]},
    {JVMOpCode.Goto,        [(2, PreviewType.JumpIndex)]},
    {JVMOpCode.Jsr,         [(2, PreviewType.JumpIndex)]},
    {JVMOpCode.Ret,         [(1, PreviewType.Local)]},

    {JVMOpCode.Tableswitch,   null},
    {JVMOpCode.Lookupswitch,  null},

    {JVMOpCode.Ireturn,         []},
    {JVMOpCode.Lreturn,         []},
    {JVMOpCode.Freturn,         []},
    {JVMOpCode.Dreturn,         []},
    {JVMOpCode.Areturn,         []},
    {JVMOpCode.Return,          []},

    {JVMOpCode.Getstatic,       [(2, PreviewType.FieldRef)]},
    {JVMOpCode.Putstatic,       [(2, PreviewType.FieldRef)]},
    {JVMOpCode.Getfield,        [(2, PreviewType.FieldRef)]},
    {JVMOpCode.Putfield,        [(2, PreviewType.FieldRef)]},
    {JVMOpCode.Invokevirtual,   [(2, PreviewType.MethodRef)]},
    {JVMOpCode.Invokespecial,   [(2, PreviewType.MethodRef)]},
    {JVMOpCode.Invokestatic,    [(2, PreviewType.MethodRef)]},

    {JVMOpCode.Invokeinterface, [(2, PreviewType.MethodRef), 1, (1, PreviewType.Hidden)]},
    {JVMOpCode.Invokedynamic,   [(2, PreviewType.MethodRef), (2, PreviewType.Hidden)]},

    {JVMOpCode.New,             [(2, PreviewType.Constant)]},
    {JVMOpCode.Newarray,        [1]}, // <- primitve type index, too lazy...
    {JVMOpCode.Anewarray,       [(2, PreviewType.Constant)]},

    {JVMOpCode.Arraylength,     []},
    {JVMOpCode.Athrow,          []},

    {JVMOpCode.Checkcast,       [(2, PreviewType.Constant)]},
    {JVMOpCode.Instanceof,      [(2, PreviewType.Constant)]},

    {JVMOpCode.Monitorenter,    []},
    {JVMOpCode.Monitorexit,     []},

    {JVMOpCode.Wide,            null},

    {JVMOpCode.Multianewarray,  [(2, PreviewType.Constant), 1]},

    {JVMOpCode.Ifnull,          [(2, PreviewType.JumpIndex)]},
    {JVMOpCode.Ifnonnull,       [(2, PreviewType.JumpIndex)]},

    {JVMOpCode.GotoW,           [(4, PreviewType.JumpIndex)]},
    {JVMOpCode.JsrW,            [(4, PreviewType.JumpIndex)]},
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
    JVMOpCode op_code, ByteSource source)
  {
    ParameterPreview[] size_map = sParametersPreviews[op_code];
    int alignment = OpCodeDataAlignment(op_code);
    int padding = alignment == 0 ? 0 : (alignment - (source.Position % alignment)) % alignment;
    source.Seek(padding, SeekOrigin.Current);

    InstructionParameter[] padding_parameter = [new(0, (byte)padding)];

    if (size_map is null)
    {
      return  padding_parameter.Concat(ParseDynamic(op_code, source));
    }

    return padding_parameter.Concat(ParseSimple(size_map, source));
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

  private static int OpCodeDataAlignment(JVMOpCode op_code)
  {
    if (op_code == JVMOpCode.Lookupswitch || op_code == JVMOpCode.Tableswitch)
    {
      return 4;
    }
    return 0;
  }

  private static IEnumerable<InstructionParameter> ParseDynamic(
    JVMOpCode op_code, ByteSource source)
  {
    if (op_code == JVMOpCode.Tableswitch)
    {
      return ParseDynamic_TableSwitch(source);
    }

    if (op_code == JVMOpCode.Lookupswitch)
    {
      return ParseDynamic_LookupSwitch(source);
    }

    if (op_code == JVMOpCode.Wide)
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

    JVMOpCode op_code = Instruction.ReadOpCode((byte)op_code_param.Value);
    Debug.Assert(op_code != JVMOpCode._Invalid);

    yield return ParseOne(source, 2);

    if (op_code == JVMOpCode.Iinc)
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
