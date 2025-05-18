using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LKtunnel
{
    public interface IConfigurableProtocol
    {
        void ApplyConfig(ProtocolConfig config);
        ProtocolConfig ExportConfig();
    }
}
