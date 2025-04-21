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
