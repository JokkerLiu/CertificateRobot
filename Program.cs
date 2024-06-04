using CertificateRobot.Interface;
using CertificateRobot.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CertificateRobot
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // 获取配置文件
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

            // 注册服务
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddTransient<ISystemService, WindowsSystemService>();
            serviceCollection.AddTransient<ISystemService, LinuxSystemService>();
            serviceCollection.AddTransient<ICertificateService, LetsencryptService>();
            serviceCollection.AddTransient<IDomainService, TencentDomainService>();
            serviceCollection.AddTransient<IDomainService, AlibabaDomainService>();
            serviceCollection.AddTransient<IMessageService, WeChatWorkService>();
            serviceCollection.AddTransient<IMessageService, MailService>();
            serviceCollection.AddTransient<Workflow>();

            // 构建服务提供程序
            using (ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider())
            {
                var workflow = serviceProvider.GetService<Workflow>();
                await workflow.Run();
            }
        }
    }
}