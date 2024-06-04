using CertificateRobot.Dto;
using CertificateRobot.Helper;
using CertificateRobot.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;

namespace CertificateRobot.Service
{
    internal class LetsencryptService : ICertificateService
    {
        private IConfiguration _configuration { get; }
        private string _authentication { get; set; }
        private string _url { get; set; }

        public LetsencryptService(IConfiguration configuration)
        {
            _configuration = configuration;
            _authentication = string.Format("{0}:{1}", configuration["Letsencrypt:Token"], configuration["Letsencrypt:Account"]);
            _url = configuration["Letsencrypt:Url"];
        }

        /// <summary>
        /// 申请证书
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns>证书ID</returns>
        public async Task<string> CertificateApplyAsync(string domain)
        {
            var client = new RestClient(_url)
            {
                AcceptedContentTypes = new[] { "application/json" }
            };

            var request = new RestRequest("/api/order/apply", Method.Get)
                 .AddHeader("Authorization", $"Bearer {_authentication}")
                .AddParameter("domain", domain);

            request.AddHeader("Authorization", $"Bearer {_authentication}");

            var response = await client.GetAsync<LetsencryptResponse<string>>(request);

            return response.v;
        }

        /// <summary>
        /// 获取证书详情
        /// </summary>
        /// <param name="cert_id"></param>
        /// <returns></returns>
        public async Task<(string, string, string, int, bool)> CertificateDetail(string cert_id)
        {
            try
            {
                var client = new RestClient(_url)
                {
                    AcceptedContentTypes = new[] { "application/json" }
                };

                var request = new RestRequest("/api/order/detail", Method.Get)
                     .AddHeader("Authorization", $"Bearer {_authentication}")
                     .AddParameter("id", cert_id);

                int number = 1;

                while (number <= 100)
                {
                    var res = await client.GetAsync(request);

                    var response = JsonConvert.DeserializeObject<LetsencryptResponse<CertificateDetailResponse>>(res.Content);

                    if (response.v.status.ToString() != "初始化")
                    {
                        number += 1;
                        if (response.v.status.ToString() == "需要验证" || response.v.status.ToString() == "待验证")
                        {
                            var tmp = response.v.verify_data[0].check;

                            string dns = tmp["dns-01"].dns.Replace("." + string.Join('.', response.v.verify_data[0].domain.Split('.').Reverse().Take(2).Reverse().ToArray()), "");
                            string txt = tmp["dns-01"].txt.ToString();
                            int id = response.v.verify_data[0].id;
                            string domain = response.v.verify_data[0].domain;

                            return (domain, dns, txt, id, false);
                        }
                        if (response.v.status.ToString() == "验证中")
                        {
                            return (null, null, null, 0, true);
                        }
                    }
                    else
                    {
                        Thread.Sleep(1000 * 5);
                    }
                }
                throw new Exception($"获取证书信息错误");
            }
            catch (Exception e)
            {
                throw new Exception($"获取证书信息错误：{e.Message}");
            }
        }

        /// <summary>
        /// 下载证书
        /// </summary>
        /// <param name="cert_id"></param>
        /// <returns></returns>
        public async Task<bool> CertificateDownLoad(string cert_id)
        {
            try
            {
                // 检查证书是否可下载
                var client_check = new RestClient(_url)
                {
                    AcceptedContentTypes = new[] { "application/json" }
                };

                var request_check = new RestRequest($"api/order/detail", Method.Get)
                    .AddHeader("Authorization", $"Bearer {_authentication}")
                    .AddParameter("id", cert_id);

                while (true)
                {
                    var tmp_res = await client_check.GetAsync(request_check);

                    var response_check = JsonConvert.DeserializeObject<LetsencryptResponse<CertificateDetailResponse>>(tmp_res.Content);

                    if (response_check.v.can_download.ToString().ToLower() == "true")
                    {
                        break;
                    }

                    Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}：证书验证尚未完成，{_configuration["Letsencrypt:Interval"]}分钟后将再次验证，请耐心等待....");
                    Thread.Sleep(1000 * 60 * Convert.ToInt32(_configuration["Letsencrypt:Interval"]));
                }

                // 设置证书默认下载路径
                if (File.Exists("cert.zip"))
                {
                    File.Delete("cert.zip");
                }

                var client = new RestClient(_url)
                {
                    AcceptedContentTypes = new[] { "application/json" }
                };

                var request = new RestRequest($"/api/order/down", Method.Get)
                    .AddHeader("Authorization", $"Bearer {_authentication}")
                    .AddParameter("id", cert_id);

                var buffer = await client.DownloadDataAsync(request);

                using (FileStream fileStream = new FileStream($"cert.zip", FileMode.CreateNew))
                {
                    fileStream.Write(buffer, 0, buffer.Length);
                    return true;
                }
                throw new Exception("下载证书错误");
            }
            catch (Exception e)
            {
                throw new Exception($"下载证书错误{e.Message}");
            }
        }

        /// <summary>
        /// 验证证书
        /// </summary>
        /// <param name="cert_id">证书ID</param>
        /// <returns></returns>
        public async Task<bool> CertificateVerify(string cert_id, int setid)
        {
            try
            {
                var client = new RestClient(_url)
                {
                    AcceptedContentTypes = new[] { "application/json" }
                };

                var request = new RestRequest($"/api/order/verify", Method.Get)
                    .AddHeader("Authorization", $"Bearer {_authentication}")
                    .AddParameter("id", cert_id)
                    .AddParameter("set", setid + ":dns-01");

                while (true)
                {
                    var response = await client.GetAsync<LetsencryptResponse<string>>(request);

                    // 验证成功/已经验证
                    if (response.c == 20 || response.c == 40)
                    {
                        return true;
                    }
                }

                throw new Exception($"验证证书失败");
            }
            catch (Exception e)
            {
                throw new Exception($"验证证书失败：{e.Message}");
            }
        }

        /// <summary>
        /// 判断是否已经申请过证书
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="advanceDate"></param>
        /// <returns></returns>
        public async Task<(string, bool)> CheckCertificateHasApplyAsync(string domain, int advanceDate)
        {
            try
            {
                var client = new RestClient(_url)
                {
                    AcceptedContentTypes = new[] { "application/json" }
                };

                var request = new RestRequest("/api/order/list", Method.Get)
                    .AddHeader("Authorization", $"Bearer {_authentication}");

                var response = await client.GetAsync<LetsencryptResponse<CertificateListResponse>>(request);

                string cert_id = string.Empty;

                // 已申请且未验证DNS
                cert_id = response.v.list.FirstOrDefault(x => x.domains.Contains(domain) && (x.status == "待验证" || x.status == "验证中"))?.id;
                if (!string.IsNullOrEmpty(cert_id))
                {
                    return (cert_id, false);
                }

                // 已申请且已验证DNS,可能存在多个证书，取最近的一个
                cert_id = response.v.list.Where(x => x.domains.Contains(domain) && Convert.ToDateTime(x.time_end) > DateTime.Now.AddDays(advanceDate)).OrderByDescending(m => Convert.ToDateTime(m.time_end)).FirstOrDefault()?.id;
                if (!string.IsNullOrEmpty(cert_id))
                {
                    return (cert_id, true);
                }
                return (cert_id, false);
            }
            catch (Exception e)
            {
                throw new Exception("验证证书是否已经申请异常：" + e.Message);
            }
        }

        /// <summary>
        /// 获取证书安装密码
        /// </summary>
        /// <returns></returns>
        public string GetCertifacateInstallPassword(out string certificatePath)
        {
            certificatePath = "cert/certificate.pfx";

            string certificateKey = "cert/private.pem";
            string certificateCrt = "cert/certificate.crt";
            string caFile = "cert/chain.crt";
            string certificatePfx = "cert/certificate.pfx";

            string contentPath = "cert/detail.txt";
            string certificatePassword = "", domain = "";

            try
            {
                if (File.Exists(contentPath))
                {
                    string strCon = string.Empty;
                    using (StreamReader sr = new StreamReader(contentPath))
                    {
                        strCon = sr.ReadToEnd();
                    }
                    int begin = strCon.IndexOf("pfx导入密码:") + 8;
                    int end = strCon.IndexOf("*", begin) - 1;
                    certificatePassword = strCon.Substring(begin, end - begin).Trim();

                    begin = strCon.IndexOf("域名:") + 3;
                    end = strCon.IndexOf("---", begin) - 1;
                    domain = strCon.Substring(begin, end - begin).Trim();
                }

                // 重新生成pfx证书，因为Let's Encrypt证书已经不支持低版本的IIS
                if (File.Exists(certificatePfx))
                {
                    File.Delete(certificatePfx);
                }
                Utils.GeneratePfxCertificate(certificateCrt, certificateKey, caFile, certificatePfx, certificatePassword, domain);

                return certificatePassword;
            }
            catch (Exception e)
            {
                throw new Exception($"获取证书安装密码错误：{e.Message}");
            }
        }

        /// <summary>
        /// 重命名证书文件
        /// </summary>
        /// <returns></returns>
        public void RenameCertifacateFiles()
        {
            string contentPath = "cert/detail.txt";
            string certificateKey = "cert/private.pem";
            string certificateCrt = "cert/fullchain.crt";
            string domain = "";
            try
            {
                if (File.Exists(contentPath))
                {
                    string strCon = string.Empty;
                    using (StreamReader sr = new StreamReader(contentPath))
                    {
                        strCon = sr.ReadToEnd();
                    }

                    int begin = strCon.IndexOf("域名:") + 3;
                    int end = strCon.IndexOf("---", begin) - 1;
                    domain = strCon.Substring(begin, end - begin).Trim();
                }

                if (!string.IsNullOrEmpty(domain))
                {
                    if (domain.StartsWith("*."))
                    {
                        domain = domain.Replace("*.", "");
                    }

                    // 文档归类
                    string sslPath = _configuration["CertificatesPathInLinux"];
                    string domainPath = Path.Combine(sslPath, domain);
                    if (!Directory.Exists(domainPath))
                    {
                        Directory.CreateDirectory(domainPath);
                    }

                    if (File.Exists(certificateCrt))
                    {
                        if (File.Exists(Path.Combine(domainPath, $"{domain}.crt")))
                        {
                            File.Delete(Path.Combine(domainPath, $"{domain}.crt"));
                        }
                        File.Move(certificateCrt, Path.Combine(domainPath, $"{domain}.crt"));
                    }
                    if (File.Exists(certificateKey))
                    {
                        if (File.Exists(Path.Combine(domainPath, $"{domain}.key")))
                        {
                            File.Delete(Path.Combine(domainPath, $"{domain}.key"));
                        }
                        File.Move(certificateKey, Path.Combine(domainPath, $"{domain}.key"));
                    }
                }
                else
                {
                    throw new Exception("获取证书域名信息错误");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"重命名证书及密钥文件错误：{e.Message}");
            }
        }
    }
}