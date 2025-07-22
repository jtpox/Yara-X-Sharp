global using Rules = System.IntPtr;
global using Compiler = System.IntPtr;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YaraXSharp
{
    public enum YRX_SCANNER_FLAGS
    {
        LOAD_METADATA,
        LOAD_TAGS,
        LOAD_PATTERNS,
    }
    public enum YRX_COMPILE_FLAGS
    {
        YRX_COLORIZE_ERRORS = 1,
        YRX_RELAXED_RE_SYNTAX = 2,
        YRX_ERROR_ON_SLOW_PATTERN = 4,
        YRX_ERROR_ON_SLOW_LOOP = 8,
        YRX_ENABLE_CONDITION_OPTIMIZATION = 16,
        YRX_DISABLE_INCLUDES = 32,
    }
    internal enum YRX_RESULT
    {
        YRX_SUCCESS = 0,
        YRX_SYNTAX_ERROR = 1,
        YRX_VARIABLE_ERROR = 3,
        YRX_SCAN_ERROR = 4,
        YRX_SCAN_TIMEOUT = 5,
        YRX_INVALID_ARGUMENT = 6,
        YRX_INVALID_UTF8 = 7,
        YRX_SERIALIZATION_ERROR = 8,
        YRX_NO_METADATA = 9,
    }

    internal struct YRX_BUFFER
    {
        public IntPtr data;
        public nuint length;
    }


    public struct YRX_METADATA_BYTES
    {
        public int length;
        public IntPtr data;
    }

    public enum YRX_METADATA_VALUE_TYPE
    {
        YRX_I64,
        YRX_F64,
        YRX_BOOLEAN,
        YRX_STRING,
        YRX_BYTES,
    }

    // Magic for union types.
    [StructLayout(LayoutKind.Explicit)]
    public struct YRX_METADATA_VALUE
    {
        [FieldOffset(0)]
        public Int64 i64;
        [FieldOffset(0)]
        public double f64;
        [FieldOffset(0)]
        public bool boolean;
        [FieldOffset(0)]
        public IntPtr String;
        // public char[] String;
        [FieldOffset(0)]
        public YRX_METADATA_BYTES bytes;
    }

    public struct YRX_METADATA
    {
        public string identifier;
        public YRX_METADATA_VALUE_TYPE value_type;
        public YRX_METADATA_VALUE value;
    }

    public class YrxException : Exception
    {
        public YrxException(string message) : base(message) { }
    }

    public class YrxErrorFormat
    {
        public required string type { get; set; }
        public required string code { get; set; }
        public required string title { get; set; }
        public required string text { get; set; }
    }
    internal class Declaratives
    {
    }
}
