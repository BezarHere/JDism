
namespace JDism.attribute;

enum AnnotationElementTag
{
  ByteTag = (byte)'B', // const_value_index	CONSTANT_Integer
  CharTag = (byte)'C', // const_value_index	CONSTANT_Integer
  DoubleTag = (byte)'D', // const_value_index	CONSTANT_Double
  FloatTag = (byte)'F', // const_value_index	CONSTANT_Float
  IntTag = (byte)'I', // const_value_index	CONSTANT_Integer
  LongTag = (byte)'J', // const_value_index	CONSTANT_Long
  ShortTag = (byte)'S', // const_value_index	CONSTANT_Integer
  BooleanTag = (byte)'Z', // const_value_index	CONSTANT_Integer
  StringTag = (byte)'s', // const_value_index	CONSTANT_Utf8
  EnumTag = (byte)'e', // type	enum_const_value	Not applicable
  ClassTag = (byte)'c', // class_info_index	Not applicable
  AnnotationTag = (byte)'@', // type	annotation_value	Not applicable
  ArrayTag = (byte)'[', // type	array_value	Not applicable
};

struct AnnotationElementValue
{
  public AnnotationElementTag Tag;
  
  Constant _constant = null;
  Annotation _annotation = null;
  AnnotationElementValue[] _children = null;

  public AnnotationElementValue()
  {
  }
};

record Annotation(ushort NameIndex, string Name, AnnotationElementValue[] Elements);


// [Register(JVMAttributeType.RuntimeVisibleAnnotations)]
class RuntimeVisibleAnnotation : JVMAttribute
{

}
