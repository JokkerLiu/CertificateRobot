namespace CertificateRobot.Dto
{
    /// <summary>
    /// 站点信息
    /// </summary>
    internal class SiteInformation
    {
        public string SiteName { get; set; }
        public List<BindingInformations> Bindings { get; set; }
    }

    /// <summary>
    /// 站点绑定信息
    /// </summary>
    public class BindingInformations
    {
        /// <summary>
        /// 协议
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// 绑定信息
        /// </summary>
        public string BindingInformation { get; set; }

        /// <summary>
        /// 证书串
        /// </summary>
        public string CertHashString { get; set; }

        /// <summary>
        /// 证书hash
        /// </summary>
        public byte[] CertificateHash { get; set; }
    }
}