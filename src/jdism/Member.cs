namespace JDism;

class Member<AccessFlagsT> where AccessFlagsT : Enum
{
  public AccessFlagsT AccessFlags;
  public ushort NameIndex;
  public string Name = "";
  public ushort DescriptorIndex;
  public JType InnerType = new();
  public JVMAttribute[] Attributes = [];

  protected virtual string BuildSource(SourceBuilder builder) {
    throw new NotImplementedException();
  }

  public string ToString(JContextView context = default)
  {
    SourceBuilder source_builder = new(Attributes, Name, InnerType, context);
    return BuildSource(source_builder);
  }
}
