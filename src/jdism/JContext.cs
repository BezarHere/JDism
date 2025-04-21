
namespace JDism;

class JContext
{
  public string FileName = "";
  public Field[] Fields = null;
  public Method[] Methods = null;
  public Constant[] Constants = null;
  public InnerLogger Logger = new();
}

readonly ref struct JContextView(in JContext context = null)
{
  public readonly string FileName = context is null ? "" : context.FileName;
  public readonly ReadOnlySpan<Field> Fields = context is null ? [] : context.Fields;
  public readonly ReadOnlySpan<Method> Methods = context is null ? [] : context.Methods;
  public readonly ReadOnlySpan<Constant> Constants = context is null ? [] : context.Constants;
  public readonly InnerLogger Logger = context?.Logger ?? new InnerLogger();


  public static implicit operator JContextView(in JContext context)
  {
    return new(context);
  }
}


