namespace CertificateRobot.Dto
{
    internal class WechatWorkRequest
    {
        public string touser { get; set; } = "@all";

        public string msgtype { get; set; } = "text";
        public string agentid { get; set; } = string.Empty;

        public Text text { get; set; } = null!;
    }

    public class Text
    {
        public string content { get; set; } = "";
    }
}