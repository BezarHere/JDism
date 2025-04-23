
static class EnumerableExtensions
{
  public static IEnumerable<(int Index, T Value)> Iterate<T>(this IEnumerable<T> source)
  {
    int counter = 0;
    return from s in source select (counter++, s);
  }


}
