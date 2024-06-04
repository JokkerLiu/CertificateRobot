namespace CertificateRobot.Dto
{
    internal class CertificateInformation
    {
        /// <summary>
        /// 证书名
        /// </summary>
        public string CerName { get; set; } = string.Empty;

        /// <summary>
        /// 证书哈希值字符串
        /// </summary>
        public string CertHashString { get; set; } = string.Empty;

        /// <summary>
        /// 颁发给
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime NotAfter { get; set; }
    }
}