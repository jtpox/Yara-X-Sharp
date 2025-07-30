using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YaraXSharp
{
    public class Rules : IDisposable
    {
        internal IntPtr _pointer;
        public Rules(IntPtr rulesPtr)
        {
            _pointer = rulesPtr;
        }

        public int Count()
        {
            return YaraX.yrx_rules_count(_pointer);
        }

        public void Destroy()
        {
            if (_pointer != IntPtr.Zero) YaraX.yrx_rules_destroy(_pointer);
            _pointer = IntPtr.Zero;
        }

        public void Dispose()
        {
            Destroy();
        }
    }
}
