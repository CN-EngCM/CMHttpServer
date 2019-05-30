using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CMHttpServer {
    public static class SystemPara {
        public static string ExePath { get; set; }
        public static Assembly Assembly { get; set; }
        /// <summary>
        /// 页面文件根目录
        /// </summary>
        public static string HtmlFilesPath { get; set; }

    }

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
}
