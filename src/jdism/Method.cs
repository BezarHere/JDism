namespace JDism;

class Method : Member<MethodAccessFlags>
{

  protected override string BuildSource(SourceBuilder builder)
  {
    return builder.BuildMethod(AccessFlags);
  }

}
