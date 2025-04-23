
namespace JDism;

readonly record struct JVersion(ushort Major, ushort Minor)
{
  public static implicit operator JVersion((ushort, ushort) tpl)
  {
    return new JVersion(tpl.Item1, tpl.Item2);
  }
}

class JContext
{
  public string FileName = "";
  public Field[] Fields = null;
  public Method[] Methods = null;
  public Constant[] Constants = null;

  public JVersion Version;

  public InnerLogger Logger = new();
}

readonly ref struct JContextView(in JContext context = null)
{
  public readonly string FileName = context?.FileName ?? "";
  public readonly ReadOnlySpan<Field> Fields = context?.Fields ?? [];
  public readonly ReadOnlySpan<Method> Methods = context?.Methods ?? [];
  public readonly ReadOnlySpan<Constant> Constants = context?.Constants ?? [];

  public readonly JVersion Version = context?.Version ?? default;

  public readonly InnerLogger Logger = context?.Logger ?? new InnerLogger();


  public static implicit operator JContextView(in JContext context)
  {
    return new(context);
  }
}


