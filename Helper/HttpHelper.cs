using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace CertificateRobot.Helper
{
    internal class HttpHelper
    {
        public static bool Post(string postUrl, string data)
        {
            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(data); //字符串转换为UTF-8编码的字节数组
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
                webReq.Method = "POST";
                webReq.ContentType = "application/x-www-form-urlencoded";

                webReq.ContentLength = byteArray.Length;
                Stream newStream = webReq.GetRequestStream();
                newStream.Write(byteArray, 0, byteArray.Length);//写入参数
                newStream.Close();
                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.Default);
                var res = sr.ReadToEnd();
                sr.Close();
                response.Close();
                newStream.Close();
                var code = JsonConvert.DeserializeObject<dynamic>(res).errcode;
                return code == "0" ? true : throw new Exception("发送消息失败");
            }
            catch (Exception ex)
            {
                throw new Exception("发送消息失败：" + ex.Message);
            }
        }
    }
}