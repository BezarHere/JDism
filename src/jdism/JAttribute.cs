using System.Diagnostics;
using System.Reflection;

namespace JDism;

public enum JAttributeType : ushort
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

public abstract class JAttribute
{
  [AttributeUsage(AttributeTargets.Class)]
  internal class RegisterAttribute(JAttributeType type, string name = null) : Attribute
  {
    public readonly JAttributeType Type = type;
    public readonly string Name = string.IsNullOrEmpty(name) ? type.ToString() : name;
  }


  public record JAttributeTypeInfo(Type InstanceType, JAttributeType AttributeType, string Name)
  {
    public JAttributeTypeInfo(Type Type, JAttributeType AttributeType)
      : this(Type, AttributeType, AttributeType.ToString())
    {
    }
  }

  public JAttributeTypeInfo GetTypeInfo() => FetchAttributeTypeInfo(GetType());


  public static JAttributeTypeInfo FetchAttributeTypeInfo(string name)
  {
    return sCachedAttributeTypeInfos.FirstOrDefault(
      attr_info => attr_info.Name == name,
      CustomAttributeType
    );
  }

  public static JAttributeTypeInfo FetchAttributeTypeInfo(JAttributeType type)
  {
    return sCachedAttributeTypeInfos.FirstOrDefault(
      attr_info => attr_info.AttributeType == type,
      CustomAttributeType
    );
  }

  public static JAttributeTypeInfo FetchAttributeTypeInfo(Type type)
  {
    Debug.Assert(type.IsAssignableTo(typeof(JAttribute)));

    return sCachedAttributeTypeInfos.FirstOrDefault(
      attr_info => attr_info.InstanceType == type,
      CustomAttributeType
    );
  }

  public static JAttributeTypeInfo FetchAttributeTypeInfo<T>() where T : JAttribute
  {
    return sCachedAttributeTypeInfos.FirstOrDefault(
      attr_info => attr_info.InstanceType == typeof(T),
      CustomAttributeType
    );
  }

  private static IEnumerable<JAttributeTypeInfo> ScanForAttributeTypeInfos()
  {
    var assembly = Assembly.GetExecutingAssembly();
    Console.WriteLine($"assembly={assembly}");
    var registered_classes =
      from t in assembly.DefinedTypes
      where t.IsClass && t.BaseType == typeof(JAttribute)
      where t.CustomAttributes.Any(a => a.AttributeType == typeof(RegisterAttribute))
      select (t.AsType(), t.GetCustomAttribute<RegisterAttribute>());
    registered_classes = registered_classes.DistinctBy(tpl => tpl.Item2.Type);

    foreach ((Type type, RegisterAttribute attr) in registered_classes)
    {
      Debug.Assert(attr.Type != JAttributeType._Max);
      Console.WriteLine(
        $"[*] Found J-attribute: class {type.FullName}, type={attr.Type}, name='{attr.Name}'"
      );
      yield return new(type, attr.Type, attr.Name);
    }

    for (int i = 0; i < (int)JAttributeType._Max; i++)
    {
      JAttributeType type = (JAttributeType)i;
      yield return new(typeof(UnknownJAttribute), type);
    }
  }


  private static readonly JAttributeTypeInfo[] sCachedAttributeTypeInfos = [
    .. ScanForAttributeTypeInfos()
  ];
  private static readonly JAttributeTypeInfo CustomAttributeType =
    new(typeof(CustomJAttribute), JAttributeType.UserDefined);
}


