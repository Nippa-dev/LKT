using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LKtunnel
{
    public class ProtocolConfig
    {
        public string Protocol { get; set; }

        public string SSHHost { get; set; }
        public string SSHPort { get; set; }
        public string SSHUsername { get; set; }
        public string SSHPassword { get; set; }

        public string WireGuardConfigPath { get; set; }
        public string OpenVPNConfigPath { get; set; }
        public string V2RayConfigPath { get; set; }
        public string ShadowSocksConfigPath { get; set; }
    }
}
