

using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using JDism;
using Util;

ref struct InstructionDisplay
{
  public const int JumpMapColumnsMaxCount = 18;


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
    Span<int> addresses = stackalloc int[instructions.Length];
    LoadAddresses(instructions, ref addresses);

    string[] strings = GetStringLines(instructions, ctx);

    StringBuilder builder = new(instructions.Length * 16);

    string address_format = GetAddressFormat();
    string indent = string.Join("", Enumerable.Repeat(IndentChars, IndentLevel));

    string[] jumps_strings = [
      .. BuildJumpsString(
        instructions.Length,
        GetJumpTable(instructions, addresses)
      )
    ];

    for (int i = 0; i < instructions.Length; i++)
    {
      builder.Append(indent);
      builder.Append(jumps_strings[i]);
      builder.Append(addresses[i].ToString(address_format));
      builder.Append(' ');
      builder.Append(strings[i]);

      if (Instruction.IsJumpOpCode(instructions[i].OpCode))
      {
        builder.Append(' ');
        builder.Append("// ");
        int target_address = instructions[i].Parameters[0].Value + addresses[i];
        int target_index = addresses.IndexOf(target_address);

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

  private readonly record struct Jump(int Start, int Target);
  private record JumpLevel(IndexRange Range, Jump[] Jumps);

  private static Dictionary<int, List<int>> GetJumpTable(
      ReadOnlySpan<Instruction> instructions,
      ReadOnlySpan<int> addresses
    )
  {
    Dictionary<int, List<int>> jumps = [];
    for (int current_index = 0; current_index < instructions.Length; current_index++)
    {
      if (!Instruction.IsJumpOpCode(instructions[current_index].OpCode))
      {
        continue;
      }

      int target_index = GetJumpTarget(
        instructions[current_index], addresses[current_index],
        instructions, addresses
      );

      if (!jumps.ContainsKey(target_index))
      {
        jumps.Add(target_index, []);
      }

      jumps[target_index].Add(current_index);
    }

    return jumps;
  }

  private static int GetJumpTarget(in Instruction inst, int address,
                                    ReadOnlySpan<Instruction> instructions,
                                    ReadOnlySpan<int> addresses)
  {

    int target_address = inst.Parameters[0].Value + address;
    int target_index = addresses.IndexOf(target_address);

    if (target_index == -1)
    {
      return 0;
    }

    if (Instruction.IsAlwaysJumpOpCode(instructions[target_index].OpCode))
    {
      return GetJumpTarget(
        instructions[target_index], target_address,
        instructions, addresses
      );
    }

    return target_index;
  }

  private static List<List<Jump>> GenJumpStack(Dictionary<int, List<int>> table)
  {
    List<List<Jump>> stacks = [];
    List<IndexRange> ranges = [];

    foreach ((int target_index, List<int> sources) in table)
    {
      List<Jump> jumps = [];
      int reach_start = target_index;
      int reach_end = target_index;

      foreach (int source in sources)
      {
        jumps.Add(
          new(source, target_index)
        );

        if (source < reach_start)
        {
          reach_start = source;
        }

        if (source > reach_end)
        {
          reach_end = source;
        }
      }

      IndexRange reach_range = new(reach_start, reach_end);

      int target_stack = -1;

      for (int i = 0; i < stacks.Count; i++)
      {
        foreach (Jump jump_lvl in stacks[i])
        {
          IndexRange jmp_range = new(jump_lvl.Start, jump_lvl.Target);
          if (jmp_range.Touches(reach_range))
          {
            continue;
          }

          target_stack = i;
          break;
        }

        if (target_index != -1)
        {
          break;
        }
      }

      if (target_stack == -1)
      {
        stacks.Add(jumps);
        ranges.Add(reach_range);
        continue;
      }

      stacks[target_stack].AddRange(jumps);
      ranges[target_stack] = ranges[target_stack].Merge(reach_range);
    }

    return stacks;
  }

  private enum Orientation : byte
  {
    Vertical,
    Horizontal
  }

  private class JumpStringMap
  {
    public const char Empty = ' ';
    public const char Vertical = '|';
    public const char Horizontal = '-';
    public const char DeflectInto = '\\';
    public const char DeflectDown = '/';
    public const char Crossing = '+';
    public const char IntoArrow = '>';

    public readonly int Width;
    public int Height => Chars.Length;

    public char[][] Chars { get; init; }

    public char this[int x, int y]
    {
      get
      {
        if (y < 0 || y >= Chars.Length)
        {
          throw new IndexOutOfRangeException(nameof(y));
        }

        if (x < 0 || x >= Width)
        {
          throw new IndexOutOfRangeException(nameof(x));
        }

        return Chars[y][x];
      }

      set
      {
        if (y < 0 || y >= Chars.Length)
        {
          throw new IndexOutOfRangeException(nameof(y));
        }

        if (x < 0 || x >= Width)
        {
          throw new IndexOutOfRangeException(nameof(x));
        }

        Chars[y][x] = value;
      }
    }

    public char this[Index x, Index y]
    {
      get
      {
        if (y.IsFromEnd)
        {
          y = new(Chars.Length - y.Value);
        }

        if (x.IsFromEnd)
        {
          x = new(Width - x.Value);
        }

        return this[x.Value, y.Value];
      }
      set
      {
        if (y.IsFromEnd)
        {
          y = new(Chars.Length - y.Value);
        }

        if (x.IsFromEnd)
        {
          x = new(Width - x.Value);
        }

        this[x.Value, y.Value] = value;
      }
    }

    public JumpStringMap(int width, int height)
    {
      Width = width;
      Chars = new char[height][];

      for (int i = 0; i < Chars.Length; i++)
      {
        Chars[i] = [.. Enumerable.Repeat(' ', width)];
      }
    }

    public void Set(int x, int y, Orientation orientation)
    {
      char current = this[x, y];
      this[x, y] = Solve(current, orientation);
    }

    public void Set(Index x, Index y, Orientation orientation)
    {
      char current = this[x, y];
      this[x, y] = Solve(current, orientation);
    }

    public override string ToString()
    {
      StringBuilder builder = new(Chars.Length * JumpMapColumnsMaxCount);

      foreach (string s in ToEnumerableStrings())
      {
        builder.Append(s);
        builder.AppendLine();
      }

      return builder.ToString();
    }

    public IEnumerable<string> ToEnumerableStrings()
    {
      foreach (char[] c in Chars)
      {
        yield return new string(c);
      }
    }

    private static char Solve(char current, Orientation orientation)
    {
      // TODO: add comments

      if (current == Vertical && orientation == Orientation.Horizontal
          || current == Horizontal && orientation == Orientation.Vertical)
      {
        return Crossing;
      }

      if (current == DeflectInto)
      {
        return current;
      }

      if (current == DeflectDown)
      {
        if (orientation == Orientation.Vertical)
        {
          return Crossing;
        }

        return current;
      }

      if (current == Crossing)
      {
        return current;
      }

      if (current == IntoArrow)
      {
        throw new InvalidOperationException(
          $"can not modify char '{current}' by orientation {orientation}"
        );
      }


      if (orientation == Orientation.Horizontal)
      {
        return Horizontal;
      }

      if (orientation == Orientation.Vertical)
      {
        return Vertical;
      }

      throw new InvalidOperationException(
        $"Unreachable branch, current={current}, orientation={orientation}"
      );
    }
  }

  private static IEnumerable<string> BuildJumpsString(int size,
                                                      Dictionary<int, List<int>> jump_table)
  {
    List<List<Jump>> jump_stack = GenJumpStack(
      jump_table
    );

    if (jump_stack.Count == 0)
    {
      return Enumerable.Repeat(string.Empty, size);
    }

    JumpStringMap map = new(jump_stack.Count * 2 + 1, size);

    for (int i = 0; i < jump_stack.Count; i++)
    {
      BuildJumpMap_NearColumn(map, jump_stack[i]);
      foreach (Jump j in jump_stack[i])
      {
        BuildJumpMap_ConnectJump(map, j, (i + 1) * 2);
      }
    }

    return map.ToEnumerableStrings();
  }

  private static void BuildJumpMap_NearColumn(JumpStringMap map, IList<Jump> jumps)
  {
    foreach (Jump j in jumps)
    {
      map.Set(^1, j.Start, Orientation.Horizontal);
      map[^1, j.Target] = JumpStringMap.IntoArrow;
    }
  }

  private static void BuildJumpMap_ConnectJump(JumpStringMap map, Jump jump, int level)
  {
    for (int x = 1; x <= level; x++)
    {
      map.Set(map.Width - x - 1, jump.Start, Orientation.Horizontal);
      map.Set(map.Width - x - 1, jump.Target, Orientation.Horizontal);
    }

    int y_dt = jump.Start < jump.Target ? 1 : -1;
    int x_pos = map.Width - level - 1;

    for (int y = jump.Start; y != jump.Target; y += y_dt)
    {
      map.Set(x_pos, y, Orientation.Vertical);
    }

    if (y_dt == 1)
    {
      map[x_pos, jump.Start] = JumpStringMap.DeflectDown;
      map[x_pos, jump.Target] = JumpStringMap.DeflectInto;
    }
    else
    {
      map[x_pos, jump.Start] = JumpStringMap.DeflectInto;
      map[x_pos, jump.Target] = JumpStringMap.DeflectDown;
    }
  }

  private static void LoadAddresses(ReadOnlySpan<Instruction> instructions,
                                    ref Span<int> addresses)
  {
    int acc = 0;

    for (int i = 0; i < instructions.Length; i++)
    {
      addresses[i] = acc;
      acc = addresses[i] + instructions[i].Size;
    }
  }

}
