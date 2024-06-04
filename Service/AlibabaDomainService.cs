using AlibabaCloud.OpenApiClient.Models;
using AlibabaCloud.SDK.Alidns20150109;
using AlibabaCloud.SDK.Alidns20150109.Models;
using AlibabaCloud.TeaUtil.Models;
using CertificateRobot.Interface;
using Microsoft.Extensions.Configuration;
using Tea;

namespace CertificateRobot.Service
{
    internal class AlibabaDomainService : IDomainService
    {
        private readonly IConfiguration _configuration;

        public AlibabaDomainService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<bool> AddDomainRecord(string domain, string subDomain, string value, string recordType = "TXT", string recordLine = "默认")
        {
            try
            {
                return Task.Run(() =>
                {
                    Config config = new Config
                    {
                        AccessKeyId = _configuration["Alibaba:AccessKeyId"],
                        AccessKeySecret = _configuration["Alibaba:AccessKeySecret"],
                        Endpoint = _configuration["Alibaba:Url"].Replace("https://", "").Replace("http://", ""),
                    };

                    var client = new Client(config);

                    RuntimeOptions runtime = new RuntimeOptions();

                    // 检查是否存在记录
                    DescribeDomainRecordsRequest describeDomainRecordsRequest = new DescribeDomainRecordsRequest
                    {
                        DomainName = string.Join('.', domain.Split('.').Reverse().Take(2).Reverse().ToArray()),
                        RRKeyWord = subDomain,
                        TypeKeyWord = recordType,
                        ValueKeyWord = value
                    };

                    try
                    {
                        var record = client.DescribeDomainRecordsWithOptions(describeDomainRecordsRequest, runtime);
                        if (record.Body.TotalCount == 1)
                        {
                            return true;
                        }
                    }
                    catch (TeaException _error)
                    {
                        throw new Exception($"阿里云解析验证解析是否存在错误！{_error.Message}");
                    }
                    catch (Exception _error)
                    {
                        throw new Exception($"阿里云解析验证解析是否存在错误！{_error.Message}");
                    }

                    AddDomainRecordRequest addDomainRecordRequest = new AddDomainRecordRequest
                    {
                        DomainName = string.Join('.', domain.Split('.').Reverse().Take(2).Reverse().ToArray()),
                        RR = subDomain,
                        Type = recordType,
                        Value = value,
                    };

                    var res = client.AddDomainRecordWithOptions(addDomainRecordRequest, runtime);
                    if (res.StatusCode == 200)
                    {
                        return true;
                    }
                    throw new Exception($"阿里云新增解析错误");
                });
            }
            catch (TeaException _error)
            {
                throw new Exception($"阿里云新增解析错误！{_error.Message}");
            }
            catch (Exception _error)
            {
                throw new Exception($"阿里云解析错误！{_error.Message}");
            }
        }
    }
}