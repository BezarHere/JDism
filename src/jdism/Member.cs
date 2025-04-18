using System.Diagnostics;
using System.Text;

namespace JDism;

public class Member<AccessFlagsT> where AccessFlagsT : Enum
{
  public AccessFlagsT AccessFlags;
  public ushort NameIndex;
  public string Name = "";
  public ushort DescriptorIndex;
  public JType InnerType = new();
  public JAttribute[] Attributes = [];

  private static JAttributeType[] sHiddenAttributes = [
    JAttributeType.Signature
  ];

  protected virtual void WriteAccessFlags(StringBuilder builder) {
    throw new NotImplementedException();
  }

  public override string ToString()
  {
    StringBuilder builder = new(32);

    {
      var annotated_attrs =
        from attr in Attributes
        let attr_type_info = JAttribute.FetchAttributeTypeInfo(attr.GetType())
        where !sHiddenAttributes.Contains(attr_type_info.AttributeType)
        select attr;
      
      foreach (JAttribute attr in annotated_attrs)
      {
        builder.Append(attr.ToString()).Append('\n');
      }
    }

    WriteAccessFlags(builder);

    SignatureJAttribute signature_attr = (SignatureJAttribute)Attributes.FirstOrDefault(
      attr => attr is SignatureJAttribute
    );

    if (signature_attr is not null)
    {
      if (signature_attr.Signature.Kind == JTypeKind.Method)
      {
        int func_name_index = signature_attr.Signature.Name.IndexOf('(');
        Debug.Assert(func_name_index != -1);

        builder.Append(signature_attr.Signature.Name[..func_name_index]);
        builder.Append(' ');
        builder.Append(Name);
        builder.Append(signature_attr.Signature.Name[func_name_index..]);
      }
      else
      {
        builder.Append(signature_attr.Signature.Name);
        builder.Append(' ');
        builder.Append(Name);
      }
    }

    return builder.ToString();
  }
}
