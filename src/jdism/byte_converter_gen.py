

import datetime
from io import StringIO


header = \
f"// THIS FILE IS AUTO GENERATED, DATE={datetime.date.today()}" \
+ """
enum Endianness
{
  Little = 0,
  Big = 1
}

static class ByteConverter {"""

footer = '\n}'

def gen_method(type_name: str, type_size: int, big_endian: bool):
  unsigned = type_name[0].lower() == 'u' or type_name == 'byte'
  is_default_form = unsigned if size == 1 else not unsigned

  builder = StringIO()

  # doc comment
  builder.write("/// <summary>\n")
  builder.write(
    f"/// Converts {type_size} bytes at <paramref name=\"offset\"/> to <see cref=\"{type_name}\"/>, "
    + f"{'in big endian' if big_endian else 'in little endian'}"
  )
  builder.write('\n')
  builder.write("/// </summary>\n")

  # accessibility modifiers
  builder.write(f"public static {type_name} ")
  
  # func name
  builder.write(f"To{type_name.capitalize()}")
  if type_size > 1:
    builder.write(f"{'_Big' if big_endian else '_Little'}")
  
  # args
  builder.write('(byte[] data, int offset = 0)')
  
  builder.write('{\n')

  prefix = '' if size <= 4 and is_default_form else f'({type_name})'
  should_result_cast = (not is_default_form or size == 2) and size > 1
  
  # logic
  builder.write(f'  return ')
  if should_result_cast:
    builder.write(f'({type_name})(')


  parts: list[str] = []
  for i in range(type_size):
    shift = (type_size - i - 1) if big_endian else i
    shift *= 8

    index_add_str = (f' + {i}') if i > 0 else ''
    shift_str = (f' << {shift}') if shift > 0 else ''

    
    if not shift_str:
      parts.append(
        f"{prefix}data[offset{index_add_str}]{shift_str}"
      )
    else:
      parts.append(
        f"({prefix}data[offset{index_add_str}]{shift_str})"
      )

  builder.write(' | '.join(parts))
  if should_result_cast:
    builder.write(')')
  builder.write(';\n}')

  builder.seek(0)
  return builder.read()


INDENT = '  '
def indent_str(text: str):
  return INDENT + text.replace('\n', '\n' + INDENT)

types = 'short', 'ushort', 'int', 'uint', 'long', 'ulong'

print(header)

for i, v in enumerate(types):
  size = 1 << ((i // 2) + 1)
  
  print(indent_str(gen_method(v, size, True)))
  print()
  if size == 1:
    continue
  print(indent_str(gen_method(v, size, False)))
  print()

print(footer)
