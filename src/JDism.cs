using System.Diagnostics;

namespace JDism;

static class JDisassembler
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

		disassembly.DeserializeConstantsTable(reader);

		

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
		build_log.WriteLine($"Class name: {disassembly.Context.Constants[disassembly.ThisClass - 1].String} : {disassembly.Context.Constants[disassembly.SuperClass - 1].String}");


		disassembly.Interfaces = new ushort[reader.ReadU16BE()];

		for (uint i = 0; i < disassembly.Interfaces.Length; i++)
		{
			disassembly.Interfaces[i] = reader.ReadU16BE();
		}


		disassembly.Context.Fields = new Field[reader.ReadU16BE()];

		for (uint i = 0; i < disassembly.Context.Fields.Length; i++)
		{
			disassembly.Context.Fields[i] = reader.ReadField();
      disassembly.Context.Fields[i].Attributes = new JAttribute[reader.ReadU16BE()];
      for (int j = 0; j < disassembly.Context.Fields[i].Attributes.Length; j++)
      {
        disassembly.Context.Fields[i].Attributes[j] = disassembly.ReadAttribute(reader);
      }
		}


		disassembly.Context.Methods = new Method[reader.ReadU16BE()];

		for (uint i = 0; i < disassembly.Context.Methods.Length; i++)
		{
			disassembly.Context.Methods[i] = reader.ReadMethod();
      disassembly.Context.Methods[i].Attributes = new JAttribute[reader.ReadU16BE()];
      for (int j = 0; j < disassembly.Context.Methods[i].Attributes.Length; j++)
      {
        disassembly.Context.Methods[i].Attributes[j] = disassembly.ReadAttribute(reader);
      }
		}



		disassembly.Attributes = new JAttribute[reader.ReadU16BE()];

		for (uint i = 0; i < disassembly.Attributes.Length; i++)
		{
      disassembly.Attributes[i] = disassembly.ReadAttribute(reader, JTypeParseContext.Class);
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
