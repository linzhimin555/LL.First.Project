using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace LL.FirstCore.HttpHelper
{
    /// <summary>
    /// httpclient扩展类
    /// </summary>
    public class CustomClient
    {
        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url">请求地址</param>
        /// <param name="dic">请求参数</param>
        /// <param name="bearerToken"></param>
        /// <returns></returns>
        public async static Task<string> GetData(HttpClient client, string url, Dictionary<string, string> dic, string bearerToken = "")
        {
            var result = string.Empty;
            var parameters = string.Join("&", dic.Select(v => $"{v.Key}={v.Value}"));
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"{url}?{parameters}"))
            {
                request.Headers.Add("Accept", "application/vnd.github.v3+json");
                request.Headers.Add("User-Agent", "HttpClientFactory-Sample");

                if (!string.IsNullOrWhiteSpace(bearerToken))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                }
                using (var response = await client.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                    }
                }
            }

            return result;

        }

        /// <summary>
        /// Post请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <param name="dic"></param>
        /// <param name="bearerToken"></param>
        /// <returns></returns>
        public async static Task<ResponseMessage<T>> PostData<T>(HttpClient client, string url, Dictionary<string, string> dic, string bearerToken = "")
        {
            var result = new ResponseMessage<T>();
            try
            {
                using (HttpContent httpContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(dic), Encoding.UTF8))
                {
                    if (!string.IsNullOrWhiteSpace(bearerToken))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                    }

                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    using (var response = await client.PostAsync(url, httpContent))
                    {
                        result.HttpStatus = response.StatusCode;
                        if (response != null && response.IsSuccessStatusCode)
                        {
                            Type type = typeof(T);
                            if (type == typeof(string))
                            {
                                var responseStr = await response.Content.ReadAsStringAsync();
                                result.Data = (T)Convert.ChangeType(responseStr, type);
                            }
                            else
                            {
                                var responseStr = response.Content.ReadAsStringAsync().Result;
                                result.Data = System.Text.Json.JsonSerializer.Deserialize<T>(responseStr);
                            }
                        }
                        else
                        {
                            result.IsSuccess = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Msg = ex.InnerException.ToString();
            }

            return result;
        }

        /// <summary>
        /// Form-Data表单数据提交
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="clientFactory"></param>
        /// <returns></returns>
        public async static Task<ResponseMessage<T>> PostFormData<T>(IHttpClientFactory clientFactory)
        {
            var result = new ResponseMessage<T>();
            Stream imageStream;
            var request = new HttpRequestMessage(HttpMethod.Get, "http://dsj.shui00.com/shuiliju/service/file.jsp?fileid=116104");
            using (var client = clientFactory.CreateClient())
            {
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    imageStream = await response.Content.ReadAsStreamAsync();
                    imageStream.Position = 0;
                    var postContent = new MultipartFormDataContent();
                    string boundary = string.Format("--{0}", DateTime.Now.Ticks.ToString("x"));
                    postContent.Headers.Add("ContentType", $"multipart/form-data, boundary={boundary}");
                    var requestUri = "https://web.dcyun.com:48119/zjsdz-api/api/file/upload/syn";
                    //一定要带上文件名称，不然就是500
                    postContent.Add(new StreamContent(imageStream, (int)imageStream.Length), "file", "test.png");
                    postContent.Add(new StringContent("taiZhouShi"), string.Format("\"{0}\"", "userCode"));
                    postContent.Add(new StringContent("2e049b9c258d7f3edb53cf4d997bf93d"), string.Format("\"{0}\"", "passWord"));
                    using (var fileClient = clientFactory.CreateClient())
                    {
                        var uploadResponse = await fileClient.PostAsync(requestUri, postContent);
                        result.HttpStatus = uploadResponse.StatusCode;
                        if (uploadResponse.IsSuccessStatusCode)
                        {
                            var responseStr = await uploadResponse.Content.ReadAsStringAsync();
                            result.Data = System.Text.Json.JsonSerializer.Deserialize<T>(responseStr);
                        }
                    }
                }
            }

            return result;
        }

        public class ResponseMessage<T>
        {
            /// <summary>
            /// 请求是否成功
            /// </summary>
            public bool IsSuccess { get; set; } = true;
            /// <summary>
            /// http请求状态码
            /// </summary>
            public HttpStatusCode HttpStatus { get; set; }
            /// <summary>
            /// 请求结果信息
            /// </summary>
            public string Msg { get; set; }
            /// <summary>
            /// 请求结果数据
            /// </summary>
            public T Data { get; set; }
        }
    }
}
