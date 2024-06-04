namespace CertificateRobot.Interface
{
    internal interface ICertificateService
    {
        /// <summary>
        /// 申请证书
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns></returns>
        public Task<string> CertificateApplyAsync(string domain);

        /// <summary>
        /// 证书验证
        /// </summary>
        /// <param name="cert_id">证书ID</param>
        /// <returns></returns>
        public Task<bool> CertificateVerify(string cert_id, int setid);

        /// <summary>
        /// 证书详情
        /// </summary>
        /// <param name="cert_id">证书ID</param>
        /// <returns></returns>
        public Task<(string, string, string, int, bool)> CertificateDetail(string cert_id);

        /// <summary>
        /// 证书下载
        /// </summary>
        /// <param name="cert_id">证书ID</param>
        /// <returns></returns>
        public Task<bool> CertificateDownLoad(string cert_id);

        /// <summary>
        /// 验证证书是否已经申请
        /// </summary>
        /// <param name="domain">域名</param>
        /// <param name="advanceDate">过期时间</param>
        /// <returns></returns>
        public Task<(string, bool)> CheckCertificateHasApplyAsync(string domain, int advanceDate);

        /// <summary>
        /// 获取证书安装密码
        /// </summary>
        /// <param name="certificatePath">证书路径</param>
        /// <returns>证书密码</returns>
        public string GetCertifacateInstallPassword(out string certificatePath);

        /// <summary>
        /// 更换证书文件名
        /// </summary>
        void RenameCertifacateFiles();
    }
}