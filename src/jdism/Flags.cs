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
	JVMAttribute = 0x2000,
	Enum = 0x4000,
}


public static class AccessFlagsUtility
{

	public static IEnumerable<string> ToString(FieldAccessFlags accessFlags)
	{
		if (accessFlags.HasFlag(FieldAccessFlags.Public))
		{
			yield return "public";
		}
		else if (accessFlags.HasFlag(FieldAccessFlags.Private))
		{
			yield return "private";
		}
		else if (accessFlags.HasFlag(FieldAccessFlags.Protected))
		{
			yield return "protected";
		}

		if (accessFlags.HasFlag(FieldAccessFlags.Static))
		{
			yield return "static";
		}

		if (accessFlags.HasFlag(FieldAccessFlags.Final))
		{
			yield return "final";
		}

		if (accessFlags.HasFlag(FieldAccessFlags.Volatile))
		{
			yield return "volatile";
		}

		if (accessFlags.HasFlag(FieldAccessFlags.Transient))
		{
			yield return "transient";
		}
	}

	public static IEnumerable<string> ToString(MethodAccessFlags accessFlags)
	{
		if (accessFlags.HasFlag(MethodAccessFlags.Public))
		{
			yield return "public";
		}
		else if (accessFlags.HasFlag(MethodAccessFlags.Private))
		{
			yield return "private";
		}
		else if (accessFlags.HasFlag(MethodAccessFlags.Protected))
		{
			yield return "protected";
		}

		if (accessFlags.HasFlag(MethodAccessFlags.Static))
		{
			yield return "static";
		}

		if (accessFlags.HasFlag(MethodAccessFlags.Final))
		{
			yield return "final";
		}

		else if (accessFlags.HasFlag(MethodAccessFlags.Abstract))
		{
			yield return "abstract";
		}
	}

	public static IEnumerable<string> ToString(ClassAccessFlags accessFlags)
	{
		if (accessFlags.HasFlag(ClassAccessFlags.Public))
		{
			yield return "public";
		}

		if (accessFlags.HasFlag(ClassAccessFlags.Final))
		{
			yield return "final";
		}

		if (accessFlags.HasFlag(ClassAccessFlags.Enum))
		{
			yield return "enum";
		}
		else if (accessFlags.HasFlag(ClassAccessFlags.Interface))
		{
			yield return "interface";
		}
		else if (accessFlags.HasFlag(ClassAccessFlags.Abstract))
		{
			yield return "abstract class";
		}
		else
		{
			yield return "class";
		}
	}


}

