using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Configuration;
using System.Reflection;

namespace CMHttpServer {
    public class HttpServer {

        private HttpListener listener = new HttpListener();

        /// <summary>
        /// 启动http服务器
        /// </summary>
        /// <param name="ips">需要监听的ip和端口集合</param>
        /// <param name="exePath">引用此项目程序集的绝对位置</param>
        public HttpServer(IEnumerable<string> ips,string exePath,string htmlFilesPath) {
            SystemPara.ExePath = exePath;
            SystemPara.Assembly = Assembly.LoadFrom(SystemPara.ExePath);
            SystemPara.HtmlFilesPath = htmlFilesPath;
            
            foreach (var ip in ips) 
            {
                listener.Prefixes.Add(ip);
            }
            listener.Start();
            listener.BeginGetContext(AcceptedContext, null);
        }


        private void AcceptedContext(IAsyncResult ar) {
            //接收到一个客户端的连接
            RequestHandler handler = new RequestHandler(listener.EndGetContext(ar));
            //调用自身继续监听
            listener.BeginGetContext(AcceptedContext, null);
            //触发事件
            Accepted?.Invoke(this, new AcceptedEventArgs(handler));
        }


        #region 事件Accepted
        public delegate void AcceptedEventHandler(Object sender, AcceptedEventArgs e);
        public event AcceptedEventHandler Accepted; //声明事件
        public class AcceptedEventArgs : EventArgs {
            public readonly RequestHandler Handler;
            public AcceptedEventArgs(RequestHandler handler) {
                Handler = handler;
            }
        }
        #endregion
    }
}
