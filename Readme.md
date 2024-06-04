# 证书管理小工具

## 写在开头：重要！！

**本工具为适应我们公司实际运维环境，一些逻辑不具有通用性，请自行修改或者测试，切勿直接上生产环境，以免带来问题**

代码能力有限，可能存在未知的Bug

## 功能介绍

工具采用`.NET6`开发，纯控制台程序，无页面操作

**仅针对`IIS`和`Nginx`开发，仅满足`腾讯云`和`阿里云`托管的域名，仅支持`Letsencrypt`托管的证书**

支持以下功能：

1. 证书过期预警

   预警逻辑：配置文件`Domains`项下的所有`Domain`的`NotAfter`信息与`AdvanceDate`比较，小于预警时间的，发送`企业微信`或者`邮件`进行提醒

   > 企业微信需要申请一个应用以发送消息，[官方文档](https://developer.work.weixin.qq.com/document/path/90235)

2. 过期证书自动申请

   目前只支持`Letsencrypt`申请证书，且域名只支持`腾讯云`和`阿里云`托管的域名，需要去对应的后台获取`Token`和`Key`

   + `Letsencrypt`:[访问官网](https://letsencrypt.osfipin.com/jump/share?code=LD1PPLRZ)，在`我的/API接口`中

3. 证书自动验证

   会根据配置`Domains`下的`DomainProvider`去对应的域名托管商添加`DNS`验证，目前只支持`腾讯云`和`阿里云`

+ `腾讯云`：[申请地址](https://console.cloud.tencent.com/cam/capi)

+ `阿里云`：[申请地址](https://ram.console.aliyun.com/manage/ak)

  > 注意：以上信息十分重要，请妥善保管，`Letsencrypt`验证时间约为30分钟

4. 证书自动下载、解压缩、重生成

   对于`IIS`需要的`pfx`文件，生成的文件已不支持低版本，程序根据证书和密钥会重新生成一个低版本支持的`pfx`文件

   对于`Linux`系统会在配置文件`CertificatesPathInLinux`下生成一个域名同名的文件夹，并将申请的证书和密钥重新命名，并保存在文件夹下

5. 证书自动替换

   对于`IIS`应用，程序会自动替换`IIS`绑定的证书

   对于`CentOS`,程序仅简单执行`nginx -s reload`操作，不加绝对路径，故需要配置`nginx`环境变量

   > 仅在`CentOS`下简单测试，理论其它`Linux`环境也可以

6. 过期证书自动删除

## 使用方式

`Windows`环境下推荐使用`任务管理器`定时执行

`CentOS`环境下推荐使用`Crond`定时执行