using CertificateRobot.Interface;
using Microsoft.Extensions.Configuration;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Dnspod.V20210323;
using TencentCloud.Dnspod.V20210323.Models;

namespace CertificateRobot.Service
{
    internal class TencentDomainService : IDomainService
    {
        private readonly IConfiguration _configuration;

        public TencentDomainService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> AddDomainRecord(string domain, string subDomain, string value, string recordType = "TXT", string recordLine = "默认")
        {
            try
            {
                return await Task.Run(() =>
                {
                    Credential cred = new Credential
                    {
                        SecretId = _configuration["Tencent:SecretId"],
                        SecretKey = _configuration["Tencent:SecretKey"]
                    };

                    ClientProfile clientProfile = new ClientProfile();
                    HttpProfile httpProfile = new HttpProfile();
                    httpProfile.Endpoint = _configuration["Tencent:Url"].Replace("https://", "").Replace("http://", "");
                    clientProfile.HttpProfile = httpProfile;
                    DnspodClient client = new DnspodClient(cred, "", clientProfile);

                    // 验证是否存在记录
                    DescribeRecordListRequest recordReq = new DescribeRecordListRequest()
                    {
                        Domain = string.Join('.', domain.Split('.').Reverse().Take(2).Reverse().ToArray()),
                        Subdomain = subDomain,
                        RecordType = recordType,
                        RecordLine = recordLine
                    };

                    DescribeRecordListResponse exist_resp = null;
                    try
                    {
                        exist_resp = client.DescribeRecordListSync(recordReq);
                    }
                    catch (TencentCloudSDKException e)
                    {
                        if (e.Message.Contains("记录列表为空。"))
                        {
                            // 添加记录
                            CreateRecordRequest req = new CreateRecordRequest()
                            {
                                Domain = string.Join('.', domain.Split('.').Reverse().Take(2).Reverse().ToArray()),
                                SubDomain = subDomain,
                                RecordType = recordType,
                                RecordLine = recordLine,
                                Value = value
                            };

                            CreateRecordResponse resp = client.CreateRecordSync(req);
                            if (resp.RecordId > 0)
                            {
                                return true;
                            }
                            else
                            {
                                throw new Exception("腾讯云解析错误");
                            }
                        }
                        throw new Exception("腾讯云解析错误");
                    }

                    if (exist_resp.RecordList.Where(x => x.Type == recordType && x.Name == subDomain && x.Value == value).Count() == 1)
                    {
                        return true;
                    }

                    throw new Exception("腾讯云解析错误");
                });
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}