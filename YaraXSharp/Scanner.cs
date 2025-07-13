using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YaraXSharp
{
    public class Scanner
    {
        private delegate void YRX_RULE_CALLBACK(IntPtr rule);

        [DllImport("yara_x_capi.dll")]
        private static extern YRX_RESULT yrx_scanner_create(IntPtr rules, out IntPtr scanner);

        [DllImport("yara_x_capi.dll")]
        private static extern void yrx_scanner_destroy(IntPtr scanner);

        [DllImport("yara_x_capi.dll")]
        private static extern YRX_RESULT yrx_scanner_scan(IntPtr scanner, byte[] data, long len);

        [DllImport("yara_x_capi.dll")]
        private static extern YRX_RESULT yrx_scanner_on_matching_rule(IntPtr scanner, YRX_RULE_CALLBACK callback);

        private IntPtr _scanner;
        private Rules _rules;

        private List<Rule> _rule = new List<Rule>();

        public Scanner(Rules rules)
        {
            _rules = rules;
            yrx_scanner_create(_rules, out _scanner);
            yrx_scanner_on_matching_rule(_scanner, OnMatchCallback);
        }

        public void Scan(string filePath)
        {
            if (!File.Exists(filePath)) throw new YrxException("File does not exist.");
            // TODO: Streaming for large files.
            byte[] file = File.ReadAllBytes(filePath);
            var result = yrx_scanner_scan(_scanner, file, file.Length);
            if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException(result.ToString());
        }

        private void OnMatchCallback(IntPtr rule)
        {
            Rule matchedRule = new Rule(rule);
            matchedRule.GetMetadata();
            _rule.Add(matchedRule);
        }

        public List<Rule> Results()
        {
            return _rule;
        }

        public void Destroy()
        {
            yrx_scanner_destroy(_scanner);
        }
    }
}
