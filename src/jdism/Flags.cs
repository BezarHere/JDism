namespace JDism;


[Flags]
public enum MethodAccessFlags
{
	None = 0,
	Public = 0x0001,
	Private = 0x0002,
	Protected = 0x0004,
	Static = 0x0008,
	Final = 0x0010,
	Synchronized = 0x0020,
	Bridge = 0x0040,
	VarArgs = 0x0080,
	Native = 0x0100,
	Abstract = 0x0400,
	Strict = 0x0800,
	Synthetic = 0x1000,
}

[Flags]
public enum FieldAccessFlags
{
	None = 0,
	Public = 0x0001,
	Private = 0x0002,
	Protected = 0x0004,
	Static = 0x0008,
	Final = 0x0010,

	Volatile = 0x0040,
	Transient = 0x0080,

	Synthetic = 0x1000,
	Enum = 0x4000,
}

[Flags]
public enum ClassAccessFlags
{
	None = 0,
	Public = 0x0001,


	Final = 0x0010,
	Super = 0x0020,

	Interface = 0x0200,
	Abstract = 0x0400,

	Synthetic = 0x1000,
	Annotation = 0x2000,
	Enum = 0x4000,
}


public static class AccessFlagsUtility
{

	public static void ToString(Action<string> loader, FieldAccessFlags accessFlags)
	{
		if (accessFlags.HasFlag(FieldAccessFlags.Public))
		{
			loader("public");
		}
		else if (accessFlags.HasFlag(FieldAccessFlags.Private))
		{
			loader("private");
		}
		else if (accessFlags.HasFlag(FieldAccessFlags.Protected))
		{
			loader("protected");
		}

		if (accessFlags.HasFlag(FieldAccessFlags.Static))
		{
			loader("static");
		}

		if (accessFlags.HasFlag(FieldAccessFlags.Final))
		{
			loader("final");
		}

		if (accessFlags.HasFlag(FieldAccessFlags.Volatile))
		{
			loader("volatile");
		}

		if (accessFlags.HasFlag(FieldAccessFlags.Transient))
		{
			loader("transient");
		}
	}

	public static void ToString(Action<string> loader, MethodAccessFlags accessFlags)
	{
		if (accessFlags.HasFlag(MethodAccessFlags.Public))
		{
			loader("public");
		}
		else if (accessFlags.HasFlag(MethodAccessFlags.Private))
		{
			loader("private");
		}
		else if (accessFlags.HasFlag(MethodAccessFlags.Protected))
		{
			loader("protected");
		}

		if (accessFlags.HasFlag(MethodAccessFlags.Static))
		{
			loader("static");
		}

		if (accessFlags.HasFlag(MethodAccessFlags.Final))
		{
			loader("final");
		}

		else if (accessFlags.HasFlag(MethodAccessFlags.Abstract))
		{
			loader("abstract");
		}
	}

	public static void ToString(Action<string> loader, ClassAccessFlags accessFlags)
	{
		if (accessFlags.HasFlag(ClassAccessFlags.Public))
		{
			loader("public");
		}

		if (accessFlags.HasFlag(ClassAccessFlags.Final))
		{
			loader("final");
		}

		if (accessFlags.HasFlag(ClassAccessFlags.Enum))
		{
			loader("enum");
		}
		else if (accessFlags.HasFlag(ClassAccessFlags.Interface))
		{
			loader("interface");
		}
		else if (accessFlags.HasFlag(ClassAccessFlags.Abstract))
		{
			loader("abstract class");
		}
		else
		{
			loader("class");
		}
	}


}

