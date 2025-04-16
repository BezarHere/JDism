namespace JDism;

public class JStringReader
{
	public JStringReader(string str, int start = 0)
	{
		String = str;
		Index = 0;
	}

	public void Skip(int count = 1)
	{
		Index += count;
	}

	public int Peek()
	{
		if (EOF)
			return -1;
		return String[Index];
	}

	public int Read()
	{
		if (EOF)
			return -1;
		return String[Index++];
	}

	public int Read(char[] buffer, int write_index, int count)
	{
		if (EOF)
			return 0;

		char[] chars = String.ToCharArray(Index, Math.Min(SpaceLeft, count));
		Array.Copy(chars, 0, buffer, write_index, chars.Length);
		Index += chars.Length;
		return chars.Length;
	}

	public string Read(int count)
	{
		if (EOF)
			return "";
		count = Math.Min(SpaceLeft, count);
		Index += count;
		return String.Substring(Index - count, count);
	}

	public int IndexOf(char value)
	{
		return String.IndexOf(value, Index);
	}

	public int IndexOf(string value)
	{
		return String.IndexOf(value, Index);
	}

	public int LastIndexOf(char value)
	{
		return String.LastIndexOf(value, Index);
	}

	public int LastIndexOf(string value)
	{
		return String.LastIndexOf(value, Index);
	}

	public int GoToIndexOf(char value)
	{
		int index = String.IndexOf(value, Index);
		if (index == -1)
			Index = String.Length;
		Index = index;
		return Index;
	}

	public int GoToIndexOf(string value)
	{
		int index = String.IndexOf(value, Index);
		if (index == -1)
			Index = String.Length;
		Index = index;
		return Index;
	}

	public int GoToLastIndexOf(char value)
	{
		int index = String.LastIndexOf(value, Index);
		if (index == -1)
			Index = String.Length;
		Index = index;
		return Index;
	}

	public int GoToLastIndexOf(string value)
	{
		int index = String.LastIndexOf(value, Index);
		if (index == -1)
			Index = String.Length;
		Index = index;
		return Index; ;
	}

	/// <summary>
	/// skips all repeats of 'value'
	/// </summary>
	/// <param name="character">the character to be skipped</param>
	/// <returns>the number of characters skipped</returns>
	/// <example> in the string 'aaaab', the return value for SkipCount('a') will be 4 and the read position will be at the 'b' </example>
	public int SkipCount(char character)
	{
		if (EOF)
			return 0;
		int count = 0;
		while (String[Index + count] == character)
		{
			count++;
		}
		Index += count;
		return count;
	}

	/// <summary>
	/// reads until the predicate is satisfied, stops the reader at the character satisfying the predicate
	/// </summary>
	/// <param name="predicate"></param>
	/// <returns>the read string</returns>
	public string ReadUntil(Predicate<char> predicate)
	{
		int count = 0;
		while (!EOF)
		{
			if (predicate(String[Index + count]))
				break;
			count++;
		}
		return Read(count);
	}

	public string String { get; init; }
	private int _index;
	public int Index
	{
		get => _index;
		set
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), "negative index");

			if (value > String.Length)
				throw new ArgumentOutOfRangeException(nameof(value), "overflow index");

			_index = value;
		}
	}

	public static implicit operator bool(JStringReader reader)
	{
		return !reader.EOF;
	}

	public int SpaceLeft { get => String.Length - Index; }
	public bool EOF { get => String.Length == Index; }
}
