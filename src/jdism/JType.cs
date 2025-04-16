using System.Text;

namespace JDism;

public class JType
{
	public JTypeType Type;
	public ushort ArrayDimension = 0;
	public string ObjectType = "";
	public JType[] MethodParameters = [];
	public JType ReturnType;
	public JType[] Generics = [];


	public JType()
	{
		Type = JTypeType.Object;
	}

	public JType(JStringReader reader)
	{
		if (reader.EOF) return;

		if (reader.String.StartsWith("<init>") || reader.String.StartsWith("<cinit>"))
		{
			Type = JTypeType.Method;
			// Logger.WriteLine($"found a special function: {reader.String}");
			return;
		}

		// check for arrays
		ArrayDimension = (ushort)reader.SkipCount('[');

		switch (reader.Read())
		{
			case 'V':
			{
				Type = JTypeType.Void;
				break;
			}
			case 'B':
			{
				Type = JTypeType.Byte;
				break;
			}
			case 'C':
			{
				Type = JTypeType.Char;
				break;
			}
			case 'D':
			{
				Type = JTypeType.Double;
				break;
			}
			case 'F':
			{
				Type = JTypeType.Float;
				break;
			}
			case 'I':
			{
				Type = JTypeType.Int;
				break;
			}
			case 'J':
			{
				Type = JTypeType.Long;
				break;
			}
			case 'S':
			{
				Type = JTypeType.Short;
				break;
			}
			case 'Z':
			{
				Type = JTypeType.Boolean;
				break;
			}
			// TYPE
			case 'L':
			{
				Type = JTypeType.Object;

				int semicolon_pos = reader.IndexOf(';');
				if (semicolon_pos == -1)
				{
					throw new InvalidDataException($"\nObject Type Does Not have The Terminating Semicolon: \"{reader}\"");
				}

				ObjectType = reader.ReadUntil(c => c == ';');
				reader.Skip(); // skip ';'



				break;
			}
			// method
			case '(':
			{
				Type = JTypeType.Method;

				int end_para = reader.IndexOf(')');
				// no closing parenthesis
				if (end_para == -1)
				{
					throw new InvalidDataException($"Ill-Formed Method JType: \"{reader}\"");
				}


				// read every thing between the '(' & ')'
				string parameters_typing = reader.ReadUntil(c => c == ')');
				reader.Skip(); // skip ')'

				try
				{
					ReturnType = new JType(reader);
				}
				catch (Exception exc)
				{
					throw new FormatException(
            $"Error While Parsing Return Type For Method JType: \"{reader}\", old_exc={exc}"
          );
				}

				List<JType> parameters = new(8);
				JStringReader parameters_reader = new(parameters_typing);

				while (parameters_reader) // not EOF
				{
					// TODO: CATCH EXCEPTIONS FOR MORE DETAILS
					JType type = new(parameters_reader);

					parameters.Add(type);
				}

				MethodParameters = parameters.ToArray();

				break;
			}
			default:
			{
				throw new InvalidDataException($"Invalid Encoding For JType: \"{reader}\"");
			}
		}

		if (reader.Peek() == '<')
		{
			reader.Skip(); // skips '<'

			int end_arrow = reader.IndexOf('>');
			if (end_arrow == -1)
			{
				throw new InvalidDataException($"Invalid generics {reader}");
			}

			string generics_raw = reader.ReadUntil(c => c == '>');
			reader.Skip(); // skips '>'


			List<JType> generics = new(8);

			JStringReader generics_reader = new(generics_raw);
			while (generics_reader) // not EOF
			{
				// TODO: CATCH EXCEPTIONS FOR MORE DETAILS
				JType type = new(generics_reader);

				generics.Add(type);
			}

			Generics = generics.ToArray();
		}

	}

	public JType(JTypeType type, ushort arr_d, string obj_type)
	{
		Type = type;
		ArrayDimension = arr_d;
		ObjectType = obj_type;
	}


	public override string ToString()
	{
		StringBuilder stringBuilder = new(8);

		switch (Type)
		{
			case JTypeType.Void:
				stringBuilder.Append("void");
				break;
			case JTypeType.Byte:
				stringBuilder.Append("byte");
				break;
			case JTypeType.Char:
				stringBuilder.Append("char");
				break;
			case JTypeType.Double:
				stringBuilder.Append("double");
				break;
			case JTypeType.Float:
				stringBuilder.Append("float");
				break;
			case JTypeType.Int:
				stringBuilder.Append("int");
				break;
			case JTypeType.Long:
				stringBuilder.Append("long");
				break;
			case JTypeType.Object:
				stringBuilder.Append(GetSimplifiedObjectType());
				break;
			case JTypeType.Short:
				stringBuilder.Append("short");
				break;
			case JTypeType.Boolean:
				stringBuilder.Append("boolean");
				break;
			case JTypeType.Method:
				// TODO?
				stringBuilder.Append($"Function<{ReturnType?.ToString()}, ...>");
				break;
			default:
				stringBuilder.Append("Object");
				break;
		}

		if (Generics is not null && Generics.Length != 0)
		{
			stringBuilder.Append('<');
			for (int i = 0; i < Generics.Length; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}

				stringBuilder.Append(Generics[i].ToString());
			}
			stringBuilder.Append('>');
		}

		for (ushort i = 0; i < ArrayDimension; i++)
			stringBuilder.Append("[]");

		return stringBuilder.ToString();
	}

	// for object/ref types, returns the package that they belong to
	// if the type is not an object/ref, an empty string will be returned
	public string GetPackageName()
	{
		if (this.Type == JTypeType.Object)
		{
			return ObjectType.Substring(0, ObjectType.LastIndexOf('/'));
		}
		return string.Empty;
	}

	// for object/ref types, returns the package that they belong to
	// if the type is not an object/ref, an empty string will be returned
	public string GetObjectName()
	{
		if (this.Type == JTypeType.Object)
		{
			return ObjectType.Substring(ObjectType.LastIndexOf('/') + 1).Replace('$', '.');
		}
		return string.Empty;
	}

	// cleans the type path, for example java/lang/object -> object or net/example/com/SomeType -> SomeType
	// subclasses 'Type$SubType' will be cleaned to 'Type.SubType'
	public string GetSimplifiedObjectType()
	{
		if (GetPackageName().StartsWith("java/"))
			return GetObjectName();
		return ObjectType;
	}

}
