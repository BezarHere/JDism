

using System.Text;
using JDism;

ref struct InstructionDisplay
{


  public int AddressStringWidth = 4;
  public bool AddressInHex = true;

  public byte IndentLevel = 1;
  public string IndentChars = "  ";

  public int CommentInfoMaxLength = 24;

  public InstructionDisplay()
  {
  }

  public readonly string GetAddressFormat()
  {
    return (AddressInHex ? 'X' : 'D') + AddressStringWidth.ToString();
  }

  public readonly string[] GetStringLines(in ReadOnlySpan<Instruction> instructions,
                                          in JContextView ctx)
  {
    string[] strings = new string[instructions.Length];

    for (int i = 0; i < strings.Length; i++)
    {
      strings[i] = instructions[i].ToString(ctx);
    }

    return strings;
  }

  public readonly string Decode(ReadOnlySpan<Instruction> instructions, JContextView ctx)
  {
    string[] strings = GetStringLines(instructions, ctx);

    Span<int> addresses = stackalloc int[instructions.Length];
    LoadAddresses(instructions, ref addresses);

    StringBuilder builder = new(instructions.Length * 16);

    string address_format = GetAddressFormat();
    string indent = string.Join("", Enumerable.Repeat(IndentChars, IndentLevel));

    for (int i = 0; i < instructions.Length; i++)
    {
      builder.Append(indent);
      builder.Append(addresses[i].ToString(address_format));
      builder.Append(' ');
      builder.Append(strings[i]);

      if (Instruction.IsJumpOpCode(instructions[i].OpCode))
      {
        builder.Append(' ');
        builder.Append("// ");
        int address = instructions[i].Parameters[0].Value;
        int target_index = addresses.IndexOf(address);

        if (target_index == -1)
        {
          builder.Append("INVALID JUMP ADDRESS");
        }
        else
        {
          string s = strings[target_index];
          if (s.Length > CommentInfoMaxLength)
          {
            builder.Append(s[..(CommentInfoMaxLength - 3)]).Append("...");
          }
          else
          {
            builder.Append(s);
          }
        }
      }


      builder.AppendLine();
    }

    return builder.ToString();
  }

  private static void LoadAddresses(ReadOnlySpan<Instruction> instructions,
                                    ref Span<int> addresses)
  {
    int acc = 0;

    for (int i = 0; i < instructions.Length; i++)
    {
      addresses[i] = instructions[i].Size + acc;
      acc = addresses[i];
    }
  }

}
