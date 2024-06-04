using CertificateRobot.Dto;
using CertificateRobot.Interface;
using Microsoft.Web.Administration;
using System.Security.Cryptography.X509Certificates;

namespace CertificateRobot.Service
{
    internal class WindowsSystemService : ISystemService
    {
        private readonly ICertificateService _certificateService;

        public WindowsSystemService(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        /// <summary>
        /// Windows系统获取所有证书信息
        /// </summary>
        /// <returns></returns>
        public List<CertificateInformation> GetAllCertificateInformation()
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            List<CertificateInformation> cerInfos = new List<CertificateInformation>();
            store.Open(OpenFlags.OpenExistingOnly);
            foreach (var cert in store.Certificates)
            {
                cerInfos.Add(new CertificateInformation()
                {
                    CerName = cert.FriendlyName,
                    Subject = cert.Subject.Split('=')[1].Split(',')[0],
                    CertHashString = cert.GetCertHashString(),
                    NotAfter = cert.NotAfter,
                });
            }
            return cerInfos;
        }

        /// <summary>
        /// 安装证书
        /// </summary>
        /// <returns></returns>
        public (string, byte[]) InstallCertificate()
        {
            try
            {
                string certificatePassword = string.Empty, certificatePath = string.Empty;

                // 读取证书密码，重新生成pfx文件
                certificatePassword = _certificateService.GetCertifacateInstallPassword(out certificatePath);

                // 安装证书
                var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadWrite);
                X509Certificate2 cert = new X509Certificate2(certificatePath, certificatePassword, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
                byte[] certHash = cert.GetCertHash();
                string certHashString = cert.GetCertHashString();
                store.Add(cert);
                store.Close();

                return (certHashString, certHash);
            }
            catch (Exception e)
            {
                throw new Exception($"安装证书错误：{e.Message}");
            }
        }

        /// <summary>
        /// 删除证书
        /// </summary>
        /// <param name="certHashString"></param>
        /// <returns></returns>
        public bool DeleteCertificate(string certHashString)
        {
            try
            {
                var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadWrite);

                X509Certificate2 cert = store.Certificates.First(x => x.GetCertHashString() == certHashString);
                store.Remove(cert);
                store.Close();
                return true;
            }
            catch (Exception e)
            {
                throw new Exception($"删除证书失败：{e.Message}");
            }
        }

        /// <summary>
        /// 替换IIS的证书
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<bool> ReplaceCertInApplication(string old_certHashString, string new_certHashString, byte[] certHash)
        {
            return Task.Run(() =>
            {
                try
                {
                    // 获取站点
                    List<SiteInformation> sites = GetSitesInfo();
                    sites = sites.Where(x => x.Bindings.Any(m => m.CertHashString.ToUpper() == old_certHashString.ToUpper())).ToList();
                    BindingCert(sites, new_certHashString, certHash);
                    return true;
                }
                catch (Exception e)
                {
                    throw new Exception($"替换证书失败：{e.Message}");
                }
            });
        }

        /// <summary>
        /// 获取所有站点信息
        /// </summary>
        /// <returns></returns>
        private List<SiteInformation> GetSitesInfo()
        {
            using (ServerManager serverManager = new ServerManager())
            {
                List<SiteInformation> siteInformationList = new List<SiteInformation>();
                foreach (var site in serverManager.Sites)
                {
                    List<BindingInformations> bindingInformationList = new List<BindingInformations>();
                    foreach (var bing in site.Bindings)
                    {
                        BindingInformations binding = new BindingInformations()
                        {
                            Protocol = bing.Protocol,
                            BindingInformation = bing.BindingInformation,
                            CertHashString = bing.Attributes["certificateHash"].Value.ToString(),
                            CertificateHash = bing.CertificateHash,
                        };
                        bindingInformationList.Add(binding);
                    }
                    siteInformationList.Add(new SiteInformation()
                    {
                        SiteName = site.Name,
                        Bindings = bindingInformationList
                    });
                }
                return siteInformationList;
            }
        }

        /// <summary>
        /// 绑定证书
        /// </summary>
        private void BindingCert(List<SiteInformation> sitesInfos, string certHashString, byte[] certHash)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                foreach (SiteInformation sitesInfo in sitesInfos)
                {
                    Site site = serverManager.Sites[sitesInfo.SiteName];
                    foreach (var binding in site.Bindings)
                    {
                        if (binding.Protocol == "https")
                        {
                            binding.CertificateHash = certHash;
                            binding.Attributes["certificateHash"].Value = certHashString;
                        }
                    }
                }
                serverManager.CommitChanges();
            }
        }
    }
}