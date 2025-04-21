using System.Diagnostics;
using System.Text;

namespace JDism;


public enum ConstantType
{
  None = 0,
  String,
  Integer = 3,
  Float,
  Long,
  Double,
  Class,
  StringReference,
  FieldReference,
  MethodReference,
  InterfaceMethodReference,
  NameTypeDescriptor,
  MethodHandle = 15,
  MethodType,
  Dynamic,
  InvokeDynamic,
  //Module,
  //Package

}

public class Constant
{
  public ConstantType type;

  public int IntegerValue { get => (int)long_int; set => long_int = value; }
  public long LongValue { get => long_int; set => long_int = value; }

  public float FloatValue { get => (float)double_float; set => double_float = value; }
  public double DoubleValue { get => double_float; set => double_float = value; }

  public ushort ClassIndex { get => index1; set => index1 = value; }
  public ushort StringIndex { get => index1; set => index1 = value; }
  public ushort NameTypeIndex { get => index2; set => index2 = value; }
  public ushort NameIndex { get => index1; set => index1 = value; }
  public ushort DescriptorIndex { get => index2; set => index2 = value; }
  public MethodReferenceKind ReferenceKind { get => (MethodReferenceKind)index1; set => index1 = (ushort)value; }
  public ushort ReferenceIndex { get => index2; set => index2 = value; }

  public ushort BootstrapMethodAttrIndex { get => index1; set => index1 = value; }

  public string String { get; set; } = "";

  private ushort index1;
  private ushort index2;
  private long long_int;
  private double double_float;

  public bool IsDoubleSlotted()
  {
    return type == ConstantType.Double || type == ConstantType.Long;
  }

  public override string ToString()
  {
    if (type == ConstantType.String)
    {
      return $"\"{String}\"";
    }
    return String;
  }

  internal static void SetupRepresentation(Constant constant, JContextView ctx)
  {
    var logger = ctx.Logger.Logger;
    switch (constant.type)
    {
      case ConstantType.Integer:
        {
          constant.String = constant.IntegerValue.ToString();
          break;
        }
      case ConstantType.Float:
        {
          constant.String = constant.FloatValue.ToString();
          break;
        }
      case ConstantType.Long:
        {
          constant.String = constant.LongValue.ToString();
          break;
        }
      case ConstantType.Double:
        {
          constant.String = constant.DoubleValue.ToString();
          break;
        }
      case ConstantType.String:
        {
          break;
        }
      case ConstantType.Class:
        {
          constant.String = ctx.Constants[constant.NameIndex - 1].String.Replace('$', '.');
          logger.WriteLine($"TYPE: \"{constant.String}\"");
          break;
        }
      case ConstantType.FieldReference:
      case ConstantType.MethodReference:
      case ConstantType.InterfaceMethodReference:
        {
          string class_name = ctx.Constants[constant.ClassIndex - 1].String;
          string name_type_name = ctx.Constants[constant.NameTypeIndex - 1].String;
          constant.String = $"{class_name}:{name_type_name}";
          logger.WriteLine($"REF: \"{constant.String}\"");
          break;
        }
      case ConstantType.StringReference:
        {
          constant.String = $"&\"{ctx.Constants[constant.NameIndex - 1].String}\"";
          logger.WriteLine($"CONSTANT STRING: \"{constant.String}\"");
          break;
        }
      case ConstantType.NameTypeDescriptor:
        {
          SourceBuilder builder = new(
            [],
            ctx.Constants[constant.NameIndex - 1].String,
            new JType(ctx.Constants[constant.DescriptorIndex - 1].String),
            ctx
          );

          if (builder.Type?.Kind == JTypeKind.Method)
          {
            constant.String = builder.BuildMethod(MethodAccessFlags.None);
          }
          else
          {
            constant.String = builder.BuildField(FieldAccessFlags.None);
          }

          logger.WriteLine($"NAME-TYPE DESC: \"{constant.String}\"");
          break;
        }
      case ConstantType.MethodHandle:
        {
          constant.String = 
            $"[{constant.ReferenceKind}] {ctx.Constants[constant.ReferenceIndex - 1].String}";
          break;
        }
      case ConstantType.MethodType:
        {
          Debug.Assert(ctx.Constants[constant.DescriptorIndex - 1].type == ConstantType.String);
          constant.String = ctx.Constants[constant.DescriptorIndex - 1].String;
          break;
        }
      case ConstantType.InvokeDynamic:
        {
          constant.String = 
            $"[ATTR={constant.BootstrapMethodAttrIndex}] {ctx.Constants[constant.NameTypeIndex - 1].String}";
          break;
        }

      default:
        throw new ArgumentException(
        $"Can not setup the representation for constant of type {constant.type}"
      );
    }
  }

  internal static Constant Read(JReader reader, JContextView ctx)
  {
    return Read(reader.Reader.BaseStream, ctx);
  }

  internal static Constant Read(Stream reader, JContextView ctx)
  {
    Constant constant = new()
    {
      type = (ConstantType)reader.ReadByte()
    };
    int index = (int)reader.Position;

    int data_buffer_size = 10;
    if (constant.type == ConstantType.String)
    {
      data_buffer_size = 1024;
    }

    Span<byte> data = stackalloc byte[data_buffer_size];
    int read_count = reader.Read(data);

    int constant_read_index = 0;
    LoadContent(constant, data, ref constant_read_index, ctx);

    reader.Seek(index + constant_read_index, SeekOrigin.Begin);

    return constant;
  }

  internal static Constant Parse(ReadOnlySpan<byte> data, ref int index, JContextView ctx = default)
  {
    Constant constant = new()
    {
      type = (ConstantType)data[index++]
    };

    LoadContent(constant, data, ref index, ctx);

    return constant;
  }

  private static void LoadContent(Constant constant, ReadOnlySpan<byte> data,
                                  ref int index, JContextView ctx)
  {
    TextWriter log = ctx.Logger.Logger;
    switch (constant.type)
    {
      case ConstantType.String:
        {
          ushort len = ByteConverter.ToUshort_Big(data, index);
          index += 2;

          ReadOnlySpan<byte> str_data = data[index..(index + len)];
          index += len;

          constant.String = Encoding.UTF8.GetString(str_data);
          log.WriteLine($"READ CONSTANT UTF8: \"{constant.String}\"");
          break;
        }
      case ConstantType.Integer:
        {
          constant.IntegerValue = ByteConverter.ToInt_Big(data, index);
          index += 4;
          log.WriteLine($"READ CONSTANT INT: {constant.IntegerValue}");
          break;
        }
      case ConstantType.Float:
        {
          constant.FloatValue = BitConverter.ToSingle(data.ToArray(), index);
          index += 4;
          log.WriteLine($"READ CONSTANT LONG: {constant.LongValue}");
          break;
        }
      case ConstantType.Long:
        {
          constant.LongValue = ByteConverter.ToLong_Big(data, index);
          index += 8;
          log.WriteLine($"READ CONSTANT FLOAT: {constant.FloatValue}");
          break;
        }
      case ConstantType.Double:
        {
          constant.DoubleValue = BitConverter.ToDouble(data.ToArray(), index);
          index += 8;
          log.WriteLine($"READ CONSTANT DOUBLE: {constant.DoubleValue}");
          break;
        }
      case ConstantType.Class:
        {
          constant.NameIndex = ByteConverter.ToUshort_Big(data, index);
          index += 2;
          log.WriteLine($"READ CONSTANT CLASS: NAME_INDEX={constant.NameIndex}");
          break;
        }
      case ConstantType.FieldReference:
      case ConstantType.MethodReference:
      case ConstantType.InterfaceMethodReference:
        {
          constant.ClassIndex = ByteConverter.ToUshort_Big(data, index);
          index += 2;
          constant.NameTypeIndex = ByteConverter.ToUshort_Big(data, index);
          index += 2;

          log.WriteLine($"READ CONSTANT F/M/IM-REF: CLASS_INDEX={constant.ClassIndex} NAMETYPE={constant.NameTypeIndex}");
          break;
        }
      case ConstantType.StringReference:
        {
          constant.StringIndex = ByteConverter.ToUshort_Big(data, index);
          index += 2;

          log.WriteLine($"READ CONSTANT STRING-REF: STRING_INDEX={constant.StringIndex}");
          break;
        }
      case ConstantType.NameTypeDescriptor:
        {
          constant.NameIndex = ByteConverter.ToUshort_Big(data, index);
          index += 2;
          constant.DescriptorIndex = ByteConverter.ToUshort_Big(data, index);
          index += 2;

          log.WriteLine($"READ CONSTANT NAMETYPE-DESC: NAMEINDEX={constant.NameIndex} DESCINDEX={constant.DescriptorIndex}");
          break;
        }
      case ConstantType.MethodHandle:
        {
          constant.ReferenceKind = (MethodReferenceKind)data[index++];
          constant.ReferenceIndex = ByteConverter.ToUshort_Big(data, index);
          index += 2;

          log.WriteLine($"READ CONSTANT MHANDLE: REFKIND={constant.ReferenceKind} REFINDEX={constant.ReferenceIndex}");
          break;
        }
      case ConstantType.MethodType:
        {
          constant.DescriptorIndex = ByteConverter.ToUshort_Big(data, index);
          index += 2;

          log.WriteLine($"READ CONSTANT MTYPE: DESCINDEX={constant.DescriptorIndex}");
          break;
        }
      case ConstantType.InvokeDynamic:
        {
          constant.BootstrapMethodAttrIndex = ByteConverter.ToUshort_Big(data, index);
          index += 2;
          constant.NameTypeIndex = ByteConverter.ToUshort_Big(data, index);
          index += 2;

          log.WriteLine($"READ CONSTANT INVOKEDYN: BMAI={constant.BootstrapMethodAttrIndex} NAMETYPE={constant.NameTypeIndex}");
          break;
        }
      default:
        throw new ArgumentOutOfRangeException($"constant type [{constant.type}] is not valid, ");
    }
  }

}

public struct ConstantError(ushort index, string msg = "")
{
  public ushort Index = index;
  public string Message = msg;
}
