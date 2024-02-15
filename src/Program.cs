

using JDism;

internal class Program
{
	static void Main(string[] args)
	{
		const string base_path = @"F:\Assets\visual studio\JDism\";
		Console.WriteLine(base_path);
		Console.WriteLine("STARTING");

		FileStream stream = File.OpenRead(base_path + "Util.class");
		BinaryReader br = new(stream);
		Disassembly disassembly;
		Console.WriteLine(JDisassembler.Decompile(br, out disassembly));
		Console.ReadKey();
	}
}
