using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using System.IO;
using System.Globalization;
using System.Reflection;


namespace ZWebUtilities
{
    class zResult
    {
        public enum HTTPError
        {
            NameResolutionFailure = 1,

            noError = 200,
            unknownError = 0,
            e400BadRequest = 400,
            e401Unauthorized = 401,
            e403Forbidden = 403,
            e404NotFound = 404,
            e500InternalServerError = 500,
            e502BadGateway = 502,
            e503ServiceUnavailable = 503,
            e504GatewayTimeout = 504
        }

        public Stream resultStream;
        public Stream errorStream;
        public bool isError { get { return error != HTTPError.noError; } }
        public HTTPError error;
        public HttpWebRequest request;
        public WebResponse response;
        public string result;
        public string resultError;

        public zResult()
        {
            resultStream = null;
            errorStream = null;
            request = null;
            response = null;
            error = HTTPError.noError;
            result = "";
            resultError = "";
        }
    }

    static class ZWeb
    {
        public static string host = "";
        public static bool keepalive = true;
        public static WebProxy proxy = null;

        static bool init = false;
        static object locker = new object();
        public delegate void RequestCallback(object item);
        public static RequestCallback requestCallback;

        static void Log(string str, bool toConsole = false)
        {
            lock (locker)
            {
                using (StreamWriter sw = new StreamWriter("ZWeb_Log.txt", true))
                {
                    sw.WriteLine(DateTime.Now.ToString(new CultureInfo("ru-RU")) + ": " + str);
                    if (toConsole) Console.WriteLine(DateTime.Now.ToString(new CultureInfo("ru-RU")) + ": " + str);
                }
            }
        }

        public static zResult Request(string url, string method, string refferer = null, NameValueCollection data = null, CookieContainer cookies = null, bool ajax = false, NameValueCollection credentials = null, string customPOST = "")
        {
            zResult zresult = new zResult();

            if (!init) Init();
            Uri u = null;
            bool correctUri = Uri.TryCreate(HttpUtility.UrlDecode(url).Trim(), UriKind.Absolute, out u);
            if (!correctUri) return null;

            HttpWebRequest request = WebRequest.Create(u) as HttpWebRequest;
            request.Proxy = ZWeb.proxy;

            if (credentials != null) request.Credentials = new NetworkCredential(credentials["Login"], credentials["Password"]);
            request.Method = method;

            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
            request.Accept = "application/json, text/javascript, text/html, application/xml, text/xml, */*";


            //request.AllowAutoRedirect = false;

            //if (ZWeb.host != "")
            request.Host = u.Host;// ZWeb.host;
            request.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:26.0) Gecko/20100101 Firefox/26.0";

            request.KeepAlive = ZWeb.keepalive;
            if (ZWeb.keepalive) request.Headers.Add("Keep-Alive: 300");

            request.Timeout = 20000;
            request.ReadWriteTimeout = 20000;

            if (refferer != null) request.Referer = refferer;

            request.Headers.Add("Cache-Control", "max-age=0");
            request.Headers.Add("Pragma", "no-cache");

            if (ajax)
            {
                request.Headers.Add("X-Prototype-Version", "1.7");
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");

                //request.Headers.Add("X-Client-Data", "CMy1yQEIh7bJAQimtskBCKm2yQEIxLbJAQiehsoBCKGIygEIuYjKAQ==");
                request.Headers.Add("X-YouTube-Page-CL", "65093206");
                request.Headers.Add("X-YouTube-Page-Timestamp", "Wed Apr 16 19:34:30 2014 (1397702070)");
            }

            if (request.Proxy != null && request.Proxy.Credentials != null) request.PreAuthenticate = true;
            // Cookies
            request.CookieContainer = cookies ?? new CookieContainer();
            //System.Net.ServicePointManager.Expect100Continue = false;

            zresult.request = request;

            string dataString = "";

            #region POST
            // Request data
            if (data != null || customPOST != "")
            {
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                if (data != null)
                    dataString = String.Join("&", Array.ConvertAll(data.AllKeys, key =>
                        String.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(data[key]))
                    )
                    );
                dataString += customPOST;

                byte[] dataBytes = Encoding.ASCII.GetBytes(dataString);
                request.ContentLength = dataBytes.Length;
                try
                {
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(dataBytes, 0, dataBytes.Length);
                    requestStream.Dispose();
                }
                catch (Exception ex)
                {
                    Log("stream Request POST:" + ex.Message);
                    if (ex.InnerException != null)
                        Log("inner exception:" + ex.InnerException.Message);
                    zresult.error = zResult.HTTPError.unknownError;
                    return zresult;
                }
            }
            #endregion

            try
            {
                zresult.response = request.GetResponse();
                zresult.resultStream = zresult.response.GetResponseStream();
            }
            catch (WebException ex)
            {
                zresult.error = (zResult.HTTPError)(int)ex.Status;
                Log("stream Request: " + ex.Message);
                if (ex.Response != null)
                {
                    zresult.error = (zResult.HTTPError)(int)((HttpWebResponse)ex.Response).StatusCode;
                    zresult.errorStream = ex.Response.GetResponseStream();
                }
            }

            return zresult;
        }

        public static string RequestString(string url, string method, string refferer = null, NameValueCollection data = null, CookieContainer cookies = null, bool ajax = false, NameValueCollection credentials = null, string customPOST = "")
        {
            var zr = Request(url, method, refferer, data, cookies, ajax, credentials, customPOST);
            GetResult(zr);
            if (zr.isError) return zr.resultError;
            else return zr.result;

        }

        public static Stream RequestStream(string url, string method, string refferer = null, NameValueCollection data = null, CookieContainer cookies = null, bool ajax = false, NameValueCollection credentials = null, string customPOST = "")
        {
            var zr = Request(url, method, refferer, data, cookies, ajax, credentials, customPOST);
            if (zr.isError) return zr.errorStream;
            else return zr.resultStream;
        }

        public static string RequestIgnoreError(string url, int countTry = 0, string method = "GET", string refferer = null, NameValueCollection data = null, CookieContainer cookies = null)
        {
            int iTry = 0;
            var zr = Request(url, method, refferer, data, cookies);
            while (zr.isError && iTry++ < countTry) { zr = Request(url, method, refferer, data, cookies); }
            GetResult(zr);

            return zr.result;
        }

        public static void GetResult(zResult zresult)
        {
            if (zresult.resultStream != null)
            {
                try
                {
                    var encoding = DecodeData(zresult);
                    using (StreamReader sr = new StreamReader(zresult.resultStream, encoding))
                    {
                        zresult.result = sr.ReadToEnd();
                    }
                    if (requestCallback != null) requestCallback(zresult.result);
                    if (zresult.resultStream != null) zresult.resultStream.Close();
                }
                catch (Exception ex)
                {
                    Log("Read result stream exeption: " + ex.Message);
                    Log("Trace: " + ex.StackTrace);
                }
            }

            if (zresult.errorStream != null)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(zresult.errorStream))
                    {
                        zresult.resultError = sr.ReadToEnd();
                    }
                    if (zresult.errorStream != null) zresult.errorStream.Close();
                }
                catch (Exception ex)
                {
                    Log("Read error stream exeption: " + ex.Message);
                    Log("Trace: " + ex.StackTrace);
                }
            }
        }

        public static void GetNewProxy()
        {
            string IPstring = "";
            if (ZWeb.proxy != null) IPstring = ZWeb.proxy.GetProxy(new Uri("http://ya.ru")).Host;
            ZWeb.proxy = null;
            string result = ZWeb.RequestIgnoreError("http://hideme.ru/proxy-list/?maxtime=500&ports=3128&type=h&anon=1", 3, "GET");
            while (ZWeb.proxy == null || IPstring == ZWeb.proxy.GetProxy(new Uri("http://ya.ru")).Host)
            {
                MatchCollection mcol = Regex.Matches(result, "<td class=tdl>([\\d\\.]+?)</td>");
                ZWeb.proxy = new WebProxy(mcol[(new Random()).Next(mcol.Count)].Groups[1].ToString(), 3128);
            }
        }

        #region Service
        private static void Init()
        {
            init = true;
            WorkWithUri();
            ServicePointManager.DefaultConnectionLimit = 200;
        }

        private static void WorkWithUri()
        {

            MethodInfo getSyntax = typeof(UriParser).GetMethod("GetSyntax", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            FieldInfo flagsField = typeof(UriParser).GetField("m_Flags", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (getSyntax != null && flagsField != null)
            {
                foreach (string scheme in new[] { "http", "https" })
                {
                    UriParser parser = (UriParser)getSyntax.Invoke(null, new object[] { scheme });
                    if (parser != null)
                    {
                        int flagsValue = (int)flagsField.GetValue(parser);
                        // Clear the CanonicalizeAsFilePath attribute
                        if ((flagsValue & 0x1000000) != 0)
                            flagsField.SetValue(parser, flagsValue & ~0x1000000);
                    }
                }
            }
        }

        private static Encoding DecodeData(zResult zr)
        {
            String charset = null;
            String ctype = zr.response.Headers["content-type"];
            if (ctype != null)
            {
                int ind = ctype.IndexOf("charset=");
                if (ind != -1)
                {
                    charset = ctype.Substring(ind + 8);
                }
            }

            MemoryStream rawdata = new MemoryStream();
            byte[] buffer = new byte[1024];
            Stream rs = zr.resultStream;

            int read = rs.Read(buffer, 0, buffer.Length);
            while (read > 0)
            {
                rawdata.Write(buffer, 0, read);
                read = rs.Read(buffer, 0, buffer.Length);
            }
            rs.Close();

            if (charset == null)
            {
                MemoryStream ms = rawdata;
                ms.Seek(0, SeekOrigin.Begin);

                StreamReader srr = new StreamReader(ms, Encoding.ASCII);
                String meta = srr.ReadToEnd();

                if (meta != null)
                {
                    int start_ind = meta.IndexOf("charset=");
                    int end_ind = -1;
                    if (start_ind != -1)
                    {
                        end_ind = meta.IndexOf("\"", start_ind);
                        if (end_ind != -1)
                        {
                            int start = start_ind + 8;
                            charset = meta.Substring(start, end_ind - start + 1);
                            charset = charset.TrimEnd(new Char[] { '>', '"' });
                        }
                    }
                }
            }

            Encoding e = null;
            if (charset == null)
            {
                e = Encoding.UTF8;
            }
            else
            {
                try
                {
                    e = Encoding.GetEncoding(charset);
                }
                catch
                {
                    e = Encoding.UTF8;
                }
            }

            rawdata.Seek(0, SeekOrigin.Begin);
            zr.resultStream = rawdata;

            return e;
        }

        /*
        static void WorkWithcertificate()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }
        */


        #endregion
    }
}
