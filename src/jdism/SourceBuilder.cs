
using System.Diagnostics;
using System.Text;

namespace JDism;

ref struct SourceBuilder(IEnumerable<JVMAttribute> attributes, string name, JType? type, JContextView context = default)
{
  private const string Indent = "  ";

  public readonly IEnumerable<JVMAttribute> Attributes => attributes;
  public readonly string Name => name;
  public readonly JType? Type => type;
  public readonly JContextView Context = context;

  public readonly string BuildMethod(MethodAccessFlags access_flags)
  {
    StringBuilder builder = new();
    ReadOnlySpan<Instruction> instructions = null;
    JType? _type = Type;

    foreach (JVMAttribute attribute in Attributes)
    {
      if (attribute is SignatureAnnotation signature_attr)
      {
        _type = signature_attr.Signature;
        continue;
      }
      if (attribute is CodeInfoAnnotation code_attr)
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

    if (instructions.IsEmpty)
    {
      builder.Append(';');
      return builder.ToString();
    }

    builder.AppendLine();
    builder.Append('{').AppendLine();

    InstructionDisplay inst_display = new();
    builder.Append(inst_display.Decode(instructions, Context));

    builder.Append('}');

    return builder.ToString();
  }

  public readonly string BuildField(FieldAccessFlags access_flags)
  {
    StringBuilder builder = new();
    JType? _type = Type;

    Constant default_value = null;

    foreach (JVMAttribute attribute in attributes)
    {
      if (attribute is SignatureAnnotation signature_attr)
      {
        _type = signature_attr.Signature;
        continue;
      }

      if (attribute is ConstantInfoAnnotation constant_attr)
      {
        default_value = constant_attr.Constant;
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
    
    if (default_value is not null)
    {
      builder.Append(' ');
      builder.Append('=');
      builder.Append(' ');
      builder.Append(default_value.ToString());
    }

    builder.Append(';');

    return builder.ToString();
  }

}

