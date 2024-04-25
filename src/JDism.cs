using System.Diagnostics;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using Colussom;

namespace JDism;


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


public static class AccessFlagsUtility
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


	public JType()
	{
		Type = JTypeType.Object;
	}

	public JType(JStringReader reader)
	{
		if (reader.EOF) return;

		if (reader.String.StartsWith("<init>") || reader.String.StartsWith("<cinit>"))
		{
			Type = JTypeType.Method;
			Console.WriteLine($"found a special function: {reader.String}");
			return;
		}

		// check for arrays
		ArrayDimension = (ushort)reader.SkipCount('[');

		switch (reader.Read())
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

				int semicolon_pos = reader.IndexOf(';');
				if (semicolon_pos == -1)
				{
					throw new InvalidDataException($"\nObject Type Does Not have The Terminating Semicolon: \"{reader}\"");
				}

				ObjectType = reader.ReadUntil(c => c == ';');
				reader.Skip(); // skip ';'



				break;
			}
			// method
			case '(':
			{
				Type = JTypeType.Method;

				int end_para = reader.IndexOf(')');
				// no closing parenthesis
				if (end_para == -1)
				{
					throw new InvalidDataException($"Ill-Formed Method JType: \"{reader}\"");
				}


				// read every thing between the '(' & ')'
				string parameters_typing = reader.ReadUntil(c => c == ')');
				reader.Skip(); // skip ')'

				try
				{
					ReturnType = new JType(reader);
				}
				catch (Exception)
				{
					Console.WriteLine($"Error While Parsing Return Type For Method JType: \"{reader}\"");
					throw;
				}

				List<JType> parameters = new(8);
				JStringReader parameters_reader = new(parameters_typing);

				while (parameters_reader) // not EOF
				{
					// TODO: CATCH EXCEPTIONS FOR MORE DETAILS
					JType type = new(parameters_reader);

					parameters.Add(type);
				}

				MethodParameters = parameters.ToArray();

				break;
			}
			default:
			{
				throw new InvalidDataException($"Invalid Encoding For JType: \"{reader}\"");
			}
		}

		if (reader.Peek() == '<')
		{
			reader.Skip(); // skips '<'

			int end_arrow = reader.IndexOf('>');
			if (end_arrow == -1)
			{
				throw new InvalidDataException($"Invalid generics {reader}");
			}

			string generics_raw = reader.ReadUntil(c => c == '>');
			reader.Skip(); // skips '>'


			List<JType> generics = new(8);

			JStringReader generics_reader = new(generics_raw);
			while (generics_reader) // not EOF
			{
				// TODO: CATCH EXCEPTIONS FOR MORE DETAILS
				JType type = new(generics_reader);

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
		StringBuilder stringBuilder = new(8);

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
				stringBuilder.Append(GetSimplifiedObjectType());
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

	// for object/ref types, returns the package that they belong to
	// if the type is not an object/ref, an empty string will be returned
	public string GetPackageName()
	{
		if (this.Type == JTypeType.Object)
		{
			return ObjectType.Substring(0, ObjectType.LastIndexOf('/'));
		}
		return string.Empty;
	}

	// for object/ref types, returns the package that they belong to
	// if the type is not an object/ref, an empty string will be returned
	public string GetObjectName()
	{
		if (this.Type == JTypeType.Object)
		{
			return ObjectType.Substring(ObjectType.LastIndexOf('/') + 1).Replace('$', '.');
		}
		return string.Empty;
	}

	// cleans the type path, for example java/lang/object -> object or net/example/com/SomeType -> SomeType
	// subclasses 'Type$SubType' will be cleaned to 'Type.SubType'
	public string GetSimplifiedObjectType()
	{
		if (GetPackageName().StartsWith("java/"))
			return GetObjectName();
		return ObjectType;
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
		public Instruction[] Instructions;

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
		Constants = [];
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

		string class_name = Constants[ThisClass - 1].String;
		string super_name = Constants[SuperClass - 1].String;

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

				AccessFlagsUtility.ToString((string s) => builder.Append(s).Append(' '), field.AccessFlags);
				builder.Append(field.ValueType is not null ? field.ValueType.ToString() : "NULL").Append(' ');
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
				AccessFlagsUtility.ToString((string s) => builder.Append(s).Append(' '), method.AccessFlags);


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
					builder.Append(method.MethodType.ReturnType.ToString()).Append(' ');
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

					builder.Append(method_param.ToString());
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
		PostProcessConstants();

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

	public void PostProcessConstants()
	{
		for (uint i = 0; i < Constants.Length; i++)
		{
			Constant constant = Constants[i];
			switch (constant.type)
			{
				case ConstantType.Integer:
				{
					constant.String = constant.IntegerValue.ToString();
					break;
				}
				case ConstantType.Float:
				{
					constant.String = constant.FloatValue.ToString();
					break;
				}
				case ConstantType.Long:
				{
					constant.String = constant.LongValue.ToString();
					break;
				}
				case ConstantType.Double:
				{
					constant.String = constant.DoubleValue.ToString();
					break;
				}
				case ConstantType.Class:
				case ConstantType.FieldReference:
				case ConstantType.MethodReference:
				case ConstantType.InterfaceMethodReference:
				case ConstantType.StringReference:
				{
					constant.String = Constants[constant.NameIndex - 1].String;
					Console.WriteLine($"CONSTANT STRING: \"{constant.String}\"");
					break;
				}
				case ConstantType.NameTypeDescriptor:
				{
					constant.String = $"{Constants[constant.DescriptorIndex - 1].String} {Constants[constant.NameIndex - 1].String}";
					Console.WriteLine($"CONSTANT STRING: \"{constant.String}\"");
					break;
				}
				default: break;
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
				sb.Append("code=[");
				for (int i = 0; i < attribute.Code.Instructions.Length; i++)
				{
					if (i > 0)
					{
						sb.Append(", ");
					}

					sb.Append(attribute.Code.Instructions[i].ToString(Constants));
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
		JStringReader reader = new(Constants[field.DescriptorIndex - 1].String);
		field.ValueType = new JType(reader);
	}

	private void SetupMethod(Method method)
	{
		// TODO: check for errors/out of range indices
		method.Name = Constants[method.NameIndex - 1].String;
		JStringReader reader = new(Constants[method.DescriptorIndex - 1].String);
		method.MethodType = new JType(reader);
	}

	private static void LoadCodeInfo(Attribute attribute)
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

		ByteReader instruction_reader = new(reader.Read(code_len), 0, reader.Endianness);
		List<Instruction> instructions = new();

		int instructions_read_tries = 0;
		while (instruction_reader.SpaceLeft > 0 && instructions_read_tries++ < 0xffffff)
		{
			instructions.Add(Instruction.Read(instruction_reader));
		}
		attribute.Code.Instructions = instructions.ToArray();

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

public class JReader
{
	public JReader(BinaryReader br)
	{
		Reader = br;
	}

	public byte[] ReadBuffer(int length)
	{
		byte[] data = new byte[length];
		// FIXME: what to do if the buffer is not filled? throw an exception?
		Reader.Read(data, 0, length);
		return data;
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

public class JStringReader
{
	public JStringReader(string str, int start = 0)
	{
		String = str;
		Index = 0;
	}

	public void Skip(int count = 1)
	{
		Index += count;
	}

	public int Peek()
	{
		if (EOF)
			return -1;
		return String[Index];
	}

	public int Read()
	{
		if (EOF)
			return -1;
		return String[Index++];
	}

	public int Read(char[] buffer, int write_index, int count)
	{
		if (EOF)
			return 0;

		char[] chars = String.ToCharArray(Index, Math.Min(SpaceLeft, count));
		Array.Copy(chars, 0, buffer, write_index, chars.Length);
		Index += chars.Length;
		return chars.Length;
	}

	public string Read(int count)
	{
		if (EOF)
			return "";
		count = Math.Min(SpaceLeft, count);
		Index += count;
		return String.Substring(Index - count, count);
	}

	public int IndexOf(char value)
	{
		return String.IndexOf(value, Index);
	}

	public int IndexOf(string value)
	{
		return String.IndexOf(value, Index);
	}

	public int LastIndexOf(char value)
	{
		return String.LastIndexOf(value, Index);
	}

	public int LastIndexOf(string value)
	{
		return String.LastIndexOf(value, Index);
	}

	public int GoToIndexOf(char value)
	{
		int index = String.IndexOf(value, Index);
		if (index == -1)
			Index = String.Length;
		Index = index;
		return Index;
	}

	public int GoToIndexOf(string value)
	{
		int index = String.IndexOf(value, Index);
		if (index == -1)
			Index = String.Length;
		Index = index;
		return Index;
	}

	public int GoToLastIndexOf(char value)
	{
		int index = String.LastIndexOf(value, Index);
		if (index == -1)
			Index = String.Length;
		Index = index;
		return Index;
	}

	public int GoToLastIndexOf(string value)
	{
		int index = String.LastIndexOf(value, Index);
		if (index == -1)
			Index = String.Length;
		Index = index;
		return Index; ;
	}

	/// <summary>
	/// skips all repeats of 'value'
	/// </summary>
	/// <param name="character">the character to be skipped</param>
	/// <returns>the number of characters skipped</returns>
	/// <example> in the string 'aaaab', the return value for SkipCount('a') will be 4 and the read position will be at the 'b' </example>
	public int SkipCount(char character)
	{
		if (EOF)
			return 0;
		int count = 0;
		while (String[Index + count] == character)
		{
			count++;
		}
		Index += count;
		return count;
	}

	/// <summary>
	/// reads until the predicate is satisfied, stops the reader at the character satisfying the predicate
	/// </summary>
	/// <param name="predicate"></param>
	/// <returns>the read string</returns>
	public string ReadUntil(Predicate<char> predicate)
	{
		int count = 0;
		while (!EOF)
		{
			if (predicate(String[Index + count]))
				break;
			count++;
		}
		return Read(count);
	}

	public string String { get; init; }
	private int _index;
	public int Index
	{
		get => _index;
		set
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), "negative index");

			if (value > String.Length)
				throw new ArgumentOutOfRangeException(nameof(value), "overflow index");

			_index = value;
		}
	}

	public static implicit operator bool(JStringReader reader)
	{
		return !reader.EOF;
	}

	public int SpaceLeft { get => String.Length - Index; }
	public bool EOF { get => String.Length == Index; }
}

static public class JDisassembler
{

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
