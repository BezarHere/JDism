namespace JDism;

class Field : Member<FieldAccessFlags>
{
  protected override string BuildSource(SourceBuilder builder)
  {
    return builder.BuildField(AccessFlags);
  }
}
