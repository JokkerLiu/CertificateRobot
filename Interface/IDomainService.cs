namespace CertificateRobot.Interface
{
    internal interface IDomainService
    {
        /// <summary>
        /// 添加解析
        /// </summary>
        /// <param name="domain">主域名</param>
        /// <param name="subDomain">子域名</param>
        /// <param name="recordType">类型</param>
        /// <param name="value">值</param>
        /// <param name="recordLine">线路</param>
        /// <returns></returns>
        public Task<bool> AddDomainRecord(string domain, string subDomain, string value, string recordType = "TXT", string recordLine = "默认");
    }
}