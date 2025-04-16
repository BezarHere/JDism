

using System.Diagnostics;
using JDism;

internal class Program
{
  static readonly string base_path = Directory.GetCurrentDirectory();
	
  

  static void DisassembleAll(string folder)
  {
    Debug.Assert(folder is not null);
    Debug.Assert(Directory.Exists(folder));

    string output_dir = folder + "-output";

    Console.WriteLine(string.Join('\n', Utils.IterateFiles(folder)));    

    IEnumerable<FileInfo> class_files = 
      from file in Utils.IterateFiles(folder)
      where file.Extension == ".class" select file;

    foreach (FileInfo file in class_files)
    {
      string output_path = file.FullName.Replace(folder, output_dir);
      if (output_path == file.FullName)
      {
        Console.WriteLine(
          "Expecting a different output path: "
          + $"\n\tinput='{file.FullName}'"
          + $"\n\toutput='{output_path}'"
        );
        continue;
      }

      output_path = string.Concat(
        output_path.AsSpan(0, output_path.Length - ".class".Length),
        ".java"
      );

      Console.WriteLine($"Disassembling '{file.FullName}'");

      FileStream stream = File.OpenRead(file.FullName);
      BinaryReader br = new(stream);
      string disassembly_output = JDisassembler.Disassemble(br, out Disassembly disassembly);

      Directory.CreateDirectory(Path.GetDirectoryName(output_path));
      File.WriteAllText(output_path + "-dism", disassembly_output);
      File.WriteAllText(output_path, disassembly.GenerateSource());
      
      stream.Close();
    }
  }

  static int Main(string[] args)
	{
		Console.WriteLine(base_path);
		Console.WriteLine("STARTING");

    Console.Write("The class file? ");
    string input_path = Console.ReadLine()??"";

    if (string.IsNullOrEmpty(input_path))
    {
      Console.WriteLine("Please input an input path");
      return 1;
    }

    bool deep = false;
    if (input_path[0] == '*')
    {
      deep = true;
      input_path = input_path[1..];
    }

    if (deep)
    {
      Console.WriteLine("Doing deep disassembly");
      DisassembleAll(input_path);
      return 0;
    }

		FileStream stream = File.OpenRead(input_path);
		BinaryReader br = new(stream);
    
		Console.WriteLine(JDisassembler.Disassemble(br, out Disassembly disassembly));
		File.WriteAllText(base_path + "/Disassembled.java", disassembly.GenerateSource());
    
    stream.Close();

		Console.ReadKey();
    
    return 0;
	}
}
