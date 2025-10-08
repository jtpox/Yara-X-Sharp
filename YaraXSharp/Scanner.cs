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

        public void SetTimeout(int seconds)
        {
            YRX_RESULT result = YaraX.yrx_scanner_set_timeout(_scanner, seconds);
            if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException(result.ToString());
        }

        public List<SlowestRules> GetSlowestRules(uint maxResults)
        {
            List<SlowestRules> slowestRules = new List<SlowestRules>();
            YRX_RESULT result = YaraX.yrx_scanner_iter_slowest_rules(_scanner, maxResults, (string nameSpace, string ruleName, double matchTime, double evalTime) =>
            {
                slowestRules.Add(new SlowestRules
                {
                    Namespace = nameSpace,
                    Rule = ruleName,
                    MatchTime = matchTime,
                    EvalTime = evalTime,
                });
            });

            if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException(result.ToString());
            return slowestRules;
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

        public void Scan(string filePath, int blockLength)
        {
            if (!File.Exists(filePath)) throw new YrxException("File does not exist.");
            using (FileStream fileSource = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                long offset = 0;
                do
                {
                    fileSource.Seek(offset, SeekOrigin.Begin);
                    byte[] bytes = new byte[blockLength];
                    int bytesRead = fileSource.Read(bytes, 0, blockLength);
                    offset += bytesRead;
                    int block = (int)(offset / (int)blockLength);
                    YRX_RESULT blockScan = YaraX.yrx_scanner_scan_block(_scanner, (uint)block, bytes, (uint)blockLength);
                    if (blockScan != YRX_RESULT.YRX_SUCCESS) throw new YrxException(blockScan.ToString());
                } while (offset < fileSource.Length);

                YRX_RESULT finalizeBlockScan = YaraX.yrx_scanner_finish(_scanner);
                if (finalizeBlockScan != YRX_RESULT.YRX_SUCCESS) throw new YrxException(finalizeBlockScan.ToString());
            }
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

        public void ClearProfilingData()
        {
            YRX_RESULT result = YaraX.yrx_scanner_clear_profiling_data(_scanner);
            if (result != YRX_RESULT.YRX_SUCCESS) throw new YrxException(result.ToString());
        }

        public void Destroy()
        {
            YaraX.yrx_scanner_destroy(_scanner);
            _scanner = IntPtr.Zero;
        }

        public void Dispose()
        {
            Destroy();
        }
    }
}
