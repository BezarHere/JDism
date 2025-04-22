namespace JDism.Attributes;


readonly record struct BootstrapMethodRef(string Name, ushort Index);
readonly record struct BootstrapMethod(BootstrapMethodRef Reference, Constant[] Arguments)
{
  public readonly override string ToString()
  {
    return $"{Reference.Name} args[{string.Join<Constant>(", ", Arguments)}]";
  }
}

[Register(JAttributeType.BootstrapMethods)]
class BootstrapMethodsJAttribute(IEnumerable<BootstrapMethod> methods)
  : JAttribute
{
  public BootstrapMethod[] Methods = [.. methods];

  public override string ToString()
  {
    return $"@BootstrapMethods({string.Join(", ", Methods)})";
  }
}


