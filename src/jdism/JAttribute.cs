using System.Diagnostics;
using System.Reflection;

namespace JDism;

public enum AnnotationType : ushort
{
  None = 0,
  ConstantValue,
  Code,
  StackMapTable,
  Exceptions,
  BootstrapMethods,
  InnerClasses,
  EnclosingMethod,
  Synthetic,
  Signature,
  RuntimeVisibleAnnotations,
  RuntimeInvisibleAnnotations,
  RuntimeVisibleParameterAnnotations,
  RuntimeInvisibleParameterAnnotations,
  RuntimeVisibleTypeAnnotations,
  RuntimeInvisibleTypeAnnotations,
  AnnotationDefault,
  MethodParameters,
  SourceFile,
  SourceDebugExtension,
  LineNumberTable,
  LocalVariableTable,
  LocalVariableTypeTable,
  Deprecated,
  _Max,
  UserDefined = 0xffff,
}

public abstract class JVMAttribute
{
  [AttributeUsage(AttributeTargets.Class)]
  internal class RegisterAttribute(AnnotationType type, string name = null) : Attribute
  {
    public readonly AnnotationType Type = type;
    public readonly string Name = string.IsNullOrEmpty(name) ? type.ToString() : name;
  }


  public record AnnotationTypeInfo(Type InstanceType, AnnotationType AttributeType, string Name)
  {
    public AnnotationTypeInfo(Type Type, AnnotationType AttributeType)
      : this(Type, AttributeType, AttributeType.ToString())
    {
    }
  }

  public AnnotationTypeInfo GetTypeInfo() => FetchAttributeTypeInfo(GetType());


  public static AnnotationTypeInfo FetchAttributeTypeInfo(string name)
  {
    return sCachedAttributeTypeInfos.FirstOrDefault(
      attr_info => attr_info.Name == name,
      CustomAttributeType
    );
  }

  public static AnnotationTypeInfo FetchAttributeTypeInfo(AnnotationType type)
  {
    return sCachedAttributeTypeInfos.FirstOrDefault(
      attr_info => attr_info.AttributeType == type,
      CustomAttributeType
    );
  }

  public static AnnotationTypeInfo FetchAttributeTypeInfo(Type type)
  {
    Debug.Assert(type.IsAssignableTo(typeof(JVMAttribute)));

    return sCachedAttributeTypeInfos.FirstOrDefault(
      attr_info => attr_info.InstanceType == type,
      CustomAttributeType
    );
  }

  public static AnnotationTypeInfo FetchAttributeTypeInfo<T>() where T : JVMAttribute
  {
    return sCachedAttributeTypeInfos.FirstOrDefault(
      attr_info => attr_info.InstanceType == typeof(T),
      CustomAttributeType
    );
  }

  private static IEnumerable<AnnotationTypeInfo> ScanForAttributeTypeInfos()
  {
    var assembly = Assembly.GetExecutingAssembly();
    Console.WriteLine($"assembly={assembly}");
    var registered_classes =
      from t in assembly.DefinedTypes
      where t.IsClass && t.BaseType == typeof(JVMAttribute)
      where t.CustomAttributes.Any(a => a.AttributeType == typeof(RegisterAttribute))
      select (t.AsType(), t.GetCustomAttribute<RegisterAttribute>());
    registered_classes = registered_classes.DistinctBy(tpl => tpl.Item2.Type);

    foreach ((Type type, RegisterAttribute attr) in registered_classes)
    {
      Debug.Assert(attr.Type != AnnotationType._Max);
      Console.WriteLine(
        $"[*] Found J-attribute: class {type.FullName}, type={attr.Type}, name='{attr.Name}'"
      );
      yield return new(type, attr.Type, attr.Name);
    }

    for (int i = 0; i < (int)AnnotationType._Max; i++)
    {
      AnnotationType type = (AnnotationType)i;
      yield return new(typeof(UnknownAnnotation), type);
    }
  }


  private static readonly AnnotationTypeInfo[] sCachedAttributeTypeInfos = [
    .. ScanForAttributeTypeInfos()
  ];
  private static readonly AnnotationTypeInfo CustomAttributeType =
    new(typeof(CustomAnnotation), AnnotationType.UserDefined);
}


