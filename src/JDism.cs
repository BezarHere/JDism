using System.Data;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using Colussom;

namespace JDism;

public class Disassembly : InnerLogger
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
				builder.Append(field.ResultType is not null ? field.ResultType.ToString() : "NULL").Append(' ');
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
					builder.Append(method.ResultType.ReturnType.ToString()).Append(' ');
					bool internal_name = method.Name.Contains('$');
					if (internal_name)
					{
						builder.Append("__");
					}

					builder.Append(method.Name.Replace('$', '_'));
				}
				// parameters
				builder.Append('(');

				for (int i = 0; i < method.ResultType.MethodParameters.Length; i++)
				{
					JType method_param = method.ResultType.MethodParameters[i];
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
			if (constant.type == ConstantType.Long || constant.type == ConstantType.Double)
				i++;

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
					Logger.WriteLine($"CONSTANT STRING: \"{constant.String}\"");
					break;
				}
				case ConstantType.NameTypeDescriptor:
				{
					constant.String = $"{Constants[constant.DescriptorIndex - 1].String} {Constants[constant.NameIndex - 1].String}";
					Logger.WriteLine($"CONSTANT STRING: \"{constant.String}\"");
					break;
				}
				default: break;
			}
		}
	}

	private string AttributesToAnnotations(Attribute[] attributes)
	{
		StringBuilder sb = new();

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
		field.ResultType = new JType(reader);
	}

	private void SetupMethod(Method method)
	{
		// TODO: check for errors/out of range indices
		method.Name = Constants[method.NameIndex - 1].String;
		JStringReader reader = new(Constants[method.DescriptorIndex - 1].String);
		method.ResultType = new JType(reader);
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

static public class JDisassembler
{

	public static string Disassemble(BinaryReader stream, out Disassembly disassembly)
	{
		if (stream == null)
		{
			throw new ArgumentNullException(nameof(stream));
		}

		long MemUsagePrivate = 0;
		long MemUsagePaged = 0;

		using (Process process = Process.GetCurrentProcess())
		{
			MemUsagePrivate = process.PrivateMemorySize64;
			MemUsagePaged = process.PagedMemorySize64;
		}

    StringWriter build_log = new();

    disassembly = new Disassembly
    {
      Logger = build_log
    };

    JReader reader = new(stream)
    {
      Logger = build_log
    };

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
			build_log.Write($"READING CONST[{i + 1}] ");
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
			build_log.WriteLine($"Error On Constant {errors[i].Index}: {errors[i].Message}");
		}
#endif

		disassembly.AccessFlags = (ClassAccessFlags)reader.ReadU16BE();
		disassembly.ThisClass = reader.ReadU16BE();
		disassembly.SuperClass = reader.ReadU16BE();
		build_log.WriteLine($"Class name: {disassembly.Constants[disassembly.ThisClass - 1].String} : {disassembly.Constants[disassembly.SuperClass - 1].String}");


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
			build_log.WriteLine($"attribute \"{attr.Name}\": {attr._data.Length} bytes");
		}

		disassembly.PostProcess();
		string text = disassembly.GenerateSource();
		File.WriteAllText("output.java", text);

		using (Process process = Process.GetCurrentProcess())
		{
			build_log.WriteLine($"memory usage [private]: {process.PrivateMemorySize64 / (1 << 20)}MB");
			build_log.WriteLine($"memory usage [paged]: {process.PagedMemorySize64 / (1 << 20)}MP");
			build_log.WriteLine($"disassembly memory usage [private]: {(process.PrivateMemorySize64 - MemUsagePrivate) / (1 << 20)}MB");
			build_log.WriteLine($"disassembly memory usage [paged]: {(process.PagedMemorySize64 - MemUsagePaged) / (1 << 20)}MP");
		}


    reader.Logger = null;
    disassembly.Logger = null;
		return build_log.ToString();
	}

}
