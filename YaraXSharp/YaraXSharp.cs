﻿using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Text;

namespace YaraXSharp
{
    public class YaraX : IDisposable
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
        private static extern int yrx_rules_count(IntPtr rules);

        private Compiler _compiler = IntPtr.Zero;
        private Rules _rules = IntPtr.Zero;

        public YaraX(params YRX_COMPILE_FLAGS[] flags)
        {
            int allFlags = 0;
            foreach (var flag in flags) allFlags += (int)flag;

            var compiler = yrx_compiler_create(allFlags, out _compiler);
            if (compiler != YRX_RESULT.YRX_SUCCESS) throw new YrxException(compiler.ToString());
        }

        public void Destroy()
        {
            if (_rules != IntPtr.Zero) yrx_rules_destroy(_rules);
            if (_compiler != IntPtr.Zero) yrx_compiler_destroy(_compiler);
        }

        public void AddRuleFile(string filePath)
        {
            if (!File.Exists(filePath)) throw new YrxException("Rule file does not exist.");
            var result = yrx_compiler_add_source(_compiler, File.ReadAllText(filePath));
            // if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException(result.ToString());
        }
        public Tuple<IntPtr, YrxErrorFormat[], YrxErrorFormat[]> Build()
        {
            YrxErrorFormat[] errors = _Errors();
            YrxErrorFormat[] warnings = _Warnings();

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
            if (yrx_buffer.length <= 2) return Array.Empty<YrxErrorFormat>();

            byte[] buffer = new byte[(int)yrx_buffer.length];
            var data = yrx_buffer.data;
            Marshal.Copy(yrx_buffer.data, buffer, 0, (int)yrx_buffer.length);
            try
            {
                // Unpredictable behaviour
                // https://github.com/jtpox/Yara-X-Sharp/commit/afc33cd67d78df1eb94d90a245936f2203dff17c#commitcomment-162014780
                return JsonConvert.DeserializeObject<YrxErrorFormat[]>(Encoding.UTF8.GetString(buffer));
            } catch (JsonException ex)
            {
                return Array.Empty<YrxErrorFormat>();
            }
        }
        
        public void Dispose()
        {
            Destroy();
        }
    }
}
