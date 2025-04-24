namespace JDism.attribute;


readonly record struct BootstrapMethodRef(string Name, ushort Index);
readonly record struct BootstrapMethod(BootstrapMethodRef Reference, Constant[] Arguments)
{
  public readonly override string ToString()
  {
    return $"{Reference.Name} args[{string.Join<Constant>(", ", Arguments)}]";
  }
}

[Register(AnnotationType.BootstrapMethods)]
class BootstrapMethodsAnnotation(IEnumerable<BootstrapMethod> methods)
  : JVMAttribute
{
  public BootstrapMethod[] Methods = [.. methods];

  public override string ToString()
  {
    return $"@BootstrapMethods({string.Join(", ", Methods)})";
  }
}


