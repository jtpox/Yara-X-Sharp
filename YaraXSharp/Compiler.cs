using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace YaraXSharp
{
    public class Compiler : IDisposable
    {
        internal IntPtr _compiler = IntPtr.Zero;
        internal Rules? _rules = null;

        public Compiler(params YRX_COMPILE_FLAGS[] flags)
        {
            uint allFlags = 0;
            foreach (var flag in flags) allFlags |= (uint)flag;

            YRX_RESULT compiler = YaraX.yrx_compiler_create(allFlags, out _compiler);
            if (compiler != YRX_RESULT.YRX_SUCCESS) throw new YrxException($"Compiler: ${compiler.ToString()}");
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

        public void AddIncludeDir(string directory)
        {
            YRX_RESULT result = YaraX.yrx_compiler_add_include_dir(_compiler, directory);
            if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException($"AddIncludeDir: {result.ToString()}");
        }

        public void IgnoreModule(string module)
        {
            YRX_RESULT result = YaraX.yrx_compiler_ignore_module(_compiler, module);
            if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException($"IgnoreModule: {result.ToString()}");
        }

        public void BanModule(string module, string errorTitle, string errorMessage)
        {
            YRX_RESULT result = YaraX.yrx_compiler_ban_module(_compiler, module, errorTitle, errorMessage);
            if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException($"BanModule: {result.ToString()}");
        }

        public void NewNamespace(string name)
        {
            YRX_RESULT result = YaraX.yrx_compiler_new_namespace(_compiler, name);
            if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException($"NewNamespace: {result.ToString()}");
        }

        public void DefineGlobal(string identity, string value)
        {
            YRX_RESULT result = YaraX.yrx_compiler_define_global_str(_compiler, identity, value);
            if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException($"DefineGlobal: {result.ToString()}");
        }

        public void DefineGlobal(string identity, bool value)
        {
            YRX_RESULT result = YaraX.yrx_compiler_define_global_bool(_compiler, identity, value);
            if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException($"DefineGlobal: {result.ToString()}");
        }

        public void DefineGlobal(string identity, int value)
        {
            YRX_RESULT result = YaraX.yrx_compiler_define_global_int(_compiler, identity, value);
            if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException($"DefineGlobal: {result.ToString()}");
        }

        public void DefineGlobal(string identity, double value)
        {
            YRX_RESULT result = YaraX.yrx_compiler_define_global_float(_compiler, identity, value);
            if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException($"DefineGlobal: {result.ToString()}");
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
                // return JsonConvert.DeserializeObject<YrxErrorFormat[]>(Encoding.UTF8.GetString(buffer));
                var deserializedJson = JsonSerializer.Deserialize<YrxErrorFormat[]>(Encoding.UTF8.GetString(buffer));
                if (deserializedJson == null)
                {
                    return Array.Empty<YrxErrorFormat>();
                }

                return deserializedJson;
            } catch (Exception ex)
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
