namespace JDism;

public class Member<AccessFlagsT>
{
  public AccessFlagsT AccessFlags;
	public ushort NameIndex;
	public string Name = "";
	public ushort DescriptorIndex;
	public JType ResultType = new();
	public Attribute[] Attributes = [];

  
}
