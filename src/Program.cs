

using JDism;

internal class Program
{
	static void Main(string[] args)
	{
		const string base_path = @"F:\Assets\visual studio\JDism\";
		Console.WriteLine(base_path);
		Console.WriteLine("STARTING");

		FileStream stream = File.OpenRead(base_path + "AllBlocks.class");
		BinaryReader br = new(stream);
		Console.WriteLine(JDisassembler.Decompile(br, out Disassembly disassembly));
		File.WriteAllText(base_path + "Decompiled.java", disassembly.GenerateSource());

		Console.ReadKey();

	}
}
