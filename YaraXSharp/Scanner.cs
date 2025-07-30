using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YaraXSharp
{
    public class Scanner : IDisposable
    {
        private IntPtr _scanner;
        private Rules _rules;

        private List<Match> _matchedRules = new List<Match>();
        private YRX_SCANNER_FLAGS[] _load_info;

        public Scanner(Rules rules, params YRX_SCANNER_FLAGS[] load_info)
        {
            _rules = rules;
            _load_info = load_info;

            YaraX.yrx_scanner_create(_rules._pointer, out _scanner);
            YaraX.yrx_scanner_on_matching_rule(_scanner, OnMatchCallback);
        }

        public void Scan(string filePath)
        {
            if (!File.Exists(filePath)) throw new YrxException("File does not exist.");
            byte[] file = File.ReadAllBytes(filePath);
            var result = YaraX.yrx_scanner_scan(_scanner, file, file.Length);
            if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException(result.ToString());
        }

        // Implement your own large file stream!
        public void Scan(byte[] fileBuffer)
        {
            if (fileBuffer.Length == 0) throw new YrxException("File buffer length is zero.");
            var result = YaraX.yrx_scanner_scan(_scanner, fileBuffer, fileBuffer.Length);
            if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException(result.ToString());
        }

        private void OnMatchCallback(IntPtr rule)
        {
            Match matchedRule = new Match(rule, _load_info);
            _matchedRules.Add(matchedRule);
        }

        public List<Match> Results()
        {
            return _matchedRules;
        }

        public void Destroy()
        {
            YaraX.yrx_scanner_destroy(_scanner);
        }

        public void Dispose()
        {
            Destroy();
        }
    }
}
