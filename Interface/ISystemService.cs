using CertificateRobot.Dto;

namespace CertificateRobot.Interface
{
    internal interface ISystemService
    {
        /// <summary>
        /// 获取所有证书信息
        /// </summary>
        /// <returns></returns>
        public List<CertificateInformation> GetAllCertificateInformation();

        /// <summary>
        /// 安装证书
        /// </summary>
        /// <returns></returns>
        public (string, byte[]) InstallCertificate();

        /// <summary>
        /// 删除过期证书
        /// </summary>
        /// <returns></returns>
        public bool DeleteCertificate(string certHashString);

        /// <summary>
        /// 替换应用程序中的证书
        /// </summary>
        /// <returns></returns>
        public Task<bool> ReplaceCertInApplication(string old_certHashString, string new_certHashString, byte[] certHash);
    }
}