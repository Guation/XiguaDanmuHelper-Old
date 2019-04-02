using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace XiguaDanmakuHelper
{
    public class Common
    {
        public static string HttpGet(string url)
        {
            HttpWebRequest myRequest = null;
            HttpWebResponse myHttpResponse = null;
            myRequest = (HttpWebRequest) WebRequest.Create(url);
            myRequest.Method = "GET";
            myHttpResponse = (HttpWebResponse) myRequest.GetResponse();
            var reader = new StreamReader(myHttpResponse.GetResponseStream());
            var json = reader.ReadToEnd();
            reader.Close();
            myHttpResponse.Close();
            return json;
        }

        public static string HttpPost(string url, string data)
        {
            var myRequest = (HttpWebRequest) WebRequest.Create(url);
            byte[] ba = Encoding.Default.GetBytes(data);
            myRequest.Method = "POST";
            myRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            myRequest.ContentLength = ba.Length;
            var pStream = myRequest.GetRequestStream();
            pStream.Write(ba, 0, ba.Length);
            pStream.Close();

            var myHttpResponse = (HttpWebResponse) myRequest.GetResponse();
            var reader = new StreamReader(myHttpResponse.GetResponseStream());
            var json = reader.ReadToEnd();
            reader.Close();
            myHttpResponse.Close();
            return json;
        }

        public static async Task<string> HttpGetAsync(string url)
        {
            var request = WebRequest.Create(url);
            request.Method = "GET";
            var response = request.GetResponse();

            using (var stream = response.GetResponseStream())
            using (var sr = new StreamReader(stream))
            {
                var json = await sr.ReadToEndAsync();
                return json;
            }
        }

        public static async Task<string> HttpPostAsync(string url, string data)
        {
            var request = WebRequest.Create(url);
            byte[] ba = Encoding.Default.GetBytes(data);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.ContentLength = ba.Length;
            var pStream = request.GetRequestStream();
            await pStream.WriteAsync(ba, 0, ba.Length);
            pStream.Close();
            var response = request.GetResponse();

            using (var stream = response.GetResponseStream())
            using (var sr = new StreamReader(stream))
            {
                var json = await sr.ReadToEndAsync();
                return json;
            }
        }
    }
}