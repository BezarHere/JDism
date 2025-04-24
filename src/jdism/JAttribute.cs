using System.Diagnostics;
using System.Reflection;

namespace JDism;

public enum JVMAttributeType : ushort
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
  internal class RegisterAttribute(JVMAttributeType type, string name = null) : Attribute
  {
    public readonly JVMAttributeType Type = type;
    public readonly string Name = string.IsNullOrEmpty(name) ? type.ToString() : name;
  }


  public record JVMAttributeTypeInfo(Type InstanceType, JVMAttributeType AttributeType, string Name)
  {
    public JVMAttributeTypeInfo(Type Type, JVMAttributeType AttributeType)
      : this(Type, AttributeType, AttributeType.ToString())
    {
    }
  }

  public JVMAttributeTypeInfo GetTypeInfo() => FetchAttributeTypeInfo(GetType());


  public static JVMAttributeTypeInfo FetchAttributeTypeInfo(string name)
  {
    return sCachedAttributeTypeInfos.FirstOrDefault(
      attr_info => attr_info.Name == name,
      CustomAttributeType
    );
  }

  public static JVMAttributeTypeInfo FetchAttributeTypeInfo(JVMAttributeType type)
  {
    return sCachedAttributeTypeInfos.FirstOrDefault(
      attr_info => attr_info.AttributeType == type,
      CustomAttributeType
    );
  }

  public static JVMAttributeTypeInfo FetchAttributeTypeInfo(Type type)
  {
    Debug.Assert(type.IsAssignableTo(typeof(JVMAttribute)));

    return sCachedAttributeTypeInfos.FirstOrDefault(
      attr_info => attr_info.InstanceType == type,
      CustomAttributeType
    );
  }

  public static JVMAttributeTypeInfo FetchAttributeTypeInfo<T>() where T : JVMAttribute
  {
    return sCachedAttributeTypeInfos.FirstOrDefault(
      attr_info => attr_info.InstanceType == typeof(T),
      CustomAttributeType
    );
  }

  private static IEnumerable<JVMAttributeTypeInfo> ScanForAttributeTypeInfos()
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
      Debug.Assert(attr.Type != JVMAttributeType._Max);
      Console.WriteLine(
        $"[*] Found J-attribute: class {type.FullName}, type={attr.Type}, name='{attr.Name}'"
      );
      yield return new(type, attr.Type, attr.Name);
    }

    for (int i = 0; i < (int)JVMAttributeType._Max; i++)
    {
      JVMAttributeType type = (JVMAttributeType)i;
      yield return new(typeof(UnknownAnnotation), type);
    }
  }


  private static readonly JVMAttributeTypeInfo[] sCachedAttributeTypeInfos = [
    .. ScanForAttributeTypeInfos()
  ];
  private static readonly JVMAttributeTypeInfo CustomAttributeType =
    new(typeof(CustomAnnotation), JVMAttributeType.UserDefined);
}


