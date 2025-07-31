using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YaraXSharp
{
    public enum YRX_COMPILE_FLAGS
    {
        YRX_COLORIZE_ERRORS = 1,
        YRX_RELAXED_RE_SYNTAX = 2,
        YRX_ERROR_ON_SLOW_PATTERN = 4,
        YRX_ERROR_ON_SLOW_LOOP = 8,
        YRX_ENABLE_CONDITION_OPTIMIZATION = 16,
        YRX_DISABLE_INCLUDES = 32,
    }

    public enum YRX_RESULT
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

    public struct YRX_BUFFER
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
    public static class YaraX
    {
        [DllImport("yara_x_capi.dll")]
        public static extern void yrx_buffer_destroy(IntPtr buffer);

        /*
         * Compiler Functions
         */
        [DllImport("yara_x_capi.dll")]
        public static extern YRX_RESULT yrx_compiler_create(int flags, out IntPtr compiler);

        [DllImport("yara_x_capi.dll")]
        public static extern void yrx_compiler_destroy(IntPtr compiler);

        [DllImport("yara_x_capi.dll")]
        public static extern IntPtr yrx_compiler_build(IntPtr compiler);

        [DllImport("yara_x_capi.dll")]
        public static extern YRX_RESULT yrx_compiler_add_source(IntPtr compiler, string src);

        [DllImport("yara_x_capi.dll")]
        public static extern YRX_RESULT yrx_compiler_add_source_with_origin(IntPtr compiler, string src, string origin);

        [DllImport("yara_x_capi.dll")]
        public static extern YRX_RESULT yrx_compiler_errors_json(IntPtr compiler, out IntPtr buf);

        [DllImport("yara_x_capi.dll")]
        public static extern YRX_RESULT yrx_compiler_warnings_json(IntPtr compiler, out IntPtr buf);

        /*
         * Rule Functions
         */
        [DllImport("yara_x_capi.dll")]
        public static extern void yrx_rules_destroy(IntPtr rules);

        [DllImport("yara_x_capi.dll")]
        public static extern int yrx_rules_count(IntPtr rules);

        public delegate void YRX_METADATA_CALLBACK(IntPtr metadata);
        public delegate void YRX_TAGS_CALLBACK(IntPtr tag);
        public delegate void YRX_PATTERN_CALLBACK(IntPtr pattern);

        [DllImport("yara_x_capi.dll")]
        public static extern IntPtr yrx_rule_iter_metadata(IntPtr rule, YRX_METADATA_CALLBACK callback);

        [DllImport("yara_x_capi.dll")]
        public static extern IntPtr yrx_rule_iter_tags(IntPtr rule, YRX_TAGS_CALLBACK callback);

        [DllImport("yara_x_capi.dll")]
        public static extern IntPtr yrx_rule_iter_patterns(IntPtr rule, YRX_PATTERN_CALLBACK callback);

        [DllImport("yara_x_capi.dll")]
        public static extern YRX_RESULT yrx_pattern_identifier(IntPtr pattern, out IntPtr identifier, out int length);

        [DllImport("yara_x_capi.dll")]
        public static extern YRX_RESULT yrx_rule_identifier(IntPtr rule, out IntPtr identifier, out int length);

        [DllImport("yara_x_capi.dll")]
        public static extern YRX_RESULT yrx_rule_namespace(IntPtr rule, out IntPtr identifier, out int length);

        [DllImport("yara_x_capi.dll")]
        public static extern YRX_RESULT yrx_rules_serialize(IntPtr rules, out IntPtr buf);

        [DllImport("yara_x_capi.dll")]
        public static extern YRX_RESULT yrx_rules_deserialize(byte[] data, long len, out IntPtr rules);

        /*
         * Scanner Functions
         */
        public delegate void YRX_RULE_CALLBACK(IntPtr rule);

        [DllImport("yara_x_capi.dll")]
        public static extern YRX_RESULT yrx_scanner_set_timeout(IntPtr scanner, long timeout);

        [DllImport("yara_x_capi.dll")]
        public static extern YRX_RESULT yrx_scanner_create(IntPtr rules, out IntPtr scanner);

        [DllImport("yara_x_capi.dll")]
        public static extern void yrx_scanner_destroy(IntPtr scanner);

        [DllImport("yara_x_capi.dll")]
        public static extern YRX_RESULT yrx_scanner_scan(IntPtr scanner, byte[] data, long len);

        [DllImport("yara_x_capi.dll")]
        public static extern YRX_RESULT yrx_scanner_on_matching_rule(IntPtr scanner, YRX_RULE_CALLBACK callback);
    }
}
