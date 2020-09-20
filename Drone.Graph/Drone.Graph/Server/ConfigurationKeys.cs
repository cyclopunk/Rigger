using Rigger.Attributes;

namespace Drone.Graph.Server
{
    public class ConfigurationKeys
    {
        [Document(Description = "Set this to true if you want the webserver to start.")]
        public static readonly string StartWebServer = "WebServer.Start";

        [Document(Description = "The port that the webserver runs on")]
        public static readonly string Port = "WebServer.Port";

        [Document(Description = "Path statement to the webserver certificate file")]
        public static readonly string CertificateFile = "WebServer.CertificateFile";

        [Document(Description = "Password to the certificate")]
        public static readonly string CertificatePassword = "WebServer.CertificatePassword";
    }
}