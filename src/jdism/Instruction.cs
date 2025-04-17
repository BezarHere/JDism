

using Colussom;
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


public struct Instruction
{
	public readonly VMOpCode OpCode;
	private readonly byte[] Data;

	public const Endianness DataEndianness = Endianness.Big;

	public Instruction(VMOpCode OpCode, byte[] data)
	{
		this.OpCode = OpCode;
		this.Data = data;
	}

	public Instruction(VMOpCode OpCode)
	{
		this.OpCode = OpCode;
		this.Data = [];
	}

	public Instruction()
	{
		this.OpCode = VMOpCode._Invalid;
	}

	public static string InstructionName(VMOpCode code) => Names[(int)code];

	// in bytes
	// if negative, the instruction length varies.
	// the abs(InstructionLength(X)) is the minimum bytes to load the instruction
	static public int InstructionLength(VMOpCode code)
	{
		switch (code)
		{
			case VMOpCode.Bipush:
			case VMOpCode.Ldc:
			case VMOpCode.Iload:
			case VMOpCode.Lload:
			case VMOpCode.Fload:
			case VMOpCode.Dload:
			case VMOpCode.Aload:
			case VMOpCode.Istore:
			case VMOpCode.Lstore:
			case VMOpCode.Fstore:
			case VMOpCode.Dstore:
			case VMOpCode.Astore:
			case VMOpCode.Newarray:
			case VMOpCode.Ret:
				return 1;
			case VMOpCode.Sipush:
			case VMOpCode.LdcW:
			case VMOpCode.Ldc2W:
			case VMOpCode.Iinc:
			case VMOpCode.Ifeq:
			case VMOpCode.Ifne:
			case VMOpCode.Iflt:
			case VMOpCode.Ifge:
			case VMOpCode.Ifgt:
			case VMOpCode.Ifle:
			case VMOpCode.IfIcmpeq:
			case VMOpCode.IfIcmpne:
			case VMOpCode.IfIcmplt:
			case VMOpCode.IfIcmpge:
			case VMOpCode.IfIcmpgt:
			case VMOpCode.IfIcmple:
			case VMOpCode.IfAcmpeq:
			case VMOpCode.IfAcmpne:
			case VMOpCode.Goto:
			case VMOpCode.Jsr:
			case VMOpCode.Getstatic:
			case VMOpCode.Putstatic:
			case VMOpCode.Getfield:
			case VMOpCode.Putfield:
			case VMOpCode.Invokevirtual:
			case VMOpCode.Invokespecial:
			case VMOpCode.Invokestatic:
			case VMOpCode.New:
			case VMOpCode.Anewarray:
			case VMOpCode.Checkcast:
			case VMOpCode.Instanceof:
			case VMOpCode.Ifnull:
			case VMOpCode.Ifnonnull:
				return 2;
			case VMOpCode.Multianewarray:
				return 3;
			case VMOpCode.Invokeinterface:
			case VMOpCode.Invokedynamic:
			case VMOpCode.GotoW:
			case VMOpCode.JsrW:
				return 4;
			case VMOpCode.Wide:
				return -5; // -3?
			case VMOpCode.Lookupswitch:
				return -8;
			case VMOpCode.Tableswitch:
				return -16;
			default:
				return 0;
		}
	}

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

	private static VMOpCode ReadType(ByteReader reader)
	{
		byte byte_type = reader.ReadByte();
		if (byte_type >= (byte)VMOpCode._Max)
		{
			return VMOpCode._Invalid;
		}

		return (VMOpCode)byte_type;
	}

	private static Instruction ReadWide(ByteReader reader)
	{
		VMOpCode type = ReadType(reader);
		if (type == VMOpCode._Invalid)
		{
			return new();
		}

		int length = InstructionLength(type);
		if (length >= 0)
		{
			return new(type, reader.Read(length * 2));
		}

		return new();
	}

	public static Instruction Read(ByteReader reader)
	{
		VMOpCode type = ReadType(reader);
		if (type == VMOpCode._Invalid)
		{
			return new();
		}

		int length = InstructionLength(type);
		if (length >= 0)
		{
			return new(type, reader.Read(length));
		}

		switch (type)
		{
			case VMOpCode.Tableswitch:
			{
				// skip 4 bytes
				reader.ReadInt();
				// Console.WriteLine($"encountred a table switch: {reader.ReadInt()}, {reader.ReadInt()}, {reader.ReadInt()}");
				break;
			}
			case VMOpCode.Lookupswitch:
			{
				// skip 4 bytes
				reader.ReadInt();
				// Console.WriteLine($"encountred a lookup switch: {reader.ReadInt()}, {reader.ReadInt()}, {reader.ReadInt()}");
				break;
			}
			case VMOpCode.Wide:
			{
				return ReadWide(reader);
			}
			default:
				// Console.WriteLine($"couldn't read the instruction of type {type}, length = {length}");
				break;
		}


		return new();
	}

	public override string ToString() => ToString(null);
	public string ToString(Constant[] constants)
	{
		if (OpCode >= VMOpCode._Max)
		{
			return "invalid";
		}
		StringBuilder builder = new();
		builder.Append(InstructionName(OpCode));
		if (Data.Length > 0)
		{
			builder.Append(' ');
			StringfyOpCodeData(builder, constants);
		}
		return builder.ToString();
	}

	public void StringfyOpCodeData(StringBuilder builder, Constant[] constants)
	{
		if (Data.Length == 0)
			return;

		ByteReader reader = new(Data, 0, DataEndianness);

		string get_constant(int index)
		{
      index--;

			// no constants, just the index
			if (constants is null)
				return index.ToString();

			// out of range index
			if (constants.Length <= index || index < 0)
				return $"{index}:OOR";

			var constant = constants[index];
			switch (constant.type)
			{
				case ConstantType.Integer:
				case ConstantType.Long:
					return constant.String;
				case ConstantType.Float:
					return constant.String + 'f';
				case ConstantType.Double:
					return constant.String + 'F';
				case ConstantType.String:
				case ConstantType.StringReference:
					return $"\"{constant.String}\"";
			}

			return $"{constant.String}:{constant.type}";
		}

		switch (OpCode)
		{
			case VMOpCode.Bipush:
			{
				builder.Append($"{reader.ReadByte()}");
				break;
			}
			case VMOpCode.Sipush:
			{
				builder.Append($"{reader.ReadByte()}");
				break;
			}
			case VMOpCode.Ldc:
			{
				builder.Append(get_constant(reader.ReadByte()));
				break;
			}
			case VMOpCode.LdcW:
			{
				builder.Append(get_constant(reader.ReadShort()));
				break;
			}
			case VMOpCode.Ldc2W:
			{
				builder.Append(get_constant(reader.ReadShort()));
				break;
			}
			case VMOpCode.Iload:
			case VMOpCode.Fload:
			case VMOpCode.Dload:
			case VMOpCode.Lload:
			case VMOpCode.Aload:
			{
				builder.Append('{');
				builder.Append($"LL:{reader.ReadByte()}");
				builder.Append('}');
				break;
			}
			case VMOpCode.Istore:
			case VMOpCode.Fstore:
			case VMOpCode.Dstore:
			case VMOpCode.Lstore:
			case VMOpCode.Astore:
			{
				builder.Append('{');
				builder.Append($"LS:{reader.ReadByte()}");
				builder.Append('}');
				break;
			}
			case VMOpCode.Iinc:
			{
				builder.Append('{');
				builder.Append($"LF:{reader.ReadByte()}");
				builder.Append('}');
				builder.Append(' ');
				builder.Append($"{reader.ReadByte()}");
				break;
			}
			case VMOpCode.Ifnull:
			case VMOpCode.Ifnonnull:
			case VMOpCode.Ifeq:
			case VMOpCode.Ifne:
			case VMOpCode.Iflt:
			case VMOpCode.Ifge:
			case VMOpCode.Ifgt:
			case VMOpCode.Ifle:
			case VMOpCode.IfIcmpeq:
			case VMOpCode.IfIcmpne:
			case VMOpCode.IfIcmplt:
			case VMOpCode.IfIcmpge:
			case VMOpCode.IfIcmpgt:
			case VMOpCode.IfIcmple:
			case VMOpCode.IfAcmpeq:
			case VMOpCode.IfAcmpne:
			case VMOpCode.Goto:
			case VMOpCode.Jsr:
			{
				builder.Append($"{reader.ReadShort()}");
				break;
			}
			case VMOpCode.Ret:
			{
				builder.Append($"LF:{reader.ReadByte()}");
				break;
			}
			case VMOpCode.Tableswitch:
			{
				builder.Append(reader.ReadInt());
				builder.Append(' ');
				builder.Append(reader.ReadInt());
				builder.Append(' ');
				builder.Append(reader.ReadInt());
				builder.Append(' ');
				builder.Append(reader.ReadInt());
				builder.Append(' ');
				builder.Append($"LN={reader.SpaceLeft}");
				break;
			}
			case VMOpCode.Lookupswitch:
			{
				builder.Append(reader.ReadInt());
				builder.Append(' ');
				builder.Append(reader.ReadInt());
				builder.Append(' ');
				builder.Append($"LN={reader.SpaceLeft}");
				break;
			}
			case VMOpCode.Getstatic:
			case VMOpCode.Putstatic:
			case VMOpCode.Getfield:
			case VMOpCode.Putfield:
			{
				builder.Append(get_constant(reader.ReadShort()));
				break;
			}
			case VMOpCode.Invokespecial:
			case VMOpCode.Invokestatic:
			case VMOpCode.Invokevirtual:
			{
				builder.Append(get_constant(reader.ReadShort()));
				break;
			}
			case VMOpCode.Invokeinterface:
			{
				builder.Append(get_constant(reader.ReadShort()));
				builder.Append(' ');
				builder.Append($"0x{reader.ReadByte()}");
				break;
			}
			case VMOpCode.Invokedynamic:
			{
				builder.Append(get_constant(reader.ReadShort()));
				builder.Append(' ');
				builder.Append($"0x{reader.ReadShort()}");
				break;
			}
			case VMOpCode.New:
			{
				builder.Append(get_constant(reader.ReadShort()));
				break;
			}
			case VMOpCode.Newarray:
			{
				builder.Append($"{(JTypeKind)reader.ReadByte()}");
				break;
			}
			case VMOpCode.Anewarray:
			case VMOpCode.Checkcast:
			case VMOpCode.Instanceof:
			{
				builder.Append(get_constant(reader.ReadShort()));
				break;
			}
			case VMOpCode.Wide:
			{
				VMOpCode code = (VMOpCode)reader.ReadByte();

				builder.Append(InstructionName(code));
				builder.Append(' ');

				if (code == VMOpCode.Iinc)
				{
					builder.Append('{');
					builder.Append($"LF:{reader.ReadShort()}");
					builder.Append('}');
					builder.Append(' ');
					builder.Append($"{reader.ReadShort()}");
				}
				else
				{
					builder.Append($"{reader.ReadShort()}");
				}
				break;
			}
			case VMOpCode.Multianewarray:
			{
				builder.Append($"{reader.ReadShort()}");
				builder.Append(' ');
				builder.Append($"{reader.ReadByte()}");
				break;
			}
			case VMOpCode.GotoW:
			case VMOpCode.JsrW:
			{
				builder.Append($"{reader.ReadInt()}");
				break;
			}
			default:
				return;
		}
	}

}
