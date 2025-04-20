using System.Text;

namespace JDism;

class JReader(BinaryReader br) : InnerLogger
{
  public BinaryReader Reader => br;

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
				Logger.WriteLine($"READ CONSTANT UTF8: \"{constant.String}\"");
				break;
			}
			case ConstantType.Integer:
			{
				constant.IntegerValue = (int)ReadU32BE();
				Logger.WriteLine($"READ CONSTANT INT: {constant.IntegerValue}");
				break;
			}
			case ConstantType.Float:
			{
				constant.LongValue = (int)ReadIEEE754();
				Logger.WriteLine($"READ CONSTANT LONG: {constant.LongValue}");
				break;
			}
			case ConstantType.Long:
			{
				constant.FloatValue = (int)ReadU64BE();
				Logger.WriteLine($"READ CONSTANT FLOAT: {constant.FloatValue}");
				break;
			}
			case ConstantType.Double:
			{
				constant.DoubleValue = (int)ReadDoubleIEEE754();
				Logger.WriteLine($"READ CONSTANT DOUBLE: {constant.DoubleValue}");
				break;
			}
			case ConstantType.Class:
			{
				constant.NameIndex = ReadU16BE();
				Logger.WriteLine($"READ CONSTANT CLASS: NAME_INDEX={constant.NameIndex}");
				break;
			}
			case ConstantType.FieldReference:
			case ConstantType.MethodReference:
			case ConstantType.InterfaceMethodReference:
			{
				constant.ClassIndex = ReadU16BE();
				constant.NameTypeIndex = ReadU16BE();
				Logger.WriteLine($"READ CONSTANT F/M/IM-REF: CLASS_INDEX={constant.ClassIndex} NAMETYPE={constant.NameTypeIndex}");
				break;
			}
			case ConstantType.StringReference:
			{
				constant.StringIndex = ReadU16BE();
				Logger.WriteLine($"READ CONSTANT STRING-REF: STRING_INDEX={constant.StringIndex}");
				break;
			}
			case ConstantType.NameTypeDescriptor:
			{
				constant.NameIndex = ReadU16BE();
				constant.DescriptorIndex = ReadU16BE();
				Logger.WriteLine($"READ CONSTANT NAMETYPE-DESC: NAMEINDEX={constant.NameIndex} DESCINDEX={constant.DescriptorIndex}");
				break;
			}
			case ConstantType.MethodHandle:
			{
				constant.ReferenceKind = (MethodReferenceKind)Read();
				constant.ReferenceIndex = ReadU16BE();
				Logger.WriteLine($"READ CONSTANT MHANDLE: REFKIND={constant.ReferenceKind} REFINDEX={constant.ReferenceIndex}");
				break;
			}
			case ConstantType.MethodType:
			{
				constant.DescriptorIndex = ReadU16BE();
				Logger.WriteLine($"READ CONSTANT MTYPE: DESCINDEX={constant.DescriptorIndex}");
				break;
			}
			case ConstantType.InvokeDynamic:
			{
				constant.BootstrapMethodAttrIndex = ReadU16BE();
				constant.NameTypeIndex = ReadU16BE();
				Logger.WriteLine($"READ CONSTANT INVOKEDYN: BMAI={constant.BootstrapMethodAttrIndex} NAMETYPE={constant.NameTypeIndex}");
				break;
			}
		}

		return constant;
	}

	public Field ReadField()
	{
		Field field = new()
		{
			AccessFlags = (FieldAccessFlags)ReadU16BE(),
			NameIndex = ReadU16BE(),
			DescriptorIndex = ReadU16BE()
		};

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

		return method;
	}
}
