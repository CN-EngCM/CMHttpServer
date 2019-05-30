using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CMHttpServer {

    /// <summary>
    /// 控制器返回的数据
    /// </summary>
    public class ReturnData {
        /// <summary>
        /// 数据类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 数据内容
        /// </summary>
        public byte[] Content { get; set; }
    }

    public class ServerConfig {
        public IEnumerable<string> Ips { get; set; }
        public Assembly ControllerAssembly { get; set; }
        public string ViewFilesPath { get; set; }
        public string DefaultPage { get; set; }
    }
}
