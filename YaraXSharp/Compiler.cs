using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Text;

namespace YaraXSharp
{
    public class Compiler : IDisposable
    {
        internal IntPtr _compiler = IntPtr.Zero;
        internal Rules? _rules = null;

        public Compiler(params YRX_COMPILE_FLAGS[] flags)
        {
            int allFlags = 0;
            foreach (var flag in flags) allFlags += (int)flag;

            var compiler = YaraX.yrx_compiler_create(allFlags, out _compiler);
            if (compiler != YRX_RESULT.YRX_SUCCESS) throw new YrxException(compiler.ToString());
        }

        public void Destroy()
        {
            if (_rules != null) _rules.Destroy();
            if (_compiler != IntPtr.Zero) YaraX.yrx_compiler_destroy(_compiler);

            _compiler = IntPtr.Zero;
            _rules = null;
        }

        public void AddRuleFile(string filePath)
        {
            if (!File.Exists(filePath)) throw new YrxException("Rule file does not exist.");
            // var result = YaraX.yrx_compiler_add_source(_compiler, File.ReadAllText(filePath));
            YaraX.yrx_compiler_add_source_with_origin(_compiler, File.ReadAllText(filePath), filePath);
            // if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException(result.ToString());
        }
        public Tuple<Rules, YrxErrorFormat[], YrxErrorFormat[]> Build()
        {
            YrxErrorFormat[] errors = _Errors();
            YrxErrorFormat[] warnings = _Warnings();

            IntPtr rules = YaraX.yrx_compiler_build(_compiler);
            _rules = new Rules(rules);
            return Tuple.Create(_rules, errors, warnings);
        }
        private YrxErrorFormat[] _Errors()
        {
            IntPtr yrx_buffer_pointer;
            YaraX.yrx_compiler_errors_json(_compiler, out yrx_buffer_pointer);
            YRX_BUFFER yrx_buffer = Marshal.PtrToStructure<YRX_BUFFER>(yrx_buffer_pointer);
            YrxErrorFormat[] jsonData = _GetJsonFromBuffer(yrx_buffer);

            YaraX.yrx_buffer_destroy(yrx_buffer_pointer);
            return jsonData;
        }

        private YrxErrorFormat[] _Warnings()
        {
            IntPtr yrx_buffer_pointer;
            YaraX.yrx_compiler_warnings_json(_compiler, out yrx_buffer_pointer);
            YRX_BUFFER yrx_buffer = Marshal.PtrToStructure<YRX_BUFFER>(yrx_buffer_pointer);
            YrxErrorFormat[] jsonData = _GetJsonFromBuffer(yrx_buffer);

            YaraX.yrx_buffer_destroy(yrx_buffer_pointer);
            return jsonData;

        }

        private YrxErrorFormat[] _GetJsonFromBuffer(YRX_BUFFER yrx_buffer)
        {
            if (yrx_buffer.length <= 2) return Array.Empty<YrxErrorFormat>();

            byte[] buffer = new byte[(int)yrx_buffer.length];
            var data = yrx_buffer.data;
            Marshal.Copy(yrx_buffer.data, buffer, 0, (int)yrx_buffer.length);
            try
            {
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
