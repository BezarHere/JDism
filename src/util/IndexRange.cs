namespace Util;

public readonly struct IndexRange(int start, int end)
{
  public int Start => start;
  public int End => end;

  public int Length => end - start;

  public static implicit operator IndexRange(Range range)
    => new(range.Start.Value, range.End.Value);

  public static implicit operator Range(IndexRange range)
    => new(new Index(range.Start), new Index(range.End));

  public override string ToString()
  {
    return $"({start}:{end})";
  }

  public readonly IndexRange Merge(in IndexRange other)
  {
    return new(Math.Min(Start, other.Start), Math.Max(End, other.End));
  }


  // start inclusive, end exclusive [start, end)
  public bool InRange(int index) => index >= start && index < end;
  // start and end inclusive [start, end]
  public bool Contains(int index) => index >= start && index <= end;

  public bool Intersects(in IndexRange other) => Start < other.End && End > other.Start;
  public bool Touches(in IndexRange other) => Start <= other.End && End >= other.Start;
}

