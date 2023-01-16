using System;

namespace gamecrew.Helpers
{
    public class AppSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string Key { get; set; }
        public int Exp { get; set; }
        public string CaptchaKey {get;set;}
    }
}