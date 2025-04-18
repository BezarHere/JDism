using System.Text;

namespace JDism;

public class Field : Member<FieldAccessFlags>
{
  protected override void WriteAccessFlags(StringBuilder builder)
  {
    AccessFlagsUtility.ToString(s => builder.Append(s).Append(' '), AccessFlags);
  }
}
