using System;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace SmppServer
{
    public class Program
    {
        static public IConfigurationRoot Configuration { get; set; }
  
        public static void Main(string[] args)
        {
            #region readconfig

            var configread = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json");
            Configuration = configread.Build();
            string Ipadrr = Configuration["Ip"];
            int port = Convert.ToInt32(Configuration["port"]);
            TcpHelper.StartServer(Ipadrr, port);
            TcpHelper.Listen();
            #endregion
        }
    }
}