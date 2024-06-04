namespace CertificateRobot.Interface
{
    internal interface IMessageService
    {
        public Task<bool> SendMessage(string message);
    }
}