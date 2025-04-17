

using Colussom;
using System.Diagnostics;
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


public readonly struct InstructionParameter(int value, byte size)
{
  public readonly int Value = value;
  public readonly byte Size = size;

  // null size map array indicate a complex instuction that can have dynamic size
  // depnding on the bytecode
  public static readonly Dictionary<VMOpCode, byte[]> sParametersSizeMap = new(){
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
    { VMOpCode.Ldc,        [1]},
    { VMOpCode.LdcW,       [2]},
    { VMOpCode.Ldc2W,      [2]},

    {VMOpCode.Iload,       [1]},
    {VMOpCode.Lload,       [1]},
    {VMOpCode.Fload,       [1]},
    {VMOpCode.Dload,       [1]},
    {VMOpCode.Aload,       [1]},

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

    {VMOpCode.Istore,      [1]},
    {VMOpCode.Lstore,      [1]},
    {VMOpCode.Fstore,      [1]},
    {VMOpCode.Dstore,      [1]},
    {VMOpCode.Astore,      [1]},

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

    {VMOpCode.Iinc,        [1, 1]},

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

    {VMOpCode.Ifeq,        [2]},
    {VMOpCode.Ifne,        [2]},
    {VMOpCode.Iflt,        [2]},
    {VMOpCode.Ifge,        [2]},
    {VMOpCode.Ifgt,        [2]},
    {VMOpCode.Ifle,        [2]},
    {VMOpCode.IfIcmpeq,    [2]},
    {VMOpCode.IfIcmpne,    [2]},
    {VMOpCode.IfIcmplt,    [2]},
    {VMOpCode.IfIcmpge,    [2]},
    {VMOpCode.IfIcmpgt,    [2]},
    {VMOpCode.IfIcmple,    [2]},
    {VMOpCode.IfAcmpeq,    [2]},
    {VMOpCode.IfAcmpne,    [2]},
    {VMOpCode.Goto,        [2]},
    {VMOpCode.Jsr,         [2]},
    {VMOpCode.Ret,         [1]},

    {VMOpCode.Tableswitch,   null},
    {VMOpCode.Lookupswitch,  null},

    {VMOpCode.Ireturn,         []},
    {VMOpCode.Lreturn,         []},
    {VMOpCode.Freturn,         []},
    {VMOpCode.Dreturn,         []},
    {VMOpCode.Areturn,         []},
    {VMOpCode.Return,          []},

    {VMOpCode.Getstatic,       [2]},
    {VMOpCode.Putstatic,       [2]},
    {VMOpCode.Getfield,        [2]},
    {VMOpCode.Putfield,        [2]},
    {VMOpCode.Invokevirtual,   [2]},
    {VMOpCode.Invokespecial,   [2]},
    {VMOpCode.Invokestatic,    [2]},

    {VMOpCode.Invokeinterface, [2, 1, 1]},
    {VMOpCode.Invokedynamic,   [2, 2]},

    {VMOpCode.New,             [2]},
    {VMOpCode.Newarray,        [1]},
    {VMOpCode.Anewarray,       [2]},

    {VMOpCode.Arraylength,     []},
    {VMOpCode.Athrow,          []},

    {VMOpCode.Checkcast,       [2]},
    {VMOpCode.Instanceof,      [2]},

    {VMOpCode.Monitorenter,    []},
    {VMOpCode.Monitorexit,     []},

    {VMOpCode.Wide,            null},

    {VMOpCode.Multianewarray,  [2, 1]},

    {VMOpCode.Ifnull,          [2]},
    {VMOpCode.Ifnonnull,       [2]},

    {VMOpCode.GotoW,           [4]},
    {VMOpCode.JsrW,            [4]},
  };

  public static (IEnumerable<InstructionParameter>, int) Parse(
    VMOpCode op_code, byte[] bytecode, int index)
  {
    byte[] size_map = sParametersSizeMap[op_code];
    int alignment = OpCodeDataAlignment(op_code);
    int padding = (alignment - (index % alignment)) % alignment;
    index += padding;

    if (size_map is null)
    {
      InstructionParameter[] result = [.. ParseDynamic(op_code, bytecode, index)];

      return (
        result,
        result.Aggregate(0, (acc, i) => acc + i.Size) + padding
      );
    }

    return (
      ParseSimple(size_map, bytecode, index),
      size_map.Aggregate(0, (a, b) => a + b) + padding
    );
  }

  public static InstructionParameter FromData(byte[] data, int index, byte size)
  {
    // big endian

    int value = 0;
    for (int i = 0; i < size; i++)
    {
      value |= data[i + index] << ((size - i - 1) * 8);
    }
    return new(value, size);
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
    VMOpCode op_code, byte[] bytecode, int index)
  {
    if (op_code == VMOpCode.Tableswitch)
    {
      return ParseDynamic_TableSwitch(bytecode, index);
    }

    if (op_code == VMOpCode.Lookupswitch)
    {
      return ParseDynamic_LookupSwitch(bytecode, index);
    }

    if (op_code == VMOpCode.Wide)
    {
      return ParseDynamic_Wide(bytecode, index);
    }

    return null;
  }

  private static IEnumerable<InstructionParameter> ParseDynamic_TableSwitch(
    byte[] bytecode, int index)
  {
    // default byte
    yield return FromData(bytecode, index, 4);
    index += 4;

    InstructionParameter low = FromData(bytecode, index, 4);
    yield return low;
    index += 4;

    InstructionParameter high = FromData(bytecode, index, 4);
    yield return high;
    index += 4;

    Debug.Assert(low.Value <= high.Value);

    int entries_count = high.Value - low.Value + 1;

    for (int i = 0; i < entries_count; i++)
    {
      yield return FromData(bytecode, index, 4);
      index += 4;
    }
  }

  private static IEnumerable<InstructionParameter> ParseDynamic_LookupSwitch(
    byte[] bytecode, int index)
  {
    // default byte
    yield return FromData(bytecode, index, 4);
    index += 4;

    InstructionParameter npairs_count = FromData(bytecode, index, 4);
    yield return npairs_count;
    index += 4;

    Debug.Assert(npairs_count.Value > 0);


    for (int i = 0; i < npairs_count.Value; i++)
    {
      yield return FromData(bytecode, index, 4); // match
      yield return FromData(bytecode, index + 4, 4); // offset
      index += 8;
    }
  }

  private static IEnumerable<InstructionParameter> ParseDynamic_Wide(
    byte[] bytecode, int index)
  {
    InstructionParameter op_code_param = FromData(bytecode, index, 1);
    yield return op_code_param;
    index++;

    VMOpCode op_code = Instruction.ReadOpCode((byte)op_code_param.Value);
    Debug.Assert(op_code != VMOpCode._Invalid);

    yield return FromData(bytecode, index, 2);
    index += 2;

    if (op_code == VMOpCode.Iinc)
    {
      yield return FromData(bytecode, index, 2);
      index += 2;
    }
  }

  private static IEnumerable<InstructionParameter> ParseSimple(
    byte[] size_map, byte[] bytecode, int index)
  {
    foreach (byte size in size_map)
    {
      yield return FromData(bytecode, index, size);
      index += size;
    }
  }

}

public readonly struct Instruction(VMOpCode op_code, IEnumerable<InstructionParameter> parameters)
{
  public readonly VMOpCode OpCode = op_code;
  private readonly InstructionParameter[] Parameters = [.. parameters];

  public Instruction()
    : this(VMOpCode._Invalid, [])
  {
  }

  public static string InstructionName(VMOpCode code) => Names[(int)code];

  #region
  private static readonly string[] Names = [
    "nop",
    "aconst_null",
    "iconst_m1",
    "iconst_0",
    "iconst_1",
    "iconst_2",
    "iconst_3",
    "iconst_4",
    "iconst_5",
    "lconst_0",
    "lconst_1",
    "fconst_0",
    "fconst_1",
    "fconst_2",
    "dconst_0",
    "dconst_1",
    "bipush",
    "sipush",
    "ldc",
    "ldc_w",
    "ldc2_w",
    "iload",
    "lload",
    "fload",
    "dload",
    "aload",
    "iload_0",
    "iload_1",
    "iload_2",
    "iload_3",
    "lload_0",
    "lload_1",
    "lload_2",
    "lload_3",
    "fload_0",
    "fload_1",
    "fload_2",
    "fload_3",
    "dload_0",
    "dload_1",
    "dload_2",
    "dload_3",
    "aload_0",
    "aload_1",
    "aload_2",
    "aload_3",
    "iaload",
    "laload",
    "faload",
    "daload",
    "aaload",
    "baload",
    "caload",
    "saload",
    "istore",
    "lstore",
    "fstore",
    "dstore",
    "astore",
    "istore_0",
    "istore_1",
    "istore_2",
    "istore_3",
    "lstore_0",
    "lstore_1",
    "lstore_2",
    "lstore_3",
    "fstore_0",
    "fstore_1",
    "fstore_2",
    "fstore_3",
    "dstore_0",
    "dstore_1",
    "dstore_2",
    "dstore_3",
    "astore_0",
    "astore_1",
    "astore_2",
    "astore_3",
    "iastore",
    "lastore",
    "fastore",
    "dastore",
    "aastore",
    "bastore",
    "castore",
    "sastore",
    "pop",
    "pop2",
    "dup",
    "dup_x1",
    "dup_x2",
    "dup2",
    "dup2_x1",
    "dup2_x2",
    "swap",
    "iadd",
    "ladd",
    "fadd",
    "dadd",
    "isub",
    "lsub",
    "fsub",
    "dsub",
    "imul",
    "lmul",
    "fmul",
    "dmul",
    "idiv",
    "ldiv",
    "fdiv",
    "ddiv",
    "irem",
    "lrem",
    "frem",
    "drem",
    "ineg",
    "lneg",
    "fneg",
    "dneg",
    "ishl",
    "lshl",
    "ishr",
    "lshr",
    "iushr",
    "lushr",
    "iand",
    "land",
    "ior",
    "lor",
    "ixor",
    "lxor",
    "iinc",
    "i2l",
    "i2f",
    "i2d",
    "l2i",
    "l2f",
    "l2d",
    "f2i",
    "f2l",
    "f2d",
    "d2i",
    "d2l",
    "d2f",
    "i2b",
    "i2c",
    "i2s",
    "lcmp",
    "fcmpl",
    "fcmpg",
    "dcmpl",
    "dcmpg",
    "ifeq",
    "ifne",
    "iflt",
    "ifge",
    "ifgt",
    "ifle",
    "if_icmpeq",
    "if_icmpne",
    "if_icmplt",
    "if_icmpge",
    "if_icmpgt",
    "if_icmple",
    "if_acmpeq",
    "if_acmpne",
    "goto",
    "jsr",
    "ret",
    "tableswitch",
    "lookupswitch",
    "ireturn",
    "lreturn",
    "freturn",
    "dreturn",
    "areturn",
    "return",
    "getstatic",
    "putstatic",
    "getfield",
    "putfield",
    "invokevirtual",
    "invokespecial",
    "invokestatic",
    "invokeinterface",
    "invokedynamic",
    "new",
    "newarray",
    "anewarray",
    "arraylength",
    "athrow",
    "checkcast",
    "instanceof",
    "monitorenter",
    "monitorexit",
    "wide",
    "multianewarray",
    "ifnull",
    "ifnonnull",
    "goto_w",
    "jsr_w"
  ];
  #endregion

  public static VMOpCode ReadOpCode(byte value)
  {
    byte byte_type = value;
    if (byte_type >= (byte)VMOpCode._Max)
    {
      return VMOpCode._Invalid;
    }

    return (VMOpCode)byte_type;
  }

  public static VMOpCode ReadOpCode(ByteReader reader)
  {
    return ReadOpCode(reader.ReadByte());
  }

  public static Instruction Read(ByteReader reader)
  {
    VMOpCode type = ReadOpCode(reader);
    if (type == VMOpCode._Invalid)
    {
      return new();
    }

    var (parameters, read_length) = InstructionParameter.Parse(
      type,
      reader.Bytes,
      reader.Position
    );

    _ = reader.Read(read_length);

    return new Instruction(type, parameters);
  }

  public override string ToString()
  {
    if (OpCode >= VMOpCode._Max)
    {
      return "????";
    }

    if (Parameters.Length > 0)
    {
      StringBuilder builder = new();
      builder.Append(OpCode.ToString());
      builder.Append(' ');
      builder.Append(string.Join(", ", from p in Parameters select p.Value));
      return builder.ToString();
    }

    return OpCode.ToString();
  }

}
