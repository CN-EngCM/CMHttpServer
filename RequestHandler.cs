using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Web;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;

namespace CMHttpServer
{
    public class RequestHandler
    {

        private ServerConfig _config;
        private HttpListenerContext _context;
        private byte[] _buffer;
        private HttpListenerRequest _request;
        private HttpListenerResponse _response;
        private Uri _uri;
        private string _queryParas;
        private string _controllerName;
        private string _methodName;
        private string _filePath;
        private string _httpMethod;
         
        public RequestHandler(HttpListenerContext context, ServerConfig serverConfig)
        {
            _config = serverConfig;
            _context = context;
            _request = context.Request;
            _response = context.Response;
            _uri = _request.Url;
            string ControllerPath = _uri.LocalPath;
            if (ControllerPath == "/")
                ControllerPath = _config.DefaultPage;
            _filePath = serverConfig.ViewFilesPath + _uri.LocalPath;
            _queryParas = _uri.Query;
            _httpMethod = _request.HttpMethod;
            string[] param = ControllerPath.Split('/');
            _controllerName = "Controller." + param[1] + "Controller";
            if (param.Length > 2)
                _methodName = param[2];

            StringBuilder mes = new StringBuilder();
            mes.Append("----------Request----------\r\n");
            mes.Append("服务器线程ID: " + Thread.CurrentThread.ManagedThreadId + "\r\n");
            mes.Append("路径: " + _request.Url.AbsolutePath + "\r\n");
            mes.Append("参数: " + _request.Url.Query + "\r\n");
            mes.Append("客户端IP: " + _request.RemoteEndPoint + "\r\n");
            mes.Append(_request.Headers);
            Console.WriteLine(mes.ToString());
        }

        public void Execute()
        {
            switch (_httpMethod)
            {
                case "GET":
                    HandleGet();
                    break;
                case "POST":
                    HandlePost();
                    break;
                default:
                    break;
            }
        }

        public void HandlePost()
        {
            if (!_request.HasEntityBody)
            {
                Console.WriteLine("No client data was sent with the request.");
                return;
            }
            Stream body = _request.InputStream;
            StreamReader reader = new StreamReader(body, Encoding.UTF8);
            if (_request.ContentType != null)
            {
                Console.WriteLine("Client data content type {0}", _request.ContentType);
            }
            Console.WriteLine("Client data content length {0}", _request.ContentLength64);
            Console.WriteLine("****Start of client data:****");
            // Convert the data to a string and display it on the console.
            string s = reader.ReadToEnd();
            Console.WriteLine(s);
            Console.WriteLine("****End of client data:****");
            body.Close();
            reader.Close();
            // If you are finished with the request, it should be closed also.
            //返回消息
            //获取控制器名和方法名
            try
            {
                Type t = _config.ControllerAssembly.GetType(_controllerName);
                MethodInfo mt = t.GetMethod(_methodName);
                object obj = Activator.CreateInstance(t);
                var data = (ReturnData)mt.Invoke(obj, new object[] { _queryParas, s });
                _buffer = data.Content;
                _response.ContentType = MimeMapping.GetMimeMapping(data.Type);
                _response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch
            {
                Return404();
            }
            finally
            {
                OutPutResponse();
            }
        }

        public void HandleGet()
        {
            if (_uri.LocalPath.Contains("."))
                HandleFile();
            else
                HandleMethod();
            OutPutResponse();
        }

        public void HandleFile()
        {
            _response.ContentType = MimeMapping.GetMimeMapping(_filePath);
            if (File.Exists(_filePath))
            {
                //判断文件是否需要缓存
                if (IsNeedCache(_filePath))
                {
                    string MD5 = GetMD5HashFromFile(_filePath);
                    _response.AppendHeader("Cache-Control", "must-revalidate");
                    _response.AppendHeader("ETag", MD5);
                    if (IsMD5Match(_request, MD5))
                    {
                        _buffer = new byte[0];
                        _response.StatusCode = (int)HttpStatusCode.NotModified;
                    }
                    else
                    {
                        _buffer = File.ReadAllBytes(_filePath);
                        _response.StatusCode = (int)HttpStatusCode.OK;
                    }
                }
                else
                {
                    _buffer = File.ReadAllBytes(_filePath);
                    _response.StatusCode = (int)HttpStatusCode.OK;
                }
            }
            else
            {
                Return404();
            }
        }

        public void HandleMethod()
        {
            try
            {
                Type t = _config.ControllerAssembly.GetType(_controllerName);
                MethodInfo mt = t.GetMethod(_methodName);
                object obj = Activator.CreateInstance(t);
                var data = (ReturnData)mt.Invoke(obj, new object[] { _queryParas });
                _buffer = data.Content;
                _response.ContentType = MimeMapping.GetMimeMapping(data.Type);
                _response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch
            {
                Return404();
            }
        }

        private void Return404()
        {
            _buffer = new byte[0];
            _response.StatusCode = (int)HttpStatusCode.NotFound;
        }

        private bool IsNeedCache(string path)
        {
            return path.EndsWith(".html") | path.EndsWith(".js") | path.EndsWith(".css") | path.EndsWith(".png") | path.EndsWith(".jpg") | path.EndsWith(".woff2");
        }

        private bool IsMD5Match(HttpListenerRequest request, string md5)
        {
            bool isMatch = false;
            string requestMD5 = request.Headers["If-None-Match"];
            if (requestMD5 == md5)
                isMatch = true;
            return isMatch;
        }

        private void OutPutResponse()
        {
            _response.KeepAlive = true;
            _response.ContentLength64 = _buffer.Length;
            try
            {
                using (Stream output = _context.Response.OutputStream)
                {
                    output.Write(_buffer, 0, _buffer.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _response.Close();
            }
        }

        private string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, System.IO.FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }


    }

}
