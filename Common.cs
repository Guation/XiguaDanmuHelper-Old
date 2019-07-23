using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace XiguaDanmakuHelper
{
    public class Common
    {
        public static string HttpGet(string url,bool ua=false)
        {
            HttpWebRequest myRequest = null;
            HttpWebResponse myHttpResponse = null;
            myRequest = (HttpWebRequest) WebRequest.Create(url);
            myRequest.Method = "GET";
            if (ua) myRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
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
            var request =  WebRequest.Create(url);
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
        public static bool HttpDownload(string url, string path)
        {
            string tempPath = System.IO.Path.GetDirectoryName(path) + @"\temp";
            System.IO.Directory.CreateDirectory(tempPath);  //������ʱ�ļ�Ŀ¼
            string tempFile = tempPath + @"\" + System.IO.Path.GetFileName(path) + ".temp"; //��ʱ�ļ�
            if (System.IO.File.Exists(tempFile))
            {
                System.IO.File.Delete(tempFile);    //���������ɾ��
            }
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);    //�ļ�������ɾ��
            }
            try
            {
                FileStream fs = new FileStream(tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                // ���ò���
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //�������󲢻�ȡ��Ӧ��Ӧ����
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //ֱ��request.GetResponse()����ſ�ʼ��Ŀ����ҳ����Post����
                Stream responseStream = response.GetResponseStream();
                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                while (size > 0)
                {
                    fs.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, (int)bArr.Length);
                }
                fs.Close();
                responseStream.Close();
                System.IO.File.Move(tempFile, path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}