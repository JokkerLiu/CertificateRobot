using CertificateRobot.Configuration;
using CertificateRobot.Dto;
using CertificateRobot.Helper;
using CertificateRobot.Interface;
using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;

namespace CertificateRobot.Service
{
    internal class Workflow
    {
        private readonly IConfiguration _configuration;
        private readonly IEnumerable<ISystemService> _systemService;
        private readonly ICertificateService _certificateService;
        private readonly IEnumerable<IDomainService> _domainService;
        private readonly IEnumerable<IMessageService> _messageService;

        public Workflow(IConfiguration configuration, IEnumerable<ISystemService> systemService, ICertificateService certificateService, IEnumerable<IDomainService> domainService, IEnumerable<IMessageService> messageService)
        {
            _configuration = configuration;
            _systemService = systemService;
            _certificateService = certificateService;
            _domainService = domainService;
            _messageService = messageService;
        }

        public async Task Run()
        {
            Console.WriteLine("开始执行工作流程...");

            // 获取当前运行主机操作系统
            string osType = string.Empty;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                osType = "Linux";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                osType = "Windows";
            }
            else
            {
                throw new Exception("不支持的操作系统类型，仅支持Linux、Windows");
            }

            // 获取所有证书信息
            List<CertificateInformation> certificateInformations = new List<CertificateInformation>();
            switch (osType)
            {
                case "Linux":
                    certificateInformations = _systemService.FirstOrDefault(x => x.GetType() == typeof(LinuxSystemService)).GetAllCertificateInformation();
                    break;

                case "Windows":
                    certificateInformations = _systemService.FirstOrDefault(x => x.GetType() == typeof(WindowsSystemService)).GetAllCertificateInformation();
                    break;

                default:
                    throw new Exception("不支持的操作系统类型，仅支持Linux、Windows");
            }

            // 检查证书是否临近过期
            int advanceDate = Convert.ToInt32(_configuration["AdvanceDate"]);

            // 读取配置中需要自动值守的证书
            var domains = new List<Domain>();
            _configuration.GetSection("Domains").Bind(domains);

            foreach (CertificateInformation certificateInformation in certificateInformations)
            {
                // 证书即将过期，且在值守列表中
                if (certificateInformation.NotAfter < DateTime.Now.AddDays(advanceDate) && domains.Select(x => x.Domian).Contains(certificateInformation.Subject))
                {
                    if (_configuration["WeChatWork:Enable"].ToLower() == "true")
                    {
                        // 发送企业微信提醒
                        await _messageService.FirstOrDefault(x => x.GetType() == typeof(WeChatWorkService)).SendMessage($"{certificateInformation.Subject}证书即将过期，即将去往{domains.FirstOrDefault(x => x.Domian == certificateInformation.Subject).DomainProvider}自动申请");
                    }
                    if (_configuration["Smtp:Enable"].ToLower() == "true")
                    {
                        // 发送邮件提醒
                        await _messageService.FirstOrDefault(x => x.GetType() == typeof(MailService)).SendMessage($"{certificateInformation.Subject}证书即将过期，即将去往{domains.FirstOrDefault(x => x.Domian == certificateInformation.Subject).DomainProvider}自动申请");
                    }

                    // 判断证书是否已经申请
                    (string, bool) certId_exist = await _certificateService.CheckCertificateHasApplyAsync(certificateInformation.Subject, advanceDate);

                    string certId = certId_exist.Item1;

                    // 没有申请过证书
                    if (string.IsNullOrEmpty(certId) && certId_exist.Item2 == false)
                    {
                        // 申请新证书
                        certId = await _certificateService.CertificateApplyAsync(certificateInformation.Subject);
                    }

                    // 已经申请过证书但没有DNS验证 || 新申请的证书
                    if (certId_exist.Item2 == false)
                    {
                        // 获取证书详情
                        (string, string, string, int, bool) certDetail = await _certificateService.CertificateDetail(certId);

                        // 验证中的证书跳过DNS解析及证书验证
                        if (!certDetail.Item5)
                        {
                            // 添加云DNS解析
                            switch (domains.FirstOrDefault(x => x.Domian == certificateInformation.Subject).DomainProvider)
                            {
                                case "Tencent":
                                    // 腾讯云解析
                                    await _domainService.FirstOrDefault(x => x.GetType() == typeof(TencentDomainService)).AddDomainRecord(domain: certDetail.Item1, subDomain: certDetail.Item2, value: certDetail.Item3);
                                    break;

                                case "Alibaba":
                                    // 阿里云解析
                                    await _domainService.FirstOrDefault(x => x.GetType() == typeof(AlibabaDomainService)).AddDomainRecord(domain: certDetail.Item1, subDomain: certDetail.Item2, value: certDetail.Item3);
                                    break;

                                default:
                                    throw new Exception($"未知的DNS解析提供商，请检查配置文件，目前支持的提供商有：Tencent、Alibaba");
                            }

                            // 证书验证
                            await _certificateService.CertificateVerify(certId, certDetail.Item4);
                        }
                    }

                    // 证书下载
                    await _certificateService.CertificateDownLoad(certId);

                    // 解压缩证书zip
                    Utils.UnZip(outPath: certificateInformation.Subject.Replace("*", "_"));

                    //// 安装证书
                    //(string, byte[]) installed;
                    //switch (osType)
                    //{
                    //    case "Linux":
                    //        installed = _systemService.FirstOrDefault(x => x.GetType() == typeof(LinuxSystemService)).InstallCertificate();
                    //        break;

                    //    case "Windows":
                    //        installed = _systemService.FirstOrDefault(x => x.GetType() == typeof(WindowsSystemService)).InstallCertificate();
                    //        break;

                    //    default:
                    //        throw new Exception("不支持的操作系统类型，仅支持Linux、Windows");
                    //}

                    //switch (osType)
                    //{
                    //    // 如果是Windows，安装成功后替换IIS证书绑定
                    //    case "Windows":
                    //        await _systemService.FirstOrDefault(x => x.GetType() == typeof(WindowsSystemService)).ReplaceCertInApplication(certificateInformation.CertHashString, installed.Item1, installed.Item2);
                    //        break;

                    //    // 如果是Linux，安装成功后reload Nginx
                    //    case "Linux":
                    //        await _systemService.FirstOrDefault(x => x.GetType() == typeof(LinuxSystemService)).ReplaceCertInApplication(certificateInformation.CertHashString, installed.Item1, installed.Item2);
                    //        break;

                    //    default:
                    //        throw new Exception("不支持的操作系统类型，仅支持Linux、Windows");
                    //}

                    //// 安装成功后删除旧证书
                    //switch (osType)
                    //{
                    //    case "Windows":
                    //        _systemService.FirstOrDefault(x => x.GetType() == typeof(WindowsSystemService)).DeleteCertificate(certificateInformation.CertHashString);
                    //        break;

                    //    case "Linux":
                    //        _systemService.FirstOrDefault(x => x.GetType() == typeof(LinuxSystemService)).DeleteCertificate(certificateInformation.CertHashString);
                    //        break;

                    //    default:
                    //        throw new Exception("不支持的操作系统类型，仅支持Linux、Windows");
                    //}
                }
            }
        }
    }
}