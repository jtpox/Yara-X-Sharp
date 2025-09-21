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
        LOAD_NAMESPACE,
        LOAD_IDENTIFIER,
    }

    public class SlowestRules
    {
        public string Namespace;
        public string Rule;
        public double MatchTime;
        public double EvalTime;
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
