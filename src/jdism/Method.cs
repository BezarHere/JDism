using System.Text;

namespace JDism;

public class Method : Member<MethodAccessFlags>
{

  protected override void WriteAccessFlags(StringBuilder builder)
  {
    AccessFlagsUtility.ToString(s => builder.Append(s).Append(' '), AccessFlags);
  }
}
