using CertificateRobot.Dto;
using CertificateRobot.Helper;
using CertificateRobot.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;

namespace CertificateRobot.Service
{
    internal class WeChatWorkService : IMessageService
    {
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;
        private readonly string _accessTokenUrl;
        private readonly string _messageSendURI;
        private readonly string _corpId;
        private readonly string _corpSecret;
        private readonly string _agentId;

        public WeChatWorkService(IConfiguration configuration)
        {
            _configuration = configuration;
            _baseUrl = _configuration["WeChatWork:Url"];
            _accessTokenUrl = _configuration["WeChatWork:AccessTokenUrl"];
            _messageSendURI = _configuration["WeChatWork:MessageSendURI"];
            _corpId = _configuration["WeChatWork:CorpId"];
            _corpSecret = _configuration["WeChatWork:CorpSecret"];
            _agentId = _configuration["WeChatWork:AgentId"];
        }

        /// <summary>
        /// 发送企业微信消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> SendMessage(string message)
        {
            string accessToken = await GetAccessToken();

            var body = new WechatWorkRequest()
            {
                touser = _configuration["WeChatWork:Receiver"],
                agentid = _agentId,
                msgtype = "text",
                text = new Text()
                {
                    content = message
                }
            };

            string url = $"{_baseUrl}{_messageSendURI}?access_token={accessToken}";

            bool suuccess = HttpHelper.Post(url, JsonConvert.SerializeObject(body));

            return suuccess;
        }

        /// <summary>
        /// 获取AccessToken
        /// </summary>
        private async Task<string> GetAccessToken()
        {
            var client = new RestClient(_baseUrl)
            {
                AcceptedContentTypes = new[] { "application/json" }
            };

            var request = new RestRequest(_accessTokenUrl, Method.Get)
                .AddParameter("corpid", _corpId)
                .AddParameter("corpsecret", _corpSecret);

            var response = await client.GetAsync<WechatWorkResponse>(request);

            return response.access_token;
        }
    }
}