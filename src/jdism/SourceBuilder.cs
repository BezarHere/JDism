
using System.Diagnostics;
using System.Text;

namespace JDism;

ref struct SourceBuilder(IEnumerable<JAttribute> attributes, string name, JType? type, JContextView context = default)
{
  private const string Indent = "  ";

  public readonly IEnumerable<JAttribute> Attributes => attributes;
  public readonly string Name => name;
  public readonly JType? Type => type;
  public readonly JContextView Context = context;

  public string BuildMethod(MethodAccessFlags access_flags)
  {
    StringBuilder builder = new();
    IEnumerable<Instruction> instructions = null;
    JType? _type = Type;

    foreach (JAttribute attribute in Attributes)
    {
      if (attribute is SignatureJAttribute signature_attr)
      {
        _type = signature_attr.Signature;
        continue;
      }
      if (attribute is CodeInfoJAttribute code_attr)
      {
        instructions = code_attr.Instructions;
        continue;
      }

      builder.Append(attribute.ToString()).AppendLine();
    }

    JType type = _type ?? throw new ArgumentNullException(nameof(_type));
    Debug.Assert(type.Kind == JTypeKind.Method);

    builder.Append(
      string.Join(" ", AccessFlagsUtility.ToString(access_flags))
    );
    builder.Append(' ');

    builder.Append(type.ToString().Replace(JType.SpacialNamePlaceholder, Name));

    if (instructions is null)
    {
      builder.Append(';');
      return builder.ToString();
    }

    builder.AppendLine();
    builder.Append('{').AppendLine();

    foreach (Instruction inst in instructions)
    {
      builder.Append(Indent);
      builder.Append(inst.ToString(Context));
      builder.AppendLine();
    }

    builder.Append('}');

    return builder.ToString();
  }

  public string BuildField(FieldAccessFlags access_flags)
  {
    StringBuilder builder = new();
    JType? _type = Type;

    foreach (JAttribute attribute in attributes)
    {
      if (attribute is SignatureJAttribute signature_attr)
      {
        _type = signature_attr.Signature;
        continue;
      }

      builder.Append(attribute.ToString()).AppendLine();
    }

    JType type = _type ?? throw new ArgumentNullException(nameof(_type));
    Debug.Assert(type.Kind != JTypeKind.Method);
    Debug.Assert(type.Kind != JTypeKind.Class);

    builder.Append(
      string.Join(" ", AccessFlagsUtility.ToString(access_flags))
    );
    builder.Append(' ');

    builder.Append(type.ToString());
    builder.Append(' ');
    builder.Append(name);
    builder.Append(';');

    return builder.ToString();
  }

}

