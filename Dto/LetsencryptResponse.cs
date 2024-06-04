namespace CertificateRobot.Dto
{
    /// <summary>
    /// Letsencrypt返回数据
    /// </summary>
    public class LetsencryptResponse<T>
    {
        public int c { get; set; }
        public string m { get; set; }
        public T v { get; set; }
    }

    public class CertificateListResponse
    {
        public int all { get; set; }
        public int mpage { get; set; }
        public int pnum { get; set; }
        public int num { get; set; }
        public CertificateList[] list { get; set; }
    }

    public class CertificateList
    {
        public string id { get; set; }
        public string mark { get; set; }
        public string[] domains { get; set; }
        public string time_add { get; set; }
        public string time_end { get; set; }
        public bool quicker { get; set; }
        public string status { get; set; }
        public string auto_status { get; set; }
    }

    public class CertificateDetailResponse
    {
        public string id { get; set; }
        public string mark { get; set; }
        public string[] domains { get; set; }
        public string time_add { get; set; }
        public string time_end { get; set; }
        public bool quicker { get; set; }
        public string auto_status { get; set; }
        public string status { get; set; }
        public bool can_download { get; set; }
        public bool can_clean { get; set; }
        public bool can_renew { get; set; }
        public bool can_delete { get; set; }
        public bool can_delete_coin { get; set; }
        public bool can_auto { get; set; }
        public string auto_id { get; set; }
        public bool verify_wait { get; set; }
        public Verify_Data[]? verify_data { get; set; }
    }

    public class Verify_Data
    {
        public string domain { get; set; }
        public int id { get; set; }
        public Dictionary<string, ChallengeDetails> check { get; set; }
        public string dns { get; set; }
        public string txt { get; set; }
    }

    public class ChallengeDetails
    {
        public string dns { get; set; }
        public string txt { get; set; }
        public string type { get; set; }
        public string domain { get; set; }
        public string status { get; set; }
        public string identifier { get; set; }
    }
}