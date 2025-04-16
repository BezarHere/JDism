namespace JDism;

public enum AttributeType : ushort
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

public class Attribute
{
	public struct ConstantValueInfo
	{
		public ushort Index;
	}
	public struct SignatureInfo
	{
		public ushort Index;
		public string Value;
	}
	public record ExceptionRecord(ushort StartPc, ushort EndPc, ushort HandlerPc, ushort CatchType);

	public struct CodeInfo
	{

		public ushort MaxStack;
		public ushort MaxLocals;
		public Instruction[] Instructions;

		public ExceptionRecord[] ExceptionRecords;
		public Attribute[] Attributes;
	}

	public record VerificationTypeRecord(byte Type, ushort Index);

	public struct StackMapFrameInfo
	{
		public byte Type = 255;
		public ushort OffsetDelta = 0;
		public VerificationTypeRecord[] Locals = [];
		public VerificationTypeRecord[] Stack = [];

		public StackMapFrameInfo()
		{
		}
	}

	public AttributeType Type;
	public ushort NameIndex;
	public string Name = "";



	public ConstantValueInfo ConstantValue;
	public SignatureInfo Signature;
	public CodeInfo Code;
	public StackMapFrameInfo[] StackMapTable = [];

	// for custom defined attributes
	public byte[] _data = [];
}
