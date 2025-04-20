using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Colussom;

namespace JDism;

class Disassembly : InnerLogger
{
  public ushort VersionMinor;
  public ushort VersionMajor;
  public ClassAccessFlags AccessFlags;
  public ushort ThisClass;
  public ushort SuperClass;
  public ushort[] Interfaces = [];

  public JAttribute[] Attributes = [];

  public JContext Context;

  // constant index X (X for the class file) is Constant[ConstantIndexRoutingTable[X]]
  public ushort[] ConstantIndexRoutingTable = [];

  public Disassembly()
  {
    Context = new JContext();
  }

  public string GenerateSource()
  {
    StringBuilder builder = new(4096);

    string class_name = Context.Constants[ThisClass - 1].String;
    string super_name = Context.Constants[SuperClass - 1].String;

    foreach (JAttribute attr in Attributes)
    {
      builder.Append(attr).Append('\n');
    }

    builder.Append("class ");
    builder.Append(class_name).Append(' ');

    if (super_name != "Object")
    {
      builder.Append("extends ");
      builder.Append(super_name).Append(' ');
    }

    builder.Append("{\n");

    foreach (Field field in Context.Fields ?? [])
    {
      builder.Append(field.ToString(Context)).AppendLine();
    }

    foreach (Method method in Context.Methods ?? [])
    {
      builder.Append(method.ToString(Context)).AppendLine();
    }

    builder.Append('}');

    return builder.ToString();
  }

  public void BuildCIRT()
  {
    if (Context.Constants is null)
    {
      return;
    }

    ConstantIndexRoutingTable = new ushort[Context.Constants.Length + 1];
    int offset = 1;
    for (int i = 0; i < ConstantIndexRoutingTable.Length; i++)
    {
      ConstantIndexRoutingTable[i] = (ushort)(i + offset);
      //if (Constants[i].type == ConstantType.Double || Constants[i].type == ConstantType.Long)
      //{
      //	offset++;
      //}
    }
  }

  public ConstantError[] ValidateConstantTable()
  {
    if (Context.Constants is null)
      return [];
    List<ConstantError> errors = new(2 + Context.Constants.Length / 8);
    ushort len = (ushort)Context.Constants.Length;

    for (ushort i = 0; i < len; i++)
    {
      Constant constant = Context.Constants[i];

      if (constant is null)
      {
        errors.Add(new(i, "Null constant"));
        continue;
      }

      switch (constant.type)
      {
        case ConstantType.Class:
          {
            if (!IsConstantOfType(constant.NameIndex, ConstantType.String))
            {
              errors.Add(new(i, $"Constant Index {constant.NameIndex} should be a valid index to a string constant"));
            }
            break;
          }
        case ConstantType.FieldReference:
        case ConstantType.MethodReference:
        case ConstantType.InterfaceMethodReference:
          {
            if (!IsConstantOfType(constant.ClassIndex, ConstantType.Class))
            {
              errors.Add(new(i, $"Constant Index {constant.ClassIndex} should be a valid index to a class constant"));
            }

            if (!IsConstantOfType(constant.NameTypeIndex, ConstantType.NameTypeDescriptor))
            {
              errors.Add(new(i, $"Constant Index {constant.NameTypeIndex} should be a valid index to a name type descriptor constant"));
            }

            break;
          }
        case ConstantType.StringReference:
          {
            if (!IsConstantOfType(constant.StringIndex, ConstantType.String))
            {
              errors.Add(new(i, $"Constant Index {constant.StringIndex} should be a valid index to a string constant"));
            }

            break;
          }
        case ConstantType.NameTypeDescriptor:
          {
            if (!IsConstantOfType(constant.NameIndex, ConstantType.String))
            {
              errors.Add(new(i, $"Constant Index {constant.NameIndex} should be a valid index to a string constant (name index)"));
            }

            if (!IsConstantOfType(constant.NameIndex, ConstantType.String))
            {
              errors.Add(new(i, $"Constant Index {constant.NameIndex} should be a valid index to a string constant (descriptor index)"));
            }

            break;
          }
        case ConstantType.MethodHandle:
          {
            switch (constant.ReferenceKind)
            {
              case MethodReferenceKind.GetField:
              case MethodReferenceKind.GetStatic:
              case MethodReferenceKind.PutField:
              case MethodReferenceKind.PutStatic:
                {
                  if (!IsConstantOfType(constant.ReferenceIndex, ConstantType.FieldReference))
                  {
                    errors.Add(new(i, $"Constant Index {constant.ReferenceIndex} should be a valid index to a field reference constant"));
                  }
                  break;
                }
              case MethodReferenceKind.InvokeVirtual:
              case MethodReferenceKind.NewInvokeSpecial:
                {
                  if (!IsConstantOfType(constant.ReferenceIndex, ConstantType.MethodReference))
                  {
                    errors.Add(new(i, $"Constant Index {constant.ReferenceIndex} should be a valid index to a method reference constant"));
                  }
                  else if (constant.ReferenceKind != MethodReferenceKind.NewInvokeSpecial)
                  {
                    // TODO: CHECK FOR VALID METHOD NAME
                  }
                  else
                  {
                    // TODO: CHECK FOR VALID NEW METHOD NAME
                  }

                  break;
                }
              case MethodReferenceKind.InvokeStatic:
              case MethodReferenceKind.InvokeSpecial:
                {
                  bool ref_index_valid_55 = IsConstantOfType(constant.ReferenceIndex, ConstantType.MethodReference);
                  if (VersionMajor >= 56)
                  {
                    if (!(ref_index_valid_55 || IsConstantOfType(constant.ReferenceIndex, ConstantType.InterfaceMethodReference)))
                    {
                      errors.Add(new(i, $"Constant Index {constant.ReferenceIndex} should be a valid index to a method reference or an interface method reference constant (v56)"));
                    }
                    else
                    {
                      // TODO: CHECK FOR VALID METHOD NAME
                    }

                    break;
                  }

                  if (!ref_index_valid_55)
                  {
                    errors.Add(new(i, $"Constant Index {constant.ReferenceIndex} should be a valid index to a method reference constant (pre v55)"));
                  }
                  else
                  {
                    // TODO: CHECK FOR VALID METHOD NAME
                  }

                  break;
                }
              case MethodReferenceKind.InvokeInterface:
                {
                  if (!IsConstantOfType(constant.ReferenceIndex, ConstantType.InterfaceMethodReference))
                  {
                    errors.Add(new(i, $"Constant Index {constant.ReferenceIndex} should be a valid index to an interface method reference constant"));
                  }
                  else
                  {
                    // TODO: CHECK FOR VALID METHOD NAME
                  }

                  break;
                }
            }

            break;
          }
        case ConstantType.MethodType:
          {
            if (!IsConstantOfType(constant.DescriptorIndex, ConstantType.String))
            {
              errors.Add(new(i, $"Constant Index {constant.DescriptorIndex} should be a valid index to a string constant (method type)"));
            }

            break;
          }
        case ConstantType.InvokeDynamic:
          {
            // TODO: CHECK FOR THE BOOTSTRAP METHOD BETWEEN THE BOOTSTRAP METHODS OF THIS CLASS

            if (!IsConstantOfType(constant.NameTypeIndex, ConstantType.NameTypeDescriptor))
            {
              errors.Add(new(i, $"Constant Index {constant.NameTypeIndex} should be a valid index to a name type descriptor constant (method type)"));
            }

            break;
          }
        case ConstantType.String:
        case ConstantType.Integer:
        case ConstantType.Long:
        case ConstantType.Float:
        case ConstantType.Double:
          {
            // might check for strings later
            // nothing to do here
            break;
          }
        default:
          {
            errors.Add(new(i, $"Invalid constant type {(int)constant.type}"));
            break;
          }
      }

      if (constant.IsDoubleSlotted())
        i++;

    }

    return errors.ToArray();
  }

  public void PostProcess()
  {
    PostProcessConstants();

    foreach (Field field in Context.Fields ?? [])
    {
      SetupField(field);
    }

    foreach (Method method in Context.Methods ?? [])
    {
      SetupMethod(method);
    }
  }

  public void PostProcessConstants()
  {
    for (uint i = 0; i < Context.Constants.Length; i++)
    {
      Constant constant = Context.Constants[i];
      if (constant.type == ConstantType.Long || constant.type == ConstantType.Double)
        i++;

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
        case ConstantType.Class:
          {
            constant.String = Context.Constants[constant.NameIndex - 1].String.Replace('$', '.');
            Logger.WriteLine($"TYPE: \"{constant.String}\"");
            break;
          }
        case ConstantType.FieldReference:
        case ConstantType.MethodReference:
        case ConstantType.InterfaceMethodReference:
          {
            string class_name = Context.Constants[constant.ClassIndex - 1].String;
            string name_type_name = Context.Constants[constant.NameTypeIndex - 1].String;
            constant.String = $"{class_name}:{name_type_name}";
            Logger.WriteLine($"REF: \"{constant.String}\"");
            break;
          }
        case ConstantType.StringReference:
          {
            constant.String = $"&\"{Context.Constants[constant.NameIndex - 1].String}\"";
            Logger.WriteLine($"CONSTANT STRING: \"{constant.String}\"");
            break;
          }
        case ConstantType.NameTypeDescriptor:
          {
            SourceBuilder builder = new(
              [],
              Context.Constants[constant.NameIndex - 1].String,
              new JType(Context.Constants[constant.DescriptorIndex - 1].String),
              Context
            );

            if (builder.Type?.Kind == JTypeKind.Method)
            {
              constant.String = builder.BuildMethod(MethodAccessFlags.None);
            }
            else
            {
              constant.String = builder.BuildField(FieldAccessFlags.None);
            }

            Logger.WriteLine($"NAME-TYPE DESC: \"{constant.String}\"");
            break;
          }

        default: break;
      }
    }
  }

  private bool IsConstantOfType(ushort index, ConstantType type)
  {
    // the class constant table indices start at 1
    index--;
    if (index >= Context.Constants.Length)
      return false;
    return Context.Constants[index].type == type;
  }

  public void DeserializeConstantsTable(JReader reader)
  {
    int count = reader.ReadU16BE() - 1;
    Constant[] constants = new Constant[count];

    for (uint i = 0; i < count; i++)
    {
      Logger.Write($"READING CONST[{i + 1}] ");
      Constant constant = reader.ReadConstant();
      constants[i] = constant;
      if (constant.type == ConstantType.Double || constant.type == ConstantType.Long)
      {
        i++;
      }
    }

    Context.Constants = constants;
  }

  private void SetupField(Field field)
  {
    // TODO: check for errors/out of range indices
    field.Name = Context.Constants[field.NameIndex - 1].String;
    JStringReader reader = new(Context.Constants[field.DescriptorIndex - 1].String);
    field.InnerType = new JType(reader);
  }

  private void SetupMethod(Method method)
  {
    // TODO: check for errors/out of range indices
    method.Name = Context.Constants[method.NameIndex - 1].String;
    JStringReader reader = new(Context.Constants[method.DescriptorIndex - 1].String);
    method.InnerType = new JType(reader);
  }

  public (int, int) ReadAttributeHeader(byte[] data)
  {
    int name_index = ByteConverter.ToShort_Big(data, 0) - 1;

    Debug.Assert(name_index >= 0 && name_index < Context.Constants.Length);
    Debug.Assert(Context.Constants[name_index].type == ConstantType.String);


    int data_len = ByteConverter.ToInt_Big(data, 2);
    return (name_index, data_len);
  }

  public JAttribute ReadAttribute(JReader reader,
              JTypeParseContext context = JTypeParseContext.Basic)
  {
    (int name_index, int data_len) = ReadAttributeHeader(reader.ReadBuffer(6));
    return LoadAttribute(name_index, reader.ReadBuffer(data_len), context);
  }

  public JAttribute ParseAttribute(byte[] data, ref int index)
  {
    (int name_index, int data_len) = ReadAttributeHeader(data[index..(index + 6)]);
    index += 6;

    Debug.Assert(data.Length - index >= data_len);
    var attr_data = data[index..(index + data_len)];
    index += data_len;
    return LoadAttribute(name_index, attr_data);
  }

  private JAttribute LoadAttribute(int name_index, byte[] data,
              JTypeParseContext context = JTypeParseContext.Basic)
  {
    Debug.Assert(name_index > 0 && name_index < Context.Constants.Length);

    string name = Context.Constants[name_index].String;
    var attr_info = JAttribute.FetchAttributeTypeInfo(name);

    return LoadAttribute(attr_info, data, context);
  }


  public JAttribute LoadAttribute(JAttribute.JAttributeTypeInfo info, byte[] data,
              JTypeParseContext context = JTypeParseContext.Basic)
  {
    if (info is null)
    {
      return null;
    }

    if (info.AttributeType == JAttributeType.ConstantValue)
    {
      int const_index = ByteConverter.ToUshort_Big(data, 0);
      return new ConstantInfoJAttribute(Context.Constants[const_index - 1], (ushort)const_index);
    }

    if (info.AttributeType == JAttributeType.Signature)
    {
      int const_index = ByteConverter.ToUshort_Big(data, 0);
      return new SignatureJAttribute(
        new JType(Context.Constants[const_index - 1].String, context),
        (ushort)const_index
      );
    }

    if (info.AttributeType == JAttributeType.Code)
    {
      return LoadCodeAttribute(data);
    }

    if (info.AttributeType == JAttributeType.StackMapTable)
    {
      ushort frames_count = ByteConverter.ToUshort_Big(data);
      StackMapFrame[] frames = new StackMapFrame[frames_count];

      int index = 2;
      for (int i = 0; i < frames_count; i++)
      {
        frames[i] = StackMapFrame.Parse(data, ref index);
      }

      Debug.Assert(index == data.Length);

      return new StackMapTableJAttribute(frames);
    }

    if (info.AttributeType == JAttributeType.UserDefined)
    {
      return new CustomJAttribute(data);
    }

    if (info.InstanceType == typeof(UnknownJAttribute))
    {
      return new UnknownJAttribute(info.AttributeType, data);
    }

    return null;
  }

  private CodeInfoJAttribute LoadCodeAttribute(byte[] data)
  {
    int index = 0;

    ushort max_stack = ByteConverter.ToUshort_Big(data, index);
    index += 2;
    ushort max_locals = ByteConverter.ToUshort_Big(data, index);
    index += 2;
    int code_length = ByteConverter.ToInt_Big(data, index);
    index += 4;

    byte[] code = data[index..(code_length + index)];
    index += code_length;

    ushort exception_table_length = ByteConverter.ToUshort_Big(data, index);
    index += 2;

    var exceptions = ReadExceptionRecords(data[index..], exception_table_length);
    index += exception_table_length * 8;

    ushort attr_count = ByteConverter.ToUshort_Big(data, index);
    index += 2;

    JAttribute[] attributes = new JAttribute[attr_count];

    for (int i = 0; i < attr_count; i++)
    {
      attributes[i] = ParseAttribute(data, ref index);
    }

    Debug.Assert(index == data.Length);
    return new CodeInfoJAttribute(
      max_stack, max_locals,
      [.. Instruction.ReadAll(code)],
      [.. exceptions],
      attributes
    );
  }

  private IEnumerable<ExceptionRecord> ReadExceptionRecords(byte[] data, int count)
  {
    for (int i = 0; i < count; i++)
    {
      int cur = i * 8;
      yield return new ExceptionRecord(
        ByteConverter.ToUshort_Big(data, cur + 0),
        ByteConverter.ToUshort_Big(data, cur + 2),
        ByteConverter.ToUshort_Big(data, cur + 4),
        ByteConverter.ToUshort_Big(data, cur + 6)
      );
    }
  }
}
