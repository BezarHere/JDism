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
  UserDefined = 0xffff,
}

public abstract class JAttribute
{
  public record AttributeTypeInfo(Type InstanceType, JAttributeType AttributeType, string Name)
  {
    public AttributeTypeInfo(Type Type, JAttributeType AttributeType)
      : this(Type, AttributeType, AttributeType.ToString())
    {
    }
  }

  public static readonly AttributeTypeInfo[] AttributeTypeInfos = [
    new(typeof(ConstantValueInfoJAttribute), JAttributeType.ConstantValue),
    new(typeof(SignatureJAttribute), JAttributeType.Signature),
    new(typeof(CodeInfoJAttribute), JAttributeType.Code),
    new(typeof(StackMapFrameInfoJAttribute), JAttributeType.StackMapTable),

    new(typeof(UnknownJAttribute), JAttributeType.Exceptions),
    new(typeof(UnknownJAttribute), JAttributeType.BootstrapMethods),
    new(typeof(UnknownJAttribute), JAttributeType.InnerClasses),
    new(typeof(UnknownJAttribute), JAttributeType.EnclosingMethod),
    new(typeof(UnknownJAttribute), JAttributeType.Synthetic),
    new(typeof(UnknownJAttribute), JAttributeType.Signature),
    new(typeof(UnknownJAttribute), JAttributeType.RuntimeVisibleAnnotations),
    new(typeof(UnknownJAttribute), JAttributeType.RuntimeInvisibleAnnotations),
    new(typeof(UnknownJAttribute), JAttributeType.RuntimeVisibleParameterAnnotations),
    new(typeof(UnknownJAttribute), JAttributeType.RuntimeInvisibleParameterAnnotations),
    new(typeof(UnknownJAttribute), JAttributeType.RuntimeVisibleTypeAnnotations),
    new(typeof(UnknownJAttribute), JAttributeType.RuntimeInvisibleTypeAnnotations),
    new(typeof(UnknownJAttribute), JAttributeType.AnnotationDefault),
    new(typeof(UnknownJAttribute), JAttributeType.MethodParameters),
    new(typeof(UnknownJAttribute), JAttributeType.SourceFile),
    new(typeof(UnknownJAttribute), JAttributeType.SourceDebugExtension),
    new(typeof(UnknownJAttribute), JAttributeType.LineNumberTable),
    new(typeof(UnknownJAttribute), JAttributeType.LocalVariableTable),
    new(typeof(UnknownJAttribute), JAttributeType.LocalVariableTypeTable),
    new(typeof(UnknownJAttribute), JAttributeType.Deprecated),

  ];
  public static readonly AttributeTypeInfo CustomAttributeType =
    new(typeof(CustomJAttribute), JAttributeType.UserDefined);

  public AttributeTypeInfo GetTypeInfo() => FetchAttributeTypeInfo(GetType());


  public static AttributeTypeInfo FetchAttributeTypeInfo(string name)
  {
    return AttributeTypeInfos.FirstOrDefault(
      attr_info => attr_info.Name == name,
      CustomAttributeType
    );
  }

  public static AttributeTypeInfo FetchAttributeTypeInfo(JAttributeType type)
  {
    return AttributeTypeInfos.FirstOrDefault(
      attr_info => attr_info.AttributeType == type,
      CustomAttributeType
    );
  }

  public static AttributeTypeInfo FetchAttributeTypeInfo(Type type)
  {
    Debug.Assert(type.IsAssignableTo(typeof(JAttribute)));

    return AttributeTypeInfos.FirstOrDefault(
      attr_info => attr_info.InstanceType == type,
      CustomAttributeType
    );
  }

  public static AttributeTypeInfo FetchAttributeTypeInfo<T>() where T : JAttribute
  {
    return AttributeTypeInfos.FirstOrDefault(
      attr_info => attr_info.InstanceType == typeof(T),
      CustomAttributeType
    );
  }




}

public class ConstantValueInfoJAttribute : JAttribute
{
  public Constant Constant;
  public ushort Index;


  public override string ToString()
  {
    return $"@Constant({Constant})";
  }
}

public class SignatureJAttribute : JAttribute
{
  public ushort Index;
  public JType Signature;

  public override string ToString()
  {
    return $"@Signature({Signature})";
  }
}

public record ExceptionRecord(ushort StartPc, ushort EndPc, ushort HandlerPc, ushort CatchType);

public class CodeInfoJAttribute : JAttribute
{

  public ushort MaxStack;
  public ushort MaxLocals;
  public Instruction[] Instructions;

  public ExceptionRecord[] ExceptionRecords;
  public Attribute[] Attributes;

  public override string ToString()
  {
    string instructions_str = string.Join(", ", Instructions);
    return $"locals={MaxLocals}, stack_size={MaxStack}, code=[{instructions_str}]";
  }

}

public record VerificationTypeRecord(byte Type, ushort Index);

public class StackMapFrameInfoJAttribute : JAttribute
{
  public byte Type = 255;
  public ushort OffsetDelta = 0;
  public VerificationTypeRecord[] Locals = [];
  public VerificationTypeRecord[] Stack = [];

  public StackMapFrameInfoJAttribute()
  {
  }

}

public class UnknownJAttribute(JAttributeType type, byte[] data) : JAttribute
{
  public JAttributeType Type => type;
  public byte[] Data => data;

  public override string ToString()
  {
    string data_str = string.Join(", ", from i in Data select i.ToString("X"));
    return $"@{Type}({data_str})";
  }
}

public class CustomJAttribute(byte[] data) : JAttribute
{
  public byte[] Data => data;

  public override string ToString()
  {
    string data_str = string.Join(", ", from i in Data select i.ToString("X"));
    return $"@CustomAttribute({data_str})";
  }
}

