// THIS FILE IS AUTO GENERATED, DATE=2025-04-21
enum Endianness
{
  Little = 0,
  Big = 1
}

static class ByteConverter {
  /// <summary>
  /// Converts 2 bytes at <paramref name="offset"/> to <see cref="short"/>, in big endian
  /// </summary>
  public static short ToShort_Big(byte[] data, int offset = 0){
    return (short)((data[offset] << 8) | data[offset + 1]);
  }
  /// <summary>
  /// Converts 2 bytes at <paramref name="offset"/> to <see cref="short"/>, in big endian
  /// </summary>
  public static short ToShort_Big(ReadOnlySpan<byte> data, int offset = 0){
    return (short)((data[offset] << 8) | data[offset + 1]);
  }

  /// <summary>
  /// Converts 2 bytes at <paramref name="offset"/> to <see cref="short"/>, in little endian
  /// </summary>
  public static short ToShort_Little(byte[] data, int offset = 0){
    return (short)(data[offset] | (data[offset + 1] << 8));
  }
  /// <summary>
  /// Converts 2 bytes at <paramref name="offset"/> to <see cref="short"/>, in little endian
  /// </summary>
  public static short ToShort_Little(ReadOnlySpan<byte> data, int offset = 0){
    return (short)(data[offset] | (data[offset + 1] << 8));
  }

  /// <summary>
  /// Converts 2 bytes at <paramref name="offset"/> to <see cref="ushort"/>, in big endian
  /// </summary>
  public static ushort ToUshort_Big(byte[] data, int offset = 0){
    return (ushort)(((ushort)data[offset] << 8) | (ushort)data[offset + 1]);
  }
  /// <summary>
  /// Converts 2 bytes at <paramref name="offset"/> to <see cref="ushort"/>, in big endian
  /// </summary>
  public static ushort ToUshort_Big(ReadOnlySpan<byte> data, int offset = 0){
    return (ushort)(((ushort)data[offset] << 8) | (ushort)data[offset + 1]);
  }

  /// <summary>
  /// Converts 2 bytes at <paramref name="offset"/> to <see cref="ushort"/>, in little endian
  /// </summary>
  public static ushort ToUshort_Little(byte[] data, int offset = 0){
    return (ushort)((ushort)data[offset] | ((ushort)data[offset + 1] << 8));
  }
  /// <summary>
  /// Converts 2 bytes at <paramref name="offset"/> to <see cref="ushort"/>, in little endian
  /// </summary>
  public static ushort ToUshort_Little(ReadOnlySpan<byte> data, int offset = 0){
    return (ushort)((ushort)data[offset] | ((ushort)data[offset + 1] << 8));
  }

  /// <summary>
  /// Converts 4 bytes at <paramref name="offset"/> to <see cref="int"/>, in big endian
  /// </summary>
  public static int ToInt_Big(byte[] data, int offset = 0){
    return (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];
  }
  /// <summary>
  /// Converts 4 bytes at <paramref name="offset"/> to <see cref="int"/>, in big endian
  /// </summary>
  public static int ToInt_Big(ReadOnlySpan<byte> data, int offset = 0){
    return (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];
  }

  /// <summary>
  /// Converts 4 bytes at <paramref name="offset"/> to <see cref="int"/>, in little endian
  /// </summary>
  public static int ToInt_Little(byte[] data, int offset = 0){
    return data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24);
  }
  /// <summary>
  /// Converts 4 bytes at <paramref name="offset"/> to <see cref="int"/>, in little endian
  /// </summary>
  public static int ToInt_Little(ReadOnlySpan<byte> data, int offset = 0){
    return data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24);
  }

  /// <summary>
  /// Converts 4 bytes at <paramref name="offset"/> to <see cref="uint"/>, in big endian
  /// </summary>
  public static uint ToUint_Big(byte[] data, int offset = 0){
    return (uint)(((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) | ((uint)data[offset + 2] << 8) | (uint)data[offset + 3]);
  }
  /// <summary>
  /// Converts 4 bytes at <paramref name="offset"/> to <see cref="uint"/>, in big endian
  /// </summary>
  public static uint ToUint_Big(ReadOnlySpan<byte> data, int offset = 0){
    return (uint)(((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) | ((uint)data[offset + 2] << 8) | (uint)data[offset + 3]);
  }

  /// <summary>
  /// Converts 4 bytes at <paramref name="offset"/> to <see cref="uint"/>, in little endian
  /// </summary>
  public static uint ToUint_Little(byte[] data, int offset = 0){
    return (uint)((uint)data[offset] | ((uint)data[offset + 1] << 8) | ((uint)data[offset + 2] << 16) | ((uint)data[offset + 3] << 24));
  }
  /// <summary>
  /// Converts 4 bytes at <paramref name="offset"/> to <see cref="uint"/>, in little endian
  /// </summary>
  public static uint ToUint_Little(ReadOnlySpan<byte> data, int offset = 0){
    return (uint)((uint)data[offset] | ((uint)data[offset + 1] << 8) | ((uint)data[offset + 2] << 16) | ((uint)data[offset + 3] << 24));
  }

  /// <summary>
  /// Converts 8 bytes at <paramref name="offset"/> to <see cref="long"/>, in big endian
  /// </summary>
  public static long ToLong_Big(byte[] data, int offset = 0){
    return ((long)data[offset] << 56) | ((long)data[offset + 1] << 48) | ((long)data[offset + 2] << 40) | ((long)data[offset + 3] << 32) | ((long)data[offset + 4] << 24) | ((long)data[offset + 5] << 16) | ((long)data[offset + 6] << 8) | (long)data[offset + 7];
  }
  /// <summary>
  /// Converts 8 bytes at <paramref name="offset"/> to <see cref="long"/>, in big endian
  /// </summary>
  public static long ToLong_Big(ReadOnlySpan<byte> data, int offset = 0){
    return ((long)data[offset] << 56) | ((long)data[offset + 1] << 48) | ((long)data[offset + 2] << 40) | ((long)data[offset + 3] << 32) | ((long)data[offset + 4] << 24) | ((long)data[offset + 5] << 16) | ((long)data[offset + 6] << 8) | (long)data[offset + 7];
  }

  /// <summary>
  /// Converts 8 bytes at <paramref name="offset"/> to <see cref="long"/>, in little endian
  /// </summary>
  public static long ToLong_Little(byte[] data, int offset = 0){
    return (long)data[offset] | ((long)data[offset + 1] << 8) | ((long)data[offset + 2] << 16) | ((long)data[offset + 3] << 24) | ((long)data[offset + 4] << 32) | ((long)data[offset + 5] << 40) | ((long)data[offset + 6] << 48) | ((long)data[offset + 7] << 56);
  }
  /// <summary>
  /// Converts 8 bytes at <paramref name="offset"/> to <see cref="long"/>, in little endian
  /// </summary>
  public static long ToLong_Little(ReadOnlySpan<byte> data, int offset = 0){
    return (long)data[offset] | ((long)data[offset + 1] << 8) | ((long)data[offset + 2] << 16) | ((long)data[offset + 3] << 24) | ((long)data[offset + 4] << 32) | ((long)data[offset + 5] << 40) | ((long)data[offset + 6] << 48) | ((long)data[offset + 7] << 56);
  }

  /// <summary>
  /// Converts 8 bytes at <paramref name="offset"/> to <see cref="ulong"/>, in big endian
  /// </summary>
  public static ulong ToUlong_Big(byte[] data, int offset = 0){
    return (ulong)(((ulong)data[offset] << 56) | ((ulong)data[offset + 1] << 48) | ((ulong)data[offset + 2] << 40) | ((ulong)data[offset + 3] << 32) | ((ulong)data[offset + 4] << 24) | ((ulong)data[offset + 5] << 16) | ((ulong)data[offset + 6] << 8) | (ulong)data[offset + 7]);
  }
  /// <summary>
  /// Converts 8 bytes at <paramref name="offset"/> to <see cref="ulong"/>, in big endian
  /// </summary>
  public static ulong ToUlong_Big(ReadOnlySpan<byte> data, int offset = 0){
    return (ulong)(((ulong)data[offset] << 56) | ((ulong)data[offset + 1] << 48) | ((ulong)data[offset + 2] << 40) | ((ulong)data[offset + 3] << 32) | ((ulong)data[offset + 4] << 24) | ((ulong)data[offset + 5] << 16) | ((ulong)data[offset + 6] << 8) | (ulong)data[offset + 7]);
  }

  /// <summary>
  /// Converts 8 bytes at <paramref name="offset"/> to <see cref="ulong"/>, in little endian
  /// </summary>
  public static ulong ToUlong_Little(byte[] data, int offset = 0){
    return (ulong)((ulong)data[offset] | ((ulong)data[offset + 1] << 8) | ((ulong)data[offset + 2] << 16) | ((ulong)data[offset + 3] << 24) | ((ulong)data[offset + 4] << 32) | ((ulong)data[offset + 5] << 40) | ((ulong)data[offset + 6] << 48) | ((ulong)data[offset + 7] << 56));
  }
  /// <summary>
  /// Converts 8 bytes at <paramref name="offset"/> to <see cref="ulong"/>, in little endian
  /// </summary>
  public static ulong ToUlong_Little(ReadOnlySpan<byte> data, int offset = 0){
    return (ulong)((ulong)data[offset] | ((ulong)data[offset + 1] << 8) | ((ulong)data[offset + 2] << 16) | ((ulong)data[offset + 3] << 24) | ((ulong)data[offset + 4] << 32) | ((ulong)data[offset + 5] << 40) | ((ulong)data[offset + 6] << 48) | ((ulong)data[offset + 7] << 56));
  }


}
