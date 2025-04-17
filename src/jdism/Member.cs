namespace JDism;

public class Member<AccessFlagsT>
{
  public AccessFlagsT AccessFlags;
	public ushort NameIndex;
	public string Name = "";
	public ushort DescriptorIndex;
	public JType InnerType = new();
	public Attribute[] Attributes = [];

  public override string ToString()
  {
    return "";
  }
}
