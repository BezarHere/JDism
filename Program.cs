

using System.Text;
using System.Text.Unicode;

namespace JDism;

static public class JDisassembler
{

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

	public enum JTypeType
	{
		// sigend byte
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

	public struct JType
	{
		JTypeType Type;
		uint ArrayDimension = 0;
		string ObjectType = "";
		// last parameter is the return type
		JType[]? MethodParameters;

		public JType()
		{
			Type = JTypeType.Object;
		}

		public JType(string encoded_type)
		{

		}

		public JType(JTypeType type, uint arr_d, string obj_type)
		{
			Type = type;
			ArrayDimension = arr_d;
			ObjectType = obj_type;
		}



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
		public byte ReferenceKind { get => (byte)index1; set => index1 = value; }
		public ushort ReferenceIndex { get => index2; set => index2 = value; }

		public ushort BootstrapMethodAttrIndex { get => index1; set => index1 = value; }

		public string String { get; set; } = "";

		private ushort index1;
		private ushort index2;
		private long long_int;
		private double double_float;
	}

	public class Attribute
	{
		public ushort NameIndex;
		public byte[]? _data;
	}

	public class Field
	{
		public ushort AccessFlags;
		public ushort NameIndex;
		public ushort DescriptorIndex;
		public Attribute[]? Attributes;
	}
	public class Method
	{
		public ushort AccessFlags;
		public ushort NameIndex;
		public ushort DescriptorIndex;
		public Attribute[]? Attributes;
	}

	public class Disassembly
	{
		public ushort VersionMinor;
		public ushort VersionMajor;
		public Constant[]? Constants;
		public ushort AccessFlags;
		public ushort ThisClass;
		public ushort SuperClass;
		public ushort[]? Interfaces;
		public Field[]? Fields;
		public Method[]? Methods;
		public Attribute[]? Attributes;

		// constant index X (X for the class file) is Constant[ConstantIndexRoutingTable[X]]
		public ushort[]? ConstantIndexRoutingTable;

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

			builder.Append("class ");

			return builder.ToString();
		}

		public void BuildCIRT()
		{
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

	}

	private class JReader
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
						constant.ReferenceKind = Read();
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
				AccessFlags = ReadU16BE(),
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
				AccessFlags = ReadU16BE(),
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

		disassembly.Constants = new Constant[reader.ReadU16BE()];

		for (uint i = 0; i < disassembly.Constants.Length - 1; i++)
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

		disassembly.AccessFlags = reader.ReadU16BE();
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
			disassembly.Attributes[i] = reader.ReadAttribute();
		}

		return string.Empty;
	}

}

internal class Program
{
	static void Main(string[] args)
	{
		const string base_path = @"F:\Assets\visual studio\JDism\";
		Console.WriteLine(base_path);
		Console.WriteLine("STARTING");

		FileStream stream = File.OpenRead(base_path + "Util.class");
		BinaryReader br = new(stream);
		JDisassembler.Disassembly disassembly;
		Console.WriteLine(JDisassembler.Decompile(br, out disassembly));
		Console.ReadKey();
	}
}
