using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;

namespace CertificateRobot.Helper
{
    internal class Utils
    {
        /// <summary>
        /// 获取本地IP
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIP()
        {
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            return localIP;
        }

        /// <summary>
        /// 解压缩文件
        /// </summary>
        /// <param name="zipFile"></param>
        /// <param name="outPath"></param>
        internal static void UnZip(string zipFile = "cert.zip", string outPath = "cert")
        {
            try
            {
                using (FileStream fs = new FileStream(zipFile, FileMode.Open))
                {
                    using (ZipArchive zr = new ZipArchive(fs))
                    {
                        foreach (var en in zr.Entries)
                        {
                            FileInfo path = new FileInfo(Path.Combine(outPath, en.FullName));
                            if (!path.Directory.Exists)
                            {
                                path.Directory.Create();
                            }
                            en.ExtractToFile(Path.Combine(outPath, en.FullName), true);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("解压缩错误");
            }
        }

        /// <summary>
        /// 生成pfx证书
        /// </summary>
        /// <param name="certFile"></param>
        /// <param name="keyFile"></param>
        /// <param name="caFile"></param>
        /// <param name="pfxFile"></param>
        /// <param name="pfxPassword"></param>
        internal static void GeneratePfxCertificate(string certFile, string keyFile, string caFile, string pfxFile, string pfxPassword, string domain)
        {
            try
            {
                // 读取证书文件
                X509Certificate certificate = LoadCertificate(certFile);

                // 读取私钥文件
                AsymmetricKeyParameter privateKey = LoadPrivateKey(keyFile);

                // 读取证书链文件
                X509Certificate[] chain = LoadCertificateChain(caFile);

                // 创建PFX（PKCS12）文件
                Pkcs12Store store = new Pkcs12StoreBuilder().Build();
                X509CertificateEntry certEntry = new X509CertificateEntry(certificate);
                store.SetCertificateEntry(domain, certEntry);
                store.SetKeyEntry(domain, new AsymmetricKeyEntry(privateKey), new X509CertificateEntry[] { certEntry }.Concat(chain.Select(c => new X509CertificateEntry(c))).ToArray());

                // 保存PFX文件
                using (FileStream fs = new FileStream(pfxFile, FileMode.Create, FileAccess.Write))
                {
                    store.Save(fs, pfxPassword.ToCharArray(), new SecureRandom());
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"生成pfx证书失败: {ex.Message}");
            }
        }

        private static X509Certificate[] LoadCertificateChain(string caFile)
        {
            List<X509Certificate> chain = new List<X509Certificate>();
            using (StreamReader reader = new StreamReader(caFile))
            {
                PemReader pemReader = new PemReader(reader);
                object obj;
                while ((obj = pemReader.ReadObject()) != null)
                {
                    chain.Add((X509Certificate)obj);
                }
            }
            return chain.ToArray();
        }

        private static AsymmetricKeyParameter LoadPrivateKey(string keyFile)
        {
            using (StreamReader reader = new StreamReader(keyFile))
            {
                PemReader pemReader = new PemReader(reader);
                return (AsymmetricKeyParameter)pemReader.ReadObject();
            }
        }

        private static X509Certificate LoadCertificate(string certFile)
        {
            using (StreamReader reader = new StreamReader(certFile))
            {
                PemReader pemReader = new PemReader(reader);
                return (X509Certificate)pemReader.ReadObject();
            }
        }
    }
}