namespace CertificateRobot.Configuration
{
    public class AppSettings
    {
        public int AdvanceDate { get; set; }

        public Domain[] Domains { get; set; } = new Domain[0];

        public Letsencrypt Letsencrypt { get; set; } = new Letsencrypt();
        public Tencent Tencent { get; set; } = new Tencent();
        public Alibaba Alibaba { get; set; } = new Alibaba();
    }

    public class Letsencrypt
    {
        public string Token { get; set; } = string.Empty;

        public string Account { get; set; } = string.Empty;
    }

    public class Tencent
    {
        public string SecretId { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class Alibaba
    {
        public string AccessKeyId { get; set; } = string.Empty;
        public string AccessKeySecret { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class Domain
    {
        public string Domian { get; set; } = string.Empty;
        public string DomainProvider { get; set; } = string.Empty;
        public string SSLProvider { get; set; } = string.Empty;
    }

    public class WeChatWork
    {
        public string Url { get; set; } = string.Empty;
        public string AccessTokenUrl { get; set; } = string.Empty;
        public string MessageSendURI { get; set; } = string.Empty;
        public string CorpId { get; set; } = string.Empty;
        public string CorpSecret { get; set; } = string.Empty;
        public string AgentId { get; set; } = string.Empty;
        public string Receiver { get; set; } = string.Empty;
        public string Enable { get; set; } = string.Empty;
    }

    public class Smtp
    {
        public string From { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Port { get; set; } = 587; // default smtp port
        public bool UseSSL { get; set; } = true;
        public string Enable { get; set; } = string.Empty;
    }
}