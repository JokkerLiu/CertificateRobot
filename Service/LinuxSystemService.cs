using CertificateRobot.Dto;
using CertificateRobot.Interface;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace CertificateRobot.Service
{
    internal class LinuxSystemService : ISystemService
    {
        private readonly IConfiguration _configuration;
        private readonly ICertificateService _certificateService;

        public LinuxSystemService(IConfiguration configuration, ICertificateService certificateService)
        {
            _configuration = configuration;
            _certificateService = certificateService;
        }

        public bool DeleteCertificate(string certHashString)
        {
            return true;
        }

        public List<CertificateInformation> GetAllCertificateInformation()
        {
            string certsPath = _configuration["CertificatesPathInLinux"];
            string fileExtension = "*.crt";
            string[] files = Directory.GetFiles(certsPath, fileExtension, SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                List<CertificateInformation> certificates = new List<CertificateInformation>();
                foreach (var file in files)
                {
                    X509Certificate2 cert = new X509Certificate2(file);
                    certificates.Add(new CertificateInformation
                    {
                        CerName = cert.Subject,
                        CertHashString = cert.GetCertHashString(),
                        Subject = cert.Subject.Split('=')[1].Split(',')[0],
                        NotAfter = cert.NotAfter
                    });
                }
                return certificates;
            }
            else
            {
                throw new Exception($"在{certsPath}及其子目录下未发现crt文件，请核对配置文件CertificatesPathInLinux配置项");
            }
        }

        public Task<bool> ReplaceCertInApplication(string old_certHashString, string new_certHashString, byte[] certHash)
        {
            string command = "nginx";
            string arguments = "-s reload";

            return Task.Run(() =>
            {
                try
                {
                    ProcessStartInfo processStartInfo = new ProcessStartInfo(command, arguments)
                    {
                        FileName = command,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process process = new Process { StartInfo = processStartInfo })
                    {
                        process.Start();
                        process.WaitForExit();
                        return true;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Reload Nginx失败：" + e.Message);
                }
            });
        }

        (string, byte[]) ISystemService.InstallCertificate()
        {
            try
            {
                _certificateService.RenameCertifacateFiles();
                return (null, null);
            }
            catch (Exception e)
            {
                throw new Exception("重命名证书文件或者移动文件失败：" + e.Message);
            }
        }
    }
}