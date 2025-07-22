using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Text;

namespace YaraXSharp
{
    public class YaraX
    {
        [DllImport("yara_x_capi.dll")]
        private static extern YRX_RESULT yrx_compiler_create(int flags, out IntPtr compiler);

        [DllImport("yara_x_capi.dll")]
        private static extern void yrx_compiler_destroy(IntPtr compiler);

        [DllImport("yara_x_capi.dll")]
        private static extern IntPtr yrx_compiler_build(IntPtr compiler);

        [DllImport("yara_x_capi.dll")]
        private static extern YRX_RESULT yrx_compiler_add_source(IntPtr compiler, string src);

        [DllImport("yara_x_capi.dll")]
        private static extern YRX_RESULT yrx_compiler_errors_json(IntPtr compiler, out IntPtr buf);

        [DllImport("yara_x_capi.dll")]
        private static extern YRX_RESULT yrx_compiler_warnings_json(IntPtr compiler, out IntPtr buf);

        [DllImport("yara_x_capi.dll")]
        private static extern void yrx_rules_destroy(IntPtr rules);

        [DllImport("yara_x_capi.dll")]
        private static extern void yrx_buffer_destroy(IntPtr buffer);

        [DllImport("yara_x_capi.dll")]
        public static extern int yrx_rules_count(IntPtr rules);

        private Compiler _compiler;
        private Rules _rules;

        public YaraX(params YRX_COMPILE_FLAGS[] flags)
        {
            int allFlags = 0;
            foreach (var flag in flags) allFlags += (int)flag;

            var compiler = yrx_compiler_create(allFlags, out _compiler);
            if (compiler != YRX_RESULT.YRX_SUCCESS) throw new YrxException(compiler.ToString());
        }

        public void Destroy()
        {
            yrx_rules_destroy(_rules);
            yrx_compiler_destroy(_compiler);
        }

        public void AddRuleFile(string filePath)
        {
            if (!File.Exists(filePath)) throw new YrxException("Rule file does not exist.");
            var result = yrx_compiler_add_source(_compiler, File.ReadAllText(filePath));
            // if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException(result.ToString());
        }

        /* public Rules Build()
        {
            _rules = yrx_compiler_build(_compiler);
            return _rules;
        } */
        public Tuple<IntPtr, YrxErrorFormat[], YrxErrorFormat[]> Build()
        {
            var errors = _Errors();
            var warnings = _Warnings();

            _rules = yrx_compiler_build(_compiler);
            return Tuple.Create(_rules, errors, warnings);
        }

        public int RulesCount()
        {
            return yrx_rules_count(_rules);
        }

        private YrxErrorFormat[] _Errors()
        {
            IntPtr yrx_buffer_pointer;
            yrx_compiler_errors_json(_compiler, out yrx_buffer_pointer);
            YRX_BUFFER yrx_buffer = Marshal.PtrToStructure<YRX_BUFFER>(yrx_buffer_pointer);
            yrx_buffer_destroy(yrx_buffer_pointer);
            return _GetJsonFromBuffer(yrx_buffer);
        }

        private YrxErrorFormat[] _Warnings()
        {
            IntPtr yrx_buffer_pointer;
            yrx_compiler_warnings_json(_compiler, out yrx_buffer_pointer);
            YRX_BUFFER yrx_buffer = Marshal.PtrToStructure<YRX_BUFFER>(yrx_buffer_pointer);
            yrx_buffer_destroy(yrx_buffer_pointer);
            return _GetJsonFromBuffer(yrx_buffer);

        }

        private YrxErrorFormat[] _GetJsonFromBuffer(YRX_BUFFER yrx_buffer)
        {
            if (yrx_buffer.length <= 2)
            {
                return Array.Empty<YrxErrorFormat>();
            }

            byte[] buffer = new byte[(int)yrx_buffer.length];
            var data = yrx_buffer.data;
            Marshal.Copy(yrx_buffer.data, buffer, 0, (int)yrx_buffer.length);

            Console.WriteLine(Marshal.PtrToStringUTF8(yrx_buffer.data, (int)yrx_buffer.length));
            Console.WriteLine(BitConverter.ToString(buffer));
            // Unpredictable behaviour
            try
            {
                return JsonConvert.DeserializeObject<YrxErrorFormat[]>(Encoding.UTF8.GetString(buffer));
            } catch (JsonException ex)
            {
                return new YrxErrorFormat[0];
            }
        }
    }
}
