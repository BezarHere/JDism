using System.Diagnostics;
using System.Text;

namespace JDism;

public enum JTypeKind : byte
{
  Unknown = 0,
  // used for example to mark the end of type parameters in functions
  Marker = (byte)'*',
  TypeParameter = (byte)'T',
  TypeParameterSpecification = (byte)':',
  Class = (byte)'X',

  Array = (byte)'[',
  Void = (byte)'V',
  // signed byte
  Byte = (byte)'B',
  // utf16
  Char = (byte)'C',
  Double = (byte)'D',
  Float = (byte)'F',
  Int = (byte)'I',
  Long = (byte)'J',
  Object = (byte)'L',
  Short = (byte)'S',
  Boolean = (byte)'Z',
  Method = (byte)'M',
}

public enum JTypeParseContext : byte
{
  Basic,
  TypeParameter,
  MethodParameter,
  Class
};

public struct JType
{
  static class Globals
  {
    public static bool StripObjectNames = true;
  }

  public JTypeKind Kind = JTypeKind.Unknown;
  public string Name = "";
  public JType[] Children = [];

  public int ReadLength { get; init; }

  
  public const string SpacialNamePlaceholder = "[NAME]";

  public static readonly string[] sSpacialMethods = ["<init>", "<clinit>"];
  public static readonly Dictionary<JTypeKind, string> sBasicTypeNames = new Dictionary<JTypeKind, string>{
    {JTypeKind.Void, "void"},
    {JTypeKind.Byte, "byte"},
    {JTypeKind.Char, "char"},
    {JTypeKind.Double, "double"},
    {JTypeKind.Float, "float"},
    {JTypeKind.Int, "int"},
    {JTypeKind.Long, "long"},
    {JTypeKind.Short, "short"},
    {JTypeKind.Boolean, "boolean"},
  };
  public static readonly JTypeKind[] sBasicTypes = [.. sBasicTypeNames.Keys];

  public readonly bool Valid { get => Kind != JTypeKind.Unknown && Kind != JTypeKind.Marker; }

  public JType(JTypeKind kind = JTypeKind.Unknown)
  {
    this.Kind = kind;
  }

  public JType(string source, JTypeParseContext context = JTypeParseContext.Basic)
  {
    if (string.IsNullOrEmpty(source))
    {
      ReadLength = 0;
      return;
    }

    foreach (string s in sSpacialMethods)
    {
      if (source.StartsWith(s))
      {
        Kind = JTypeKind.Method;
        Name = s;
        ReadLength = s.Length;
        // Logger.WriteLine($"found a special function: {reader.String}");
        return;
      }
    }

    // array?
    if (source[0] == '[')
    {
      Kind = JTypeKind.Array;
      Children = [new JType(source[1..])];
      Name = Children[0].Name + "[]";
      ReadLength = 1 + Children[0].ReadLength;
      return;
    }

    // type parameter specification can be just like a basic type
    // ex: boolean is 'Z', but a type parameter specification can be 'Z:Ljava/lang/object'
    if (context == JTypeParseContext.TypeParameter)
    {
      Range tpd_range = TryParseTypeParameterDeclarationName(source, 0);
      if (tpd_range.End.Value > tpd_range.Start.Value)
      {
        Debug.Assert(source[tpd_range.End] == ':');

        Kind = JTypeKind.TypeParameterSpecification;
        Children = [new(source[(tpd_range.End.Value + 1)..])];
        Name = $"{source[tpd_range]} extends {Children[0].Name}";

        int range_length = tpd_range.End.Value - tpd_range.Start.Value;
        ReadLength = range_length + 1 + Children[0].ReadLength;
        return;
      }
    }

    // basic types
    foreach (JTypeKind t in sBasicTypes)
    {
      if (source[0] == (char)t)
      {
        Kind = t;
        ReadLength = 1;
        Name = sBasicTypeNames[t];
        return;
      }
    }

    // functions only (to my knowledge) are prefixed with type parameters
    if (source[0] == '<')
    {
      Children = [.. TryParseTypeParameters(source, 0), new(JTypeKind.Marker)];

      int type_parameters_read_length =
        Children.Aggregate(0, (acc, t) => acc + t.ReadLength);

      if (type_parameters_read_length > 0)
      {
        type_parameters_read_length += 2; // for the '<' and '>'

        // we SHOULD have either a function after this
        // or a superclass,..interfaces depending on context
        Debug.Assert(source.Length > type_parameters_read_length);
        char expected_after = context == JTypeParseContext.Class ? 'L' : '(';
        Debug.Assert(source[type_parameters_read_length] == expected_after);
      }

      ReadLength = type_parameters_read_length;
      source = source[type_parameters_read_length..];
    }

    // class signature
    if (context == JTypeParseContext.Class && source[0] == (char)JTypeKind.Object)
    {
      // and the interfaces
      List<JType> super_classes = [];

      int index = 0;
      while (index < source.Length)
      {
        super_classes.Add(new(source[index..], JTypeParseContext.Basic));
        Debug.Assert(super_classes.Last().ReadLength != 0);
        index += super_classes.Last().ReadLength;
      }
      
      Kind = JTypeKind.Class;

      {
        StringBuilder builder = new();
        builder.Append(SpacialNamePlaceholder);
        if (Children.Length > 0)
        {
          builder.Append('<');
          builder.Append(string.Join(", ", Children));
          builder.Append('>');
        }

        builder.Append(" extends ");
        builder.Append(super_classes[0]);
        builder.Append(" implements ");
        builder.Append(string.Join(", ", super_classes[1..]));

        Name = builder.ToString();
      }

      Children = [..Children, ..super_classes];
      ReadLength += source.Length;
      return;
    }

    // object? form is "LPackage/Module/Object;"
    // try handle type parameters (template or generics)
    if (source[0] == (char)JTypeKind.Object)
    {
      int name_end_pos = source.IndexOfAny([';', '<']);
      if (name_end_pos == -1)
      {
        throw new InvalidDataException(
          $"Object type name does not have a termination: \"{source}\""
        );
      }

      if (name_end_pos == 1)
      {
        throw new InvalidDataException(
          $"Object type is empty ('L;' or 'L<...>;'): \"{source}\""
        );
      }

      Kind = JTypeKind.Object;
      Children = [.. TryParseTypeParameters(source, name_end_pos)];
      Name = source[1..name_end_pos].Replace('$', '.');

      if (Globals.StripObjectNames)
      {
        int last_slash = Name.LastIndexOf('/');
        Name = Name[(last_slash + 1)..];
      }

      if (Children.Length > 0)
      {
        string type_parameters_stringified = string.Join(
          ", ",
          from p in Children select p.ToString()
        );
        Name = $"{Name}<{type_parameters_stringified}>";
      }

      // handling type parameters
      int type_parameters_read_length =
        Children.Aggregate(0, (acc, t) => acc + t.ReadLength);

      if (type_parameters_read_length > 0)
      {
        Debug.Assert(source[name_end_pos] == '<');

        type_parameters_read_length += 2; // for the '<' and '>'

        Debug.Assert(source[name_end_pos + type_parameters_read_length] == ';');
      }

      // note that type parameters read length will be zero
      // if there is no type parameters
      ReadLength = name_end_pos + type_parameters_read_length + 1;
      return;
    }

    // function? form is "(IDLPackage/Module/Thread;)LPackage/Module/Object;"
    if (source[0] == '(')
    {
      List<JType> parameters = [];

      int index = 1;
      while (index < source.Length)
      {
        if (source[index] == ')')
        {
          break;
        }

        parameters.Add(new JType(source[index..], JTypeParseContext.TypeParameter));
        Debug.Assert(parameters.Last().ReadLength > 0);

        index += parameters.Last().ReadLength;
      }

      Debug.Assert(source[index] == ')');
      index++;

      JType returnType = new(source[index..], JTypeParseContext.TypeParameter);

      bool had_type_parameters = ReadLength != 0;
      if (had_type_parameters)
      {
        Debug.Assert(Children.Length > 0);
        Debug.Assert(Children.Last().Kind == JTypeKind.Marker);
      }

      Kind = JTypeKind.Method;

      {
        StringBuilder name_builder = new();

        name_builder.Append(returnType.Name);
        name_builder.Append(' ');
        name_builder.Append(SpacialNamePlaceholder);

        if (Children.Length > 0)
        {
          name_builder.Append('<');
          name_builder.Append(string.Join(", ", from p in Children select p.Name));
          name_builder.Append('>');
        }

        name_builder.Append('(');
        int param_index = 0;
        name_builder.Append(
          string.Join(
            ", ",
            from p in parameters select p.Name + $" param_{param_index++}"
          )
        );
        name_builder.Append(')');

        Name = name_builder.ToString();
      }

      Children = [.. Children, returnType, .. parameters];
      ReadLength += index + returnType.ReadLength;
      return;
    }

    if (context != JTypeParseContext.TypeParameter)
    {
      throw new FormatException($"Unknown type format in context={context}: '{source}'");
    }

    // context type parameter VVV

    if (source[0] == '*')
    {
      Kind = JTypeKind.TypeParameter;
      Name = "?";
      Children = [];
      ReadLength = 1;
      return;
    }

    if (source[0] == '+' || source[0] == '-')
    {
      Kind = JTypeKind.TypeParameter;
      string super_or_extend = source[0] == '+' ? "extends" : "super";
      Children = [new(source[1..], JTypeParseContext.Basic)];
      Name = $"? {super_or_extend} {Children[0].Name}";
      ReadLength = 1 + Children[0].ReadLength;
      return;
    }

    if (source[0] == 'T')
    {
      int end_index = source.IndexOf(';');
      if (end_index == -1)
      {
        throw new FormatException(
          $"Expecting a type parameter name end (valid ex. 'TT;'): source={source}"
        );
      }

      if (end_index == 1)
      {
        throw new FormatException(
          $"Invalid a type parameter name end (valid ex. 'TE;', found 'T;'): source={source}"
        );
      }

      if (!ValidTypeParameterName(source, 1, end_index))
      {
        throw new FormatException(
          $"Invalid type parameter from {1} to {end_index}, "
          + $"name='{source[1..end_index]}', source='{source}'"
        );
      }

      Kind = JTypeKind.TypeParameter;
      Name = source[1..end_index];
      Children = [];
      ReadLength = end_index + 1;
      return;
    }

    throw new FormatException($"Unhandled type format in context={context}: '{source}'");
  }

  public JType(JStringReader reader)
    : this(reader.String[reader.Index..])
  {
    _ = reader.Read(ReadLength);
  }

  public override readonly string ToString() => Name;

  public readonly string GetObjectName()
  {
    return Name[(Name.LastIndexOf('/') + 1)..];
  }

  public readonly int GetMethodMarkerIndex()
  {
    return Array.IndexOf(Children, (JType i) => i.Kind == JTypeKind.Marker);
  }

  public readonly JType GetMethodReturnType()
  {
    return Children[GetMethodMarkerIndex() + 1];
  }



  public readonly IEnumerable<JType> GetMethodParameters()
  {
    int first_parameter_index = GetMethodMarkerIndex() + 2;
    if (first_parameter_index >= Children.Length)
    {
      return [];
    }

    return Children.Skip(first_parameter_index);
  }

  private static bool IdentifierCharPredicate(char c)
  {
    return char.IsLetter(c) || char.IsDigit(c) || c == '_';
  }

  private static bool ValidTypeParameterName(string source, int start, int end,
                                             bool is_decl = false)
  {
    for (int i = start; i < end; i++)
    {
      if ((is_decl && source[i] == '$') || !IdentifierCharPredicate(source[i]))
      {
        return false;
      }
    }

    return true;
  }

  private static IEnumerable<JType> FindTypeParameters(string source, ref int index)
  {
    int angle_bracket_index = source.IndexOf('<', index);
    if (angle_bracket_index == -1)
    {
      return null;
    }

    index += angle_bracket_index;

    return TryParseTypeParameters(source, index);
  }

  private static IEnumerable<JType> TryParseTypeParameters(string source, int index)
  {
    if (source[index] != '<')
    {
      yield break;
    }

    index += 1;
    while (index < source.Length)
    {
      if (source[index] == '>')
      {
        break;
      }

      JType type = new(source[index..], JTypeParseContext.TypeParameter);
      Debug.Assert(type.ReadLength > 0);

      index += type.ReadLength;
      yield return type;
    }

    yield break;
  }

  private static Range TryParseTypeParameterDeclarationName(string source, int index)
  {
    int colon_index = source.IndexOf(':', index);
    if (colon_index <= index)
    {
      return 0..0;
    }

    if (!ValidTypeParameterName(source, index, colon_index, true))
    {
      return 0..0;
    }

    return new(index, colon_index);
  }
}
