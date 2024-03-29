﻿using System.Diagnostics;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using Colussom;

namespace JDism;

public enum VMOpCode : short
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
	JsrW
}

public enum MethodReferenceKind : byte
{
	None = 0,
	GetField,
	GetStatic,
	PutField,
	PutStatic,
	InvokeVirtual,
	InvokeStatic,
	InvokeSpecial,
	NewInvokeSpecial,
	InvokeInterface
}

public enum JTypeType
{
	Void = 'V',
	// signed byte
	Byte = 'B',
	// utf16
	Char = 'C',
	Double = 'D',
	Float = 'F',
	Int = 'I',
	Long = 'J',
	Object = 'L',
	Short = 'S',
	Boolean = 'Z',
	Method,
}

public enum AttributeType : ushort
{
	None = 0,
	ConstantValue,
	Code,
	StackMapTable,
	Exceptions,
	BootstrapMethods,
	InnerClasses,
	EnclosingMethod,
	Synthetic,
	Signature,
	RuntimeVisibleAnnotations,
	RuntimeInvisibleAnnotations,
	RuntimeVisibleParameterAnnotations,
	RuntimeInvisibleParameterAnnotations,
	RuntimeVisibleTypeAnnotations,
	RuntimeInvisibleTypeAnnotations,
	AnnotationDefault,
	MethodParameters,
	SourceFile,
	SourceDebugExtension,
	LineNumberTable,
	LocalVariableTable,
	LocalVariableTypeTable,
	Deprecated,
	UserDefined = 0xffff,
}

[Flags]
public enum MethodAccessFlags
{
	None = 0,
	Public = 0x0001,
	Private = 0x0002,
	Protected = 0x0004,
	Static = 0x0008,
	Final = 0x0010,
	Synchronized = 0x0020,
	Bridge = 0x0040,
	VarArgs = 0x0080,
	Native = 0x0100,
	Abstract = 0x0400,
	Strict = 0x0800,
	Synthetic = 0x1000,
}

[Flags]
public enum FieldAccessFlags
{
	None = 0,
	Public = 0x0001,
	Private = 0x0002,
	Protected = 0x0004,
	Static = 0x0008,
	Final = 0x0010,

	Volatile = 0x0040,
	Transient = 0x0080,

	Synthetic = 0x1000,
	Enum = 0x4000,
}

[Flags]
public enum ClassAccessFlags
{
	None = 0,
	Public = 0x0001,


	Final = 0x0010,
	Super = 0x0020,

	Interface = 0x0200,
	Abstract = 0x0400,

	Synthetic = 0x1000,
	Annotation = 0x2000,
	Enum = 0x4000,
}


public static class AccessFlagsUtil
{

	public static void ToString(Action<string> loader, FieldAccessFlags accessFlags)
	{
		if (accessFlags.HasFlag(FieldAccessFlags.Public))
		{
			loader("public");
		}
		else if (accessFlags.HasFlag(FieldAccessFlags.Private))
		{
			loader("private");
		}
		else if (accessFlags.HasFlag(FieldAccessFlags.Protected))
		{
			loader("protected");
		}

		if (accessFlags.HasFlag(FieldAccessFlags.Static))
		{
			loader("static");
		}

		if (accessFlags.HasFlag(FieldAccessFlags.Final))
		{
			loader("final");
		}

		if (accessFlags.HasFlag(FieldAccessFlags.Volatile))
		{
			loader("volatile");
		}

		if (accessFlags.HasFlag(FieldAccessFlags.Transient))
		{
			loader("transient");
		}
	}

	public static void ToString(Action<string> loader, MethodAccessFlags accessFlags)
	{
		if (accessFlags.HasFlag(MethodAccessFlags.Public))
		{
			loader("public");
		}
		else if (accessFlags.HasFlag(MethodAccessFlags.Private))
		{
			loader("private");
		}
		else if (accessFlags.HasFlag(MethodAccessFlags.Protected))
		{
			loader("protected");
		}

		if (accessFlags.HasFlag(MethodAccessFlags.Static))
		{
			loader("static");
		}

		if (accessFlags.HasFlag(MethodAccessFlags.Final))
		{
			loader("final");
		}

		else if (accessFlags.HasFlag(MethodAccessFlags.Abstract))
		{
			loader("abstract");
		}
	}

	public static void ToString(Action<string> loader, ClassAccessFlags accessFlags)
	{
		if (accessFlags.HasFlag(ClassAccessFlags.Public))
		{
			loader("public");
		}

		if (accessFlags.HasFlag(ClassAccessFlags.Final))
		{
			loader("final");
		}

		if (accessFlags.HasFlag(ClassAccessFlags.Enum))
		{
			loader("enum");
		}
		else if (accessFlags.HasFlag(ClassAccessFlags.Interface))
		{
			loader("interface");
		}
		else if (accessFlags.HasFlag(ClassAccessFlags.Abstract))
		{
			loader("abstract class");
		}
		else
		{
			loader("class");
		}
	}


}


public class JType
{
	public JTypeType Type;
	public ushort ArrayDimension = 0;
	public string ObjectType = "";
	public JType[] MethodParameters = [];
	public JType ReturnType;
	public JType[] Generics = [];

	public uint ReadLength { get; init; }

	public JType()
	{
		Type = JTypeType.Object;
	}

	public JType(string encoded_type)
	{
		if (encoded_type == "") return;

		int strlen = encoded_type.Length;

		// check for arrays
		ArrayDimension = 0;
		for (int i = 0; i < strlen; i++)
		{
			if (encoded_type[i] != '[')
				break;
			ArrayDimension++;
		}

		// removed prefixed array decorator "[[["
		encoded_type = encoded_type.Substring(ArrayDimension);

		ReadLength = ArrayDimension + 1U;

		switch (encoded_type[0])
		{
			case 'V':
			{
				Type = JTypeType.Void;
				break;
			}
			case 'B':
			{
				Type = JTypeType.Byte;
				break;
			}
			case 'C':
			{
				Type = JTypeType.Char;
				break;
			}
			case 'D':
			{
				Type = JTypeType.Double;
				break;
			}
			case 'F':
			{
				Type = JTypeType.Float;
				break;
			}
			case 'I':
			{
				Type = JTypeType.Int;
				break;
			}
			case 'J':
			{
				Type = JTypeType.Long;
				break;
			}
			case 'S':
			{
				Type = JTypeType.Short;
				break;
			}
			case 'Z':
			{
				Type = JTypeType.Boolean;
				break;
			}
			// TYPE
			case 'L':
			{
				Type = JTypeType.Object;

				int semicolon_pos = encoded_type.IndexOf(';');
				if (semicolon_pos == -1)
				{
					throw new InvalidDataException($"Object Type Does Not have The Terminating Semicolon: \"{encoded_type}\"");
				}

				ObjectType = encoded_type.Substring(1, semicolon_pos - 1);

				// The prefixed 'L' and array decorators are handled before the switch
				ReadLength += (uint)ObjectType.Length + 1U;

				break;
			}
			// method
			case '(':
			{
				Type = JTypeType.Method;

				int end_para = encoded_type.IndexOf(')', 1);
				if (end_para == -1)
				{
					throw new InvalidDataException($"Ill-Formed Method JType: \"{encoded_type}\"");
				}

				// check if the parameters are empty (e.g. '()V')
				string parameters_typing = end_para == 1 ? "" : encoded_type.Substring(1, end_para - 1);
				string return_type = encoded_type.Substring(end_para + 1);

				// the return_type read length might vary for it's just the rest of the string
				// and in need for parsing
				ReadLength += 2U;

				try
				{
					ReturnType = new JType(return_type);
				}
				catch (Exception)
				{
					Console.WriteLine("Error While Parsing Return Type For Method JType:");
					Console.WriteLine($"Encoded type: \"{encoded_type}\"");
					Console.WriteLine($"Return type: \"{return_type}\"");
					throw;
				}

				ReadLength += ReturnType.ReadLength;

				List<JType> parameters = new(8);

				for (uint i = 0U; i < parameters_typing.Length;)
				{
					// TODO: CATCH EXCEPTIONS FOR MORE DETAILS
					JType type = new(parameters_typing.Substring((int)i));

					// only for parsing, read length already adjusted after parameters_typing is defined
					i += type.ReadLength;
					ReadLength += type.ReadLength;

					parameters.Add(type);
				}

				MethodParameters = parameters.ToArray();

				break;
			}
			default:
			{
				throw new InvalidDataException($"Invalid Encoding For JType: \"{encoded_type}\"");
			}
		}

		// shouldn't be bigger then the encoded length
		if (ReadLength >= encoded_type.Length)
		{
			return;
		}

		encoded_type = encoded_type.Substring((int)ReadLength);

		if (!string.IsNullOrEmpty(encoded_type) && encoded_type[0] == '<')
		{
			int end_arrow = encoded_type.IndexOf('>', 1);
			if (end_arrow == -1)
			{
				throw new InvalidDataException($"Invalid generics {encoded_type}");
			}

			string generics_raw = end_arrow == 1 ? "" : encoded_type.Substring(1, end_arrow - 1);

			ReadLength += (uint)generics_raw.Length + 2u;

			List<JType> generics = new(8);

			for (uint i = 0U; i < generics_raw.Length;)
			{
				// TODO: CATCH EXCEPTIONS FOR MORE DETAILS
				JType type = new(generics_raw.Substring((int)i));

				// only for parsing, read length already adjusted after parameters_typing is defined
				i += type.ReadLength;

				generics.Add(type);
			}

			Generics = generics.ToArray();
		}

	}

	public JType(JTypeType type, ushort arr_d, string obj_type)
	{
		Type = type;
		ArrayDimension = arr_d;
		ObjectType = obj_type;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new(ArrayDimension * 2 + 2);

		switch (Type)
		{
			case JTypeType.Void:
				stringBuilder.Append("void");
				break;
			case JTypeType.Byte:
				stringBuilder.Append("byte");
				break;
			case JTypeType.Char:
				stringBuilder.Append("char");
				break;
			case JTypeType.Double:
				stringBuilder.Append("double");
				break;
			case JTypeType.Float:
				stringBuilder.Append("float");
				break;
			case JTypeType.Int:
				stringBuilder.Append("int");
				break;
			case JTypeType.Long:
				stringBuilder.Append("long");
				break;
			case JTypeType.Object:
				stringBuilder.Append(ObjectType);
				break;
			case JTypeType.Short:
				stringBuilder.Append("short");
				break;
			case JTypeType.Boolean:
				stringBuilder.Append("boolean");
				break;
			case JTypeType.Method:
				// TODO?
				stringBuilder.Append($"Function<{ReturnType?.ToString()}, ...>");
				break;
			default:
				stringBuilder.Append("Object");
				break;
		}

		if (Generics is not null && Generics.Length != 0)
		{
			stringBuilder.Append('<');
			for (int i = 0; i < Generics.Length; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}

				stringBuilder.Append(Generics[i].ToString());
			}
			stringBuilder.Append('>');
		}

		for (ushort i = 0; i < ArrayDimension; i++)
			stringBuilder.Append("[]");

		return stringBuilder.ToString();
	}

	// cleans the type path, for example java/lang/object -> object or net/example/com/SomeType -> SomeType
	// subclasses 'Type$SubType' will be cleaned to 'Type.SubType'
	public static string ShortenTypeName(string typePath)
	{
		int last_path_sep = typePath.LastIndexOf('/');
		if (last_path_sep != -1)
		{
			typePath = typePath.Substring(last_path_sep + 1);
		}
		return typePath.Replace('$', '.');
	}

}

public enum ConstantType
{
	None = 0,
	String,
	Integer = 3,
	Float,
	Long,
	Double,
	Class,
	StringReference,
	FieldReference,
	MethodReference,
	InterfaceMethodReference,
	NameTypeDescriptor,
	MethodHandle = 15,
	MethodType,
	Dynamic,
	InvokeDynamic,
	//Module,
	//Package

}

public class Constant
{
	public ConstantType type;

	public int IntegerValue { get => (int)long_int; set => long_int = value; }
	public long LongValue { get => long_int; set => long_int = value; }

	public float FloatValue { get => (float)double_float; set => double_float = value; }
	public double DoubleValue { get => double_float; set => double_float = value; }

	public ushort ClassIndex { get => index1; set => index1 = value; }
	public ushort StringIndex { get => index1; set => index1 = value; }
	public ushort NameTypeIndex { get => index2; set => index2 = value; }
	public ushort NameIndex { get => index1; set => index1 = value; }
	public ushort DescriptorIndex { get => index2; set => index2 = value; }
	public MethodReferenceKind ReferenceKind { get => (MethodReferenceKind)index1; set => index1 = (ushort)value; }
	public ushort ReferenceIndex { get => index2; set => index2 = value; }

	public ushort BootstrapMethodAttrIndex { get => index1; set => index1 = value; }

	public string String { get; set; } = "";

	private ushort index1;
	private ushort index2;
	private long long_int;
	private double double_float;

	public bool IsDoubleSlotted()
	{
		return type == ConstantType.Double || type == ConstantType.Long;
	}

}

public class Attribute
{
	public struct ConstantValueInfo
	{
		public ushort Index;
	}
	public struct SignatureInfo
	{
		public ushort Index;
		public string Value;
	}
	public record ExceptionRecord(ushort StartPc, ushort EndPc, ushort HandlerPc, ushort CatchType);

	public struct CodeInfo
	{

		public ushort MaxStack;
		public ushort MaxLocals;
		public byte[] ByteCode;

		public ExceptionRecord[] ExceptionRecords;
		public Attribute[] Attributes;
	}

	public record VerificationTypeRecord(byte Type, ushort Index);

	public struct StackMapFrameInfo
	{
		public byte Type = 255;
		public ushort OffsetDelta = 0;
		public VerificationTypeRecord[] Locals = [];
		public VerificationTypeRecord[] Stack = [];

		public StackMapFrameInfo()
		{
		}
	}

	public AttributeType Type;
	public ushort NameIndex;
	public string Name = "";



	public ConstantValueInfo ConstantValue;
	public SignatureInfo Signature;
	public CodeInfo Code;
	public StackMapFrameInfo[] StackMapTable = [];

	// for custom defined attributes
	public byte[] _data = [];
}

public class Field
{
	public FieldAccessFlags AccessFlags;
	public ushort NameIndex;
	public string Name = "";
	public ushort DescriptorIndex;
	public JType ValueType = new();
	public Attribute[] Attributes = [];
}
public class Method
{
	public MethodAccessFlags AccessFlags;
	public ushort NameIndex;
	public string Name = "";
	public ushort DescriptorIndex;
	public JType MethodType = new();
	public Attribute[] Attributes = [];
}

public struct ConstantError(ushort index, string msg = "")
{
	public ushort Index = index;
	public string Message = msg;
}

public struct Instruction
{
	public VMOpCode OpCode;

	private ushort _index1;
	private uint _index2;

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
}

public class Disassembly
{
	public ushort VersionMinor;
	public ushort VersionMajor;
	public Constant[] Constants;
	public ClassAccessFlags AccessFlags;
	public ushort ThisClass;
	public ushort SuperClass;
	public ushort[] Interfaces = [];
	public Field[] Fields = [];
	public Method[] Methods = [];
	public Attribute[] Attributes = [];

	// constant index X (X for the class file) is Constant[ConstantIndexRoutingTable[X]]
	public ushort[] ConstantIndexRoutingTable = [];

	public Disassembly()
	{
		Constants = new Constant[0];
	}

	public static string DecryptType(string type_desc)
	{
		ushort array_dim = 0;
		string base_type = "";
		for (int i = 0; i < type_desc.Length; i++)
		{
			if (type_desc[i] == '[')
			{
				array_dim++;
				continue;
			}
			base_type = type_desc.Substring(i);
			break;
		}

		if (base_type == "")
		{
			return "";
		}
		StringBuilder builder = new StringBuilder(base_type.Length + (array_dim * 2) + 1);
		builder.Append(base_type.Replace('/', '.').Replace('$', '.'));
		for (int i = 0; i < array_dim; i++)
		{
			builder.Append("[]");
		}

		return builder.ToString();
	}

	public string GenerateSource()
	{
		StringBuilder builder = new(4096);

		string class_name = JType.ShortenTypeName(Constants[ThisClass - 1].String);
		string super_name = JType.ShortenTypeName(Constants[SuperClass - 1].String);

		builder.Append("class ");
		builder.Append(class_name).Append(' ');

		if (super_name != "Object")
		{
			builder.Append("extends ");
			builder.Append(super_name).Append(' ');
		}

		builder.Append("{\n");

		if (Fields is not null)
		{
			foreach (Field field in Fields)
			{
				if (field.Attributes is not null)
					builder.Append(AttributesToAnnotations(field.Attributes));

				AccessFlagsUtil.ToString((string s) => builder.Append(s).Append(' '), field.AccessFlags);
				builder.Append(field.ValueType is not null ? JType.ShortenTypeName(field.ValueType.ToString()) : "NULL").Append(' ');
				builder.Append(field.Name).Append(";\n");
			}
		}

		if (Methods is not null)
		{
			foreach (Method method in Methods)
			{
				if (method.Attributes is not null)
					builder.Append(AttributesToAnnotations(method.Attributes));

				// TODO: annotations
				AccessFlagsUtil.ToString((string s) => builder.Append(s).Append(' '), method.AccessFlags);


				// name
				if (method.Name == "<init>")
				{
					// no return for constructor
					builder.Append(class_name);
				}
				else if (method.Name == "<clinit>")
				{
					// no return for constructor
					builder.Append('~').Append(class_name);
				}
				else
				{
					// return
					builder.Append(JType.ShortenTypeName(method.MethodType.ReturnType.ToString())).Append(' ');
					bool internal_name = method.Name.Contains('$');
					if (internal_name)
					{
						builder.Append("__");
					}

					builder.Append(method.Name.Replace('$', '_'));
				}
				// parameters
				builder.Append('(');

				for (int i = 0; i < method.MethodType.MethodParameters.Length; i++)
				{
					JType method_param = method.MethodType.MethodParameters[i];
					if (i > 0)
					{
						builder.Append(", ");
					}

					builder.Append(JType.ShortenTypeName(method_param.ToString()));
					builder.Append(' ');
					builder.Append($"parameter_{i}");
				}

				builder.Append(')').Append(";\n");

			}
		}

		builder.Append("}");

		return builder.ToString();
	}

	public void BuildCIRT()
	{
		if (Constants is null)
		{
			return;
		}

		ConstantIndexRoutingTable = new ushort[Constants.Length + 1];
		int offset = 1;
		for (int i = 0; i < ConstantIndexRoutingTable.Length; i++)
		{
			ConstantIndexRoutingTable[i] = (ushort)(i + offset);
			//if (Constants[i].type == ConstantType.Double || Constants[i].type == ConstantType.Long)
			//{
			//	offset++;
			//}
		}
	}

	public ConstantError[] ValidateConstantTable()
	{
		if (Constants is null)
			return Array.Empty<ConstantError>();
		List<ConstantError> errors = new(2 + (Constants.Length >> 3));
		ushort len = (ushort)Constants.Length;

		for (ushort i = 0; i < len; i++)
		{
			Constant constant = Constants[i];

			if (constant is null)
			{
				errors.Add(new(i, "Null constant"));
				continue;
			}

			switch (constant.type)
			{
				case ConstantType.Class:
				{
					if (!IsConstantOfType(constant.NameIndex, ConstantType.String))
					{
						errors.Add(new(i, $"Constant Index {constant.NameIndex} should be a valid index to a string constant"));
					}
					break;
				}
				case ConstantType.FieldReference:
				case ConstantType.MethodReference:
				case ConstantType.InterfaceMethodReference:
				{
					if (!IsConstantOfType(constant.ClassIndex, ConstantType.Class))
					{
						errors.Add(new(i, $"Constant Index {constant.ClassIndex} should be a valid index to a class constant"));
					}

					if (!IsConstantOfType(constant.NameTypeIndex, ConstantType.NameTypeDescriptor))
					{
						errors.Add(new(i, $"Constant Index {constant.NameTypeIndex} should be a valid index to a name type descriptor constant"));
					}

					break;
				}
				case ConstantType.StringReference:
				{
					if (!IsConstantOfType(constant.StringIndex, ConstantType.String))
					{
						errors.Add(new(i, $"Constant Index {constant.StringIndex} should be a valid index to a string constant"));
					}

					break;
				}
				case ConstantType.NameTypeDescriptor:
				{
					if (!IsConstantOfType(constant.NameIndex, ConstantType.String))
					{
						errors.Add(new(i, $"Constant Index {constant.NameIndex} should be a valid index to a string constant (name index)"));
					}

					if (!IsConstantOfType(constant.NameIndex, ConstantType.String))
					{
						errors.Add(new(i, $"Constant Index {constant.NameIndex} should be a valid index to a string constant (descriptor index)"));
					}

					break;
				}
				case ConstantType.MethodHandle:
				{
					switch (constant.ReferenceKind)
					{
						case MethodReferenceKind.GetField:
						case MethodReferenceKind.GetStatic:
						case MethodReferenceKind.PutField:
						case MethodReferenceKind.PutStatic:
						{
							if (!IsConstantOfType(constant.ReferenceIndex, ConstantType.FieldReference))
							{
								errors.Add(new(i, $"Constant Index {constant.ReferenceIndex} should be a valid index to a field reference constant"));
							}
							break;
						}
						case MethodReferenceKind.InvokeVirtual:
						case MethodReferenceKind.NewInvokeSpecial:
						{
							if (!IsConstantOfType(constant.ReferenceIndex, ConstantType.MethodReference))
							{
								errors.Add(new(i, $"Constant Index {constant.ReferenceIndex} should be a valid index to a method reference constant"));
							}
							else if (constant.ReferenceKind != MethodReferenceKind.NewInvokeSpecial)
							{
								// TODO: CHECK FOR VALID METHOD NAME
							}
							else
							{
								// TODO: CHECK FOR VALID NEW METHOD NAME
							}

							break;
						}
						case MethodReferenceKind.InvokeStatic:
						case MethodReferenceKind.InvokeSpecial:
						{
							bool ref_index_valid_55 = IsConstantOfType(constant.ReferenceIndex, ConstantType.MethodReference);
							if (VersionMajor >= 56)
							{
								if (!(ref_index_valid_55 || IsConstantOfType(constant.ReferenceIndex, ConstantType.InterfaceMethodReference)))
								{
									errors.Add(new(i, $"Constant Index {constant.ReferenceIndex} should be a valid index to a method reference or an interface method reference constant (v56)"));
								}
								else
								{
									// TODO: CHECK FOR VALID METHOD NAME
								}

								break;
							}

							if (!ref_index_valid_55)
							{
								errors.Add(new(i, $"Constant Index {constant.ReferenceIndex} should be a valid index to a method reference constant (pre v55)"));
							}
							else
							{
								// TODO: CHECK FOR VALID METHOD NAME
							}

							break;
						}
						case MethodReferenceKind.InvokeInterface:
						{
							if (!IsConstantOfType(constant.ReferenceIndex, ConstantType.InterfaceMethodReference))
							{
								errors.Add(new(i, $"Constant Index {constant.ReferenceIndex} should be a valid index to an interface method reference constant"));
							}
							else
							{
								// TODO: CHECK FOR VALID METHOD NAME
							}

							break;
						}
					}

					break;
				}
				case ConstantType.MethodType:
				{
					if (!IsConstantOfType(constant.DescriptorIndex, ConstantType.String))
					{
						errors.Add(new(i, $"Constant Index {constant.DescriptorIndex} should be a valid index to a string constant (method type)"));
					}

					break;
				}
				case ConstantType.InvokeDynamic:
				{
					// TODO: CHECK FOR THE BOOTSTRAP METHOD BETWEEN THE BOOTSTRAP METHODS OF THIS CLASS

					if (!IsConstantOfType(constant.NameTypeIndex, ConstantType.NameTypeDescriptor))
					{
						errors.Add(new(i, $"Constant Index {constant.NameTypeIndex} should be a valid index to a name type descriptor constant (method type)"));
					}

					break;
				}
				case ConstantType.String:
				case ConstantType.Integer:
				case ConstantType.Long:
				case ConstantType.Float:
				case ConstantType.Double:
				{
					// might check for strings later
					// nothing to do here
					break;
				}
				default:
				{
					errors.Add(new(i, $"Invalid constant type {(int)constant.type}"));
					break;
				}
			}

			if (constant.IsDoubleSlotted())
				i++;

		}

		return errors.ToArray();
	}

	public void PostProcess()
	{
		SetupAttributes();

		if (Fields is not null)
		{
			foreach (Field field in Fields)
			{
				SetupField(field);
			}
		}

		if (Methods is not null)
		{
			foreach (Method method in Methods)
			{
				SetupMethod(method);
			}
		}
	}

	private string AttributesToAnnotations(Attribute[] attributes)
	{
		StringBuilder sb = new StringBuilder();

		foreach (Attribute attribute in attributes)
		{

			sb.Append('@').Append(attribute.Name);
			sb.Append('(');

			if (attribute.Type == AttributeType.Code)
			{
				sb.Append("locals=").Append(attribute.Code.MaxLocals);
				sb.Append(", ");
				sb.Append("stacksize=").Append(attribute.Code.MaxStack);
				sb.Append(", ");
				sb.Append("bytecode=[");
				for (int i = 0; i < attribute.Code.ByteCode.Length; i++)
				{
					if (i > 0)
					{
						sb.Append(", ");
					}

					sb.Append($"0x{attribute.Code.ByteCode[i]:X}");
				}
				sb.Append(']');
			}
			else if (attribute.Type == AttributeType.ConstantValue)
			{
				Constant c = Constants[attribute.ConstantValue.Index - 1];
				if (c is null)
				{
					sb.Append("null");
				}

				else if (c.type == ConstantType.Integer)
				{
					sb.Append(c.IntegerValue);
				}
				else if (c.type == ConstantType.Long)
				{
					sb.Append(c.LongValue);
				}
				else if (c.type == ConstantType.Float)
				{
					sb.Append(c.FloatValue);
				}
				else if (c.type == ConstantType.Double)
				{
					sb.Append(c.DoubleValue);
				}
				else
				{
					sb.Append('"').Append(c.String).Append('"');
				}

			}
			else if (attribute.Type == AttributeType.Signature)
			{
				sb.Append("index=").Append(attribute.Signature.Index);
				sb.Append(", ");
				sb.Append('"').Append(attribute.Signature.Value).Append('"');
			}
			else
			{
				for (int i = 0; i < attribute._data.Length; i++)
				{
					if (i > 0)
					{
						sb.Append(", ");
					}

					sb.Append($"0x{attribute._data[i]:X}");
				}
			}

			sb.Append(')');
			sb.Append('\n');
		}

		return sb.ToString();
	}

	private bool IsConstantOfType(ushort index, ConstantType type)
	{
		// the class constant table indices start at 1
		index--;
		if (index >= Constants.Length)
			return false;
		return Constants[index].type == type;
	}

	private bool IsValidNonNewInvoke(string name)
	{
		return name != "<init>" && name != "<clinit>";
	}

	private void SetupField(Field field)
	{
		// TODO: check for errors/out of range indices
		field.Name = Constants[field.NameIndex - 1].String;
		field.ValueType = new JType(Constants[field.DescriptorIndex - 1].String);
	}

	private void SetupMethod(Method method)
	{
		// TODO: check for errors/out of range indices
		method.Name = Constants[method.NameIndex - 1].String;
		method.MethodType = new JType(Constants[method.DescriptorIndex - 1].String);
	}

	private void LoadCodeInfo(Attribute attribute)
	{
		ByteReader reader = new(attribute._data, 0, Endianness.Big);

		attribute.Code.MaxStack = reader.ReadUShort();
		attribute.Code.MaxLocals = reader.ReadUShort();

		int code_len = reader.ReadInt();
		if (code_len <= 0)
		{
			throw new InvalidDataException($"code length can't be less or equal to zero: {code_len}");
		}

		if (reader.SpaceLeft < code_len)
		{
			throw new InvalidDataException();
		}

		attribute.Code.ByteCode = reader.Read(code_len);


		ushort exception_table_len = reader.ReadUShort();
		attribute.Code.ExceptionRecords = new Attribute.ExceptionRecord[exception_table_len];

		Attribute.ExceptionRecord ReadExceptionRecord()
		{
			return new(reader.ReadUShort(), reader.ReadUShort(), reader.ReadUShort(), reader.ReadUShort());
		}

		for (uint i = 0; i < exception_table_len; i++)
		{
			attribute.Code.ExceptionRecords[i] = ReadExceptionRecord();
		}

		ushort subattrs_count = reader.ReadUShort();

		attribute.Code.Attributes = new Attribute[subattrs_count];

		// TODO: load the sub attributes

	}

	private void LoadAttributes(Attribute[] attrs)
	{
		for (int i = 0; i < attrs.Length; i++)
		{
			Attribute attribute = attrs[i];
			ByteReader reader = new(attribute._data);

			// TODO: check the name index
			attribute.Name = Constants[attribute.NameIndex - 1].String;

			if (sAttributeTypeNames.TryGetValue(attribute.Name, out AttributeType value))
			{
				attribute.Type = value;
			}
			else
			{
				attribute.Type = AttributeType.UserDefined;
			}

			if (attribute.Type == AttributeType.ConstantValue)
			{
				if (attribute._data.Length != 2)
				{
					throw new InvalidDataException("constant value attribute should have 2 bytes of data");
				}

				attribute.ConstantValue.Index = (ushort)((attribute._data[0] << 8) | attribute._data[1]);
			}
			else if (attribute.Type == AttributeType.Code)
			{
				if (attribute._data.Length < 8)
				{
					throw new InvalidDataException($"code attribute should have atleast 8 bytes of data, but it has {attribute._data.Length}");
				}

				LoadCodeInfo(attribute);

			}
			else if (attribute.Type == AttributeType.Signature)
			{
				if (attribute._data.Length != 2)
				{
					throw new InvalidDataException($"signature attribute should have 2 bytes of data, but it has {attribute._data.Length}");
				}

				attribute.Signature.Index = reader.ReadUShort();

				attribute.Signature.Value = Constants[attribute.Signature.Index - 1].String;
			}

		}
	}

	private void SetupAttributes()
	{
		if (Fields is not null)
		{
			foreach (Field field in Fields)
			{
				if (field.Attributes is null)
					continue;
				LoadAttributes(field.Attributes);
			}
		}

		if (Methods is not null)
		{
			foreach (Method method in Methods)
			{
				if (method.Attributes is null)
					continue;
				LoadAttributes(method.Attributes);
			}
		}
	}

	private static Dictionary<string, AttributeType> GetAttributeTypeNames()
	{
		Dictionary<string, AttributeType> dict = new();
		string[] Names =
		[
		"ConstantValue",
			"Code",
			"StackMapTable",
			"Exceptions",
			"BootstrapMethods",
			"InnerClasses",
			"EnclosingMethod",
			"Synthetic",
			"Signature",
			"RuntimeVisibleAnnotations",
			"RuntimeInvisibleAnnotations",
			"RuntimeVisibleParameterAnnotations",
			"RuntimeInvisibleParameterAnnotations",
			"RuntimeVisibleTypeAnnotations",
			"RuntimeInvisibleTypeAnnotations",
			"AnnotationDefault",
			"MethodParameters",
			"SourceFile",
			"SourceDebugExtension",
			"LineNumberTable",
			"LocalVariableTable",
			"LocalVariableTypeTable",
			"Deprecated"
		];

		for (int i = 0; i < Names.Length; i++)
		{
			dict[Names[i]] = (AttributeType)(i + 1);
		}

		return dict;
	}

	private static readonly Dictionary<string, AttributeType> sAttributeTypeNames = GetAttributeTypeNames();
}

static public class JDisassembler
{

	public class JReader
	{
		public JReader(BinaryReader br)
		{
			Reader = br;
		}

		public byte Read()
		{
			return Reader.ReadByte();
		}

		public ushort ReadU16BE()
		{
			ushort i = Reader.ReadUInt16();
			return (ushort)((i >> 8) | (i << 8));
		}

		public uint ReadU32BE()
		{
			uint i = Reader.ReadUInt32();
			return (i >> 24) | (((i >> 16) & 0xff) << 8) | (((i >> 8) & 0xff) << 16) | ((i & 0xff) << 24);
		}

		public ulong ReadU64BE()
		{
			ulong i = Reader.ReadUInt64();
			return
				(i >> 56)
		| (((i >> 48) & 0xff) << 8)
		| (((i >> 40) & 0xff) << 16)
		| (((i >> 32) & 0xff) << 24)
		| (((i >> 24) & 0xff) << 32)
		| (((i >> 16) & 0xff) << 40)
		| (((i >> 8) & 0xff) << 48)
		| ((i & 0xff) << 56);
		}

		public float ReadIEEE754()
		{
			return Reader.ReadSingle();
		}

		public double ReadDoubleIEEE754()
		{
			return Reader.ReadDouble();
		}

		public Constant ReadConstant()
		{
			Constant constant = new()
			{
				type = (ConstantType)Read()
			};

			switch (constant.type)
			{
				case ConstantType.String:
				{
					ushort len = ReadU16BE();
					byte[] bytes = new byte[len];
					if (Reader.Read(bytes, 0, len) < len)
					{
						//! NOT ENOUGH BYTES
					}

					constant.String = Encoding.UTF8.GetString(bytes);
					Console.WriteLine($"READ CONSTANT UTF8: \"{constant.String}\"");
					break;
				}
				case ConstantType.Integer:
				{
					constant.IntegerValue = (int)ReadU32BE();
					Console.WriteLine($"READ CONSTANT INT: {constant.IntegerValue}");
					break;
				}
				case ConstantType.Float:
				{
					constant.LongValue = (int)ReadIEEE754();
					Console.WriteLine($"READ CONSTANT LONG: {constant.LongValue}");
					break;
				}
				case ConstantType.Long:
				{
					constant.FloatValue = (int)ReadU64BE();
					Console.WriteLine($"READ CONSTANT FLOAT: {constant.FloatValue}");
					break;
				}
				case ConstantType.Double:
				{
					constant.DoubleValue = (int)ReadDoubleIEEE754();
					Console.WriteLine($"READ CONSTANT DOUBLE: {constant.DoubleValue}");
					break;
				}
				case ConstantType.Class:
				{
					constant.NameIndex = ReadU16BE();
					Console.WriteLine($"READ CONSTANT CLASS: NAME_INDEX={constant.NameIndex}");
					break;
				}
				case ConstantType.FieldReference:
				case ConstantType.MethodReference:
				case ConstantType.InterfaceMethodReference:
				{
					constant.ClassIndex = ReadU16BE();
					constant.NameTypeIndex = ReadU16BE();
					Console.WriteLine($"READ CONSTANT F/M/IM-REF: CLASS_INDEX={constant.ClassIndex} NAMETYPE={constant.NameTypeIndex}");
					break;
				}
				case ConstantType.StringReference:
				{
					constant.StringIndex = ReadU16BE();
					Console.WriteLine($"READ CONSTANT STRING-REF: STRING_INDEX={constant.StringIndex}");
					break;
				}
				case ConstantType.NameTypeDescriptor:
				{
					constant.NameIndex = ReadU16BE();
					constant.DescriptorIndex = ReadU16BE();
					Console.WriteLine($"READ CONSTANT NAMETYPE-DESC: NAMEINDEX={constant.NameIndex} DESCINDEX={constant.DescriptorIndex}");
					break;
				}
				case ConstantType.MethodHandle:
				{
					constant.ReferenceKind = (MethodReferenceKind)Read();
					constant.ReferenceIndex = ReadU16BE();
					Console.WriteLine($"READ CONSTANT MHANDLE: REFKIND={constant.ReferenceKind} REFINDEX={constant.ReferenceIndex}");
					break;
				}
				case ConstantType.MethodType:
				{
					constant.DescriptorIndex = ReadU16BE();
					Console.WriteLine($"READ CONSTANT MTYPE: DESCINDEX={constant.DescriptorIndex}");
					break;
				}
				case ConstantType.InvokeDynamic:
				{
					constant.BootstrapMethodAttrIndex = ReadU16BE();
					constant.NameTypeIndex = ReadU16BE();
					Console.WriteLine($"READ CONSTANT INVOKEDYN: BMAI={constant.BootstrapMethodAttrIndex} NAMETYPE={constant.NameTypeIndex}");
					break;
				}
			}

			return constant;
		}

		public Attribute ReadAttribute()
		{
			Attribute attributeInfo = new()
			{
				NameIndex = ReadU16BE()
			};

			uint data_len = ReadU32BE();
			attributeInfo._data = Reader.ReadBytes((int)data_len);
			if (attributeInfo._data.Length < data_len)
			{
				//! NOT ENOUGH BYTES
			}
			return attributeInfo;
		}

		public Field ReadField()
		{
			Field field = new()
			{
				AccessFlags = (FieldAccessFlags)ReadU16BE(),
				NameIndex = ReadU16BE(),
				DescriptorIndex = ReadU16BE()
			};

			field.Attributes = new Attribute[ReadU16BE()];

			for (uint i = 0; i < field.Attributes.Length; i++)
			{
				field.Attributes[i] = ReadAttribute();
			}

			return field;
		}

		public Method ReadMethod()
		{
			Method method = new()
			{
				AccessFlags = (MethodAccessFlags)ReadU16BE(),
				NameIndex = ReadU16BE(),
				DescriptorIndex = ReadU16BE()
			};

			method.Attributes = new Attribute[ReadU16BE()];

			for (uint i = 0; i < method.Attributes.Length; i++)
			{
				method.Attributes[i] = ReadAttribute();
			}

			return method;
		}

		public BinaryReader Reader { get; init; }
	}

	public static string Decompile(BinaryReader stream, out Disassembly disassembly)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}

		long MemUsagePrivate = 0;
		long MemUsagePaged = 0;

		using (Process process = Process.GetCurrentProcess())
		{
			MemUsagePrivate = process.PrivateMemorySize64;
			MemUsagePaged = process.PagedMemorySize64;
		}


		disassembly = new Disassembly();
		JReader reader = new(stream);

		uint signature = reader.ReadU32BE();


		// invalid sign
		if (signature != 0xCAFEBABE)
		{
			return "invalid signature";
		}

		disassembly.VersionMinor = reader.ReadU16BE();
		disassembly.VersionMajor = reader.ReadU16BE();

		disassembly.Constants = new Constant[reader.ReadU16BE() - 1];

		for (uint i = 0; i < disassembly.Constants.Length; i++)
		{
			Console.Write($"READING CONST[{i + 1}] ");
			Constant constant = reader.ReadConstant();
			disassembly.Constants[i] = constant;
			if (constant.type == ConstantType.Double || constant.type == ConstantType.Long)
			{
				i++;
			}

			switch (constant.type)
			{
				case ConstantType.Class:
				case ConstantType.FieldReference:
				case ConstantType.MethodReference:
				case ConstantType.InterfaceMethodReference:
				case ConstantType.StringReference:
				{
					constant.String = disassembly.Constants[constant.NameIndex - 1].String;
					Console.WriteLine($"CONSTANT STRING: \"{constant.String}\"");
					break;
				}
				case ConstantType.NameTypeDescriptor:
				{
					constant.String = $"{disassembly.Constants[constant.DescriptorIndex - 1].String} {disassembly.Constants[constant.NameIndex - 1].String}";
					Console.WriteLine($"CONSTANT STRING: \"{constant.String}\"");
					break;
				}
				default: break;
			}

		}

		disassembly.BuildCIRT();

#if DEBUG
		ConstantError[] errors = disassembly.ValidateConstantTable();

		for (uint i = 0; i < errors.Length; i++)
		{
			Console.WriteLine($"Error On Constant {errors[i].Index}: {errors[i].Message}");
		}
#endif

		disassembly.AccessFlags = (ClassAccessFlags)reader.ReadU16BE();
		disassembly.ThisClass = reader.ReadU16BE();
		disassembly.SuperClass = reader.ReadU16BE();
		Console.WriteLine($"Class name: {disassembly.Constants[disassembly.ThisClass - 1].String} : {disassembly.Constants[disassembly.SuperClass - 1].String}");


		disassembly.Interfaces = new ushort[reader.ReadU16BE()];

		for (uint i = 0; i < disassembly.Interfaces.Length; i++)
		{
			disassembly.Interfaces[i] = reader.ReadU16BE();
		}


		disassembly.Fields = new Field[reader.ReadU16BE()];

		for (uint i = 0; i < disassembly.Fields.Length; i++)
		{
			disassembly.Fields[i] = reader.ReadField();
		}


		disassembly.Methods = new Method[reader.ReadU16BE()];

		for (uint i = 0; i < disassembly.Methods.Length; i++)
		{
			disassembly.Methods[i] = reader.ReadMethod();
		}



		disassembly.Attributes = new Attribute[reader.ReadU16BE()];

		for (uint i = 0; i < disassembly.Attributes.Length; i++)
		{
			Attribute attr = reader.ReadAttribute();
			attr.Name = disassembly.Constants[attr.NameIndex - 1].String;
			disassembly.Attributes[i] = attr;
			Console.WriteLine($"attribute \"{attr.Name}\": {attr._data.Length} bytes");
		}

		disassembly.PostProcess();
		string text = disassembly.GenerateSource();
		File.WriteAllText("output.java", text);

		using (Process process = Process.GetCurrentProcess())
		{
			Console.WriteLine($"memory usage [private]: {process.PrivateMemorySize64 / (1 << 20)}MB");
			Console.WriteLine($"memory usage [paged]: {process.PagedMemorySize64 / (1 << 20)}MP");
			Console.WriteLine($"disassembly memory usage [private]: {(process.PrivateMemorySize64 - MemUsagePrivate) / (1 << 20)}MB");
			Console.WriteLine($"disassembly memory usage [paged]: {(process.PagedMemorySize64 - MemUsagePaged) / (1 << 20)}MP");
		}



		return string.Empty;
	}

}

static public class JSL
{

}
