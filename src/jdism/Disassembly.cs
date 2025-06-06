using System.Diagnostics;
using System.Text;
using JDism.attribute;

namespace JDism;

class Disassembly : InnerLogger
{
  public ClassAccessFlags AccessFlags;
  public ushort ThisClass;
  public ushort SuperClass;
  public ushort[] Interfaces = [];

  public JVMAttribute[] Attributes = [];

  public JContext Context;

  public Disassembly()
  {
    Context = new JContext
    {
      Logger = this
    };
  }

  public string GenerateSource()
  {
    StringBuilder builder = new(4096);

    string class_name = Context.Constants[ThisClass - 1].String;
    string super_name = Context.Constants[SuperClass - 1].String;

    foreach (JVMAttribute attr in Attributes)
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



      if (constant.IsDoubleSlotted())
        i++;

    }

    return [.. errors];
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

      Constant.SetupRepresentation(constant, Context);
    }
  }
  public void DeserializeConstantsTable(JReader reader)
  {
    int count = reader.ReadU16BE() - 1;
    Constant[] constants = new Constant[count];

    for (uint i = 0; i < count; i++)
    {
      Logger.Write($"READING CONST[{i + 1}] ");
      Constant constant = Constant.Read(reader, Context);
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
    Logger.WriteLine($"settings up field {field.Name}");

    JStringReader reader = new(Context.Constants[field.DescriptorIndex - 1].String);
    field.InnerType = new JType(reader);
  }

  private void SetupMethod(Method method)
  {

    // TODO: check for errors/out of range indices
    method.Name = Context.Constants[method.NameIndex - 1].String;
    Logger.WriteLine($"settings up method {method.Name}");

    JStringReader reader = new(Context.Constants[method.DescriptorIndex - 1].String);
    method.InnerType = new JType(reader);
  }

  public (int, int) ReadAttributeHeader(byte[] data)
  {
    int name_index = ByteConverter.ToShort_Big(data, 0) - 1;

    Debug.Assert(name_index >= 0 && name_index < Context.Constants.Length);
    Debug.Assert(Context.Constants[name_index].type == ConstantType.String);


    int data_len = ByteConverter.ToInt_Big(data, 2);


    Logger.WriteLine($"Read attribute header (name_idx={name_index}, data_ln={data_len})");
    return (name_index, data_len);
  }

  public JVMAttribute ReadAttribute(JReader reader,
              JTypeParseContext context = JTypeParseContext.Basic)
  {
    (int name_index, int data_len) = ReadAttributeHeader(reader.ReadBuffer(6));
    return LoadAttribute(name_index, reader.ReadBuffer(data_len), context);
  }

  public JVMAttribute ParseAttribute(ByteSource source)
  {
    (int name_index, int data_len) = ReadAttributeHeader(source.Get(6).ToArray());

    return LoadAttribute(name_index, [.. source.Get(data_len)]);
  }

  private JVMAttribute LoadAttribute(int name_index, byte[] data,
              JTypeParseContext context = JTypeParseContext.Basic)
  {
    Debug.Assert(name_index > 0 && name_index < Context.Constants.Length);

    string name = Context.Constants[name_index].String;
    var attr_info = JVMAttribute.FetchAttributeTypeInfo(name);

    return LoadAttribute(attr_info, new(data, Endianness.Big), context);
  }


  public JVMAttribute LoadAttribute(JVMAttribute.JVMAttributeTypeInfo info, ByteSource source,
              JTypeParseContext context = JTypeParseContext.Basic)
  {
    if (info is null)
    {
      return null;
    }

    if (info.AttributeType == JVMAttributeType.ConstantValue)
    {
      int const_index = source.GetUShort();
      return new ConstantInfoAnnotation(Context.Constants[const_index - 1], (ushort)const_index);
    }

    if (info.AttributeType == JVMAttributeType.Signature)
    {
      int const_index = source.GetUShort();
      return new SignatureAnnotation(
        new JType(Context.Constants[const_index - 1].String, context),
        (ushort)const_index
      );
    }

    if (info.AttributeType == JVMAttributeType.Code)
    {
      return LoadCodeAttribute(source);
    }

    if (info.AttributeType == JVMAttributeType.StackMapTable)
    {
      ushort frames_count = source.GetUShort();
      StackMapFrame[] frames = new StackMapFrame[frames_count];

      for (int i = 0; i < frames_count; i++)
      {
        frames[i] = StackMapFrame.Parse(source);
      }

      Debug.Assert(source.Depleted);

      return new StackMapTableAnnotation(frames);
    }

    if (info.AttributeType == JVMAttributeType.Exceptions)
    {
      ushort exceptions_count = source.GetUShort();
      var exceptions = new ExceptionAnnotation.ExceptionNameInfo[exceptions_count];
      for (int i = 0; i < exceptions_count; i++)
      {
        ushort index = source.GetUShort();
        index -= 1;

        exceptions[i] = new(Context.Constants[index].String, index);
      }
      return new ExceptionAnnotation(exceptions);
    }

    if (info.AttributeType == JVMAttributeType.BootstrapMethods)
    {
      ushort methods_count = source.GetUShort();

      var methods = new BootstrapMethod[methods_count];
      for (int i = 0; i < methods_count; i++)
      {
        ushort ref_index = source.GetUShort();
        ref_index -= 1;

        Debug.Assert(Context.Constants[ref_index].type == ConstantType.MethodHandle);

        ushort args_count = source.GetUShort();

        var args =
          from _ in Enumerable.Repeat(0, args_count)
          let index = source.GetUShort()
          select Context.Constants[index];

        methods[i] = new(
          new BootstrapMethodRef(Context.Constants[ref_index].String, ref_index),
          [.. args]
        );
      }

      return new BootstrapMethodsAnnotation(methods);
    }

    if (info.AttributeType == JVMAttributeType.UserDefined)
    {
      return new CustomAnnotation(source.Content.ToArray());
    }

    if (info.InstanceType == typeof(UnknownAnnotation))
    {
      return new UnknownAnnotation(info.AttributeType, source.Content.ToArray());
    }

    return null;
  }

  private bool IsConstantOfType(ushort index, ConstantType type)
  {
    return IsConstantOfType(index, type, Context);
  }

  private static bool IsConstantOfType(ushort index, ConstantType type, JContextView ctx)
  {
    // the class constant table indices start at 1
    index--;
    if (index >= ctx.Constants.Length)
      return false;
    return ctx.Constants[index].type == type;
  }


  private CodeInfoAnnotation LoadCodeAttribute(ByteSource data)
  {
    ushort max_stack = data.GetUShort();
    ushort max_locals = data.GetUShort();
    int code_length = data.GetInt();

    ByteSource code = new(data.Get(code_length), data.Endianness);

    ushort exception_table_length = data.GetUShort();

    ExceptionRecord[] exceptions = [
      ..ReadExceptionRecords(data, exception_table_length)
    ];

    ushort attr_count = data.GetUShort();
    JVMAttribute[] attributes = new JVMAttribute[attr_count];

    for (int i = 0; i < attr_count; i++)
    {
      attributes[i] = ParseAttribute(data);
    }

    return new CodeInfoAnnotation(
      max_stack, max_locals,
      [.. Instruction.ReadAll(code)],
      exceptions,
      attributes
    );
  }

  private static IEnumerable<ExceptionRecord> ReadExceptionRecords(ByteSource source, int count)
  {
    for (int i = 0; i < count; i++)
    {
      yield return new ExceptionRecord(
        source.GetUShort(),
        source.GetUShort(),
        source.GetUShort(),
        source.GetUShort()
      );
    }
  }

  private static IList<string> CheckConstantErrors(Constant constant, JContextView ctx)
  {
    List<string> results = [];
    switch (constant.type)
    {
      case ConstantType.Class:
        {
          if (!IsConstantOfType(constant.NameIndex, ConstantType.String, ctx))
          {
            results.Add($"Constant Index {constant.NameIndex} should be a valid index to a string constant");
          }
          break;
        }
      case ConstantType.FieldReference:
      case ConstantType.MethodReference:
      case ConstantType.InterfaceMethodReference:
        {
          if (!IsConstantOfType(constant.ClassIndex, ConstantType.Class, ctx))
          {
            results.Add($"Constant Index {constant.ClassIndex} should be a valid index to a class constant");
          }

          if (!IsConstantOfType(constant.NameTypeIndex, ConstantType.NameTypeDescriptor, ctx))
          {
            results.Add($"Constant Index {constant.NameTypeIndex} should be a valid index to a name type descriptor constant");
          }

          break;
        }
      case ConstantType.StringReference:
        {
          if (!IsConstantOfType(constant.StringIndex, ConstantType.String, ctx))
          {
            results.Add($"Constant Index {constant.StringIndex} should be a valid index to a string constant");
          }

          break;
        }
      case ConstantType.NameTypeDescriptor:
        {
          if (!IsConstantOfType(constant.NameIndex, ConstantType.String, ctx))
          {
            results.Add($"Constant Index {constant.NameIndex} should be a valid index to a string constant (name index)");
          }

          if (!IsConstantOfType(constant.NameIndex, ConstantType.String, ctx))
          {
            results.Add($"Constant Index {constant.NameIndex} should be a valid index to a string constant (descriptor index)");
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
                if (!IsConstantOfType(constant.ReferenceIndex, ConstantType.FieldReference, ctx))
                {
                  results.Add($"Constant Index {constant.ReferenceIndex} should be a valid index to a field reference constant");
                }
                break;
              }
            case MethodReferenceKind.InvokeVirtual:
            case MethodReferenceKind.NewInvokeSpecial:
              {
                if (!IsConstantOfType(constant.ReferenceIndex, ConstantType.MethodReference, ctx))
                {
                  results.Add($"Constant Index {constant.ReferenceIndex} should be a valid index to a method reference constant");
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
                bool ref_index_valid_55 = IsConstantOfType(constant.ReferenceIndex, ConstantType.MethodReference, ctx);
                if (ctx.Version.Major >= 56)
                {
                  if (!(ref_index_valid_55 || IsConstantOfType(constant.ReferenceIndex, ConstantType.InterfaceMethodReference, ctx)))
                  {
                    results.Add($"Constant Index {constant.ReferenceIndex} should be a valid index to a method reference or an interface method reference constant (v56)");
                  }
                  else
                  {
                    // TODO: CHECK FOR VALID METHOD NAME
                  }

                  break;
                }

                if (!ref_index_valid_55)
                {
                  results.Add($"Constant Index {constant.ReferenceIndex} should be a valid index to a method reference constant (pre v55)");
                }
                else
                {
                  // TODO: CHECK FOR VALID METHOD NAME
                }

                break;
              }
            case MethodReferenceKind.InvokeInterface:
              {
                if (!IsConstantOfType(constant.ReferenceIndex, ConstantType.InterfaceMethodReference, ctx))
                {
                  results.Add($"Constant Index {constant.ReferenceIndex} should be a valid index to an interface method reference constant");
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
          if (!IsConstantOfType(constant.DescriptorIndex, ConstantType.String, ctx))
          {
            results.Add($"Constant Index {constant.DescriptorIndex} should be a valid index to a string constant (method type)");
          }

          break;
        }
      case ConstantType.InvokeDynamic:
        {
          // TODO: CHECK FOR THE BOOTSTRAP METHOD BETWEEN THE BOOTSTRAP METHODS OF THIS CLASS

          if (!IsConstantOfType(constant.NameTypeIndex, ConstantType.NameTypeDescriptor, ctx))
          {
            results.Add($"Constant Index {constant.NameTypeIndex} should be a valid index to a name type descriptor constant (method type)");
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
          results.Add($"Invalid constant type {(int)constant.type}");
          break;
        }
    }
    return results;
  }
}
