using System.Runtime.InteropServices;

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
        private static extern YRX_RESULT yrx_compiler_errors_json(IntPtr compiler, out YRX_BUFFER buf); // TODO

        [DllImport("yara_x_capi.dll")]
        private static extern YRX_RESULT yrx_compiler_warnings_json(IntPtr compiler, out YRX_BUFFER buf); // TODO

        [DllImport("yara_x_capi.dll")]
        private static extern void yrx_rules_destroy(IntPtr rules);

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
            if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException(result.ToString());
        }

        public Rules Build()
        {
            _rules = yrx_compiler_build(_compiler);
            return _rules;
        }

        public int RulesCount()
        {
            return yrx_rules_count(_rules);
        }
    }
}
