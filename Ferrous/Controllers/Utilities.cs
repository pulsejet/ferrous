using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ferrous.Controllers
{
    public class Utilities
    {
        public const string HTML_MIME_TYPE = "text/html";
        public const string ROOT_URL = "/";
        public const string LOGIN_URL = "/account/login.html";

        public static string InlineFile(string path)
        {
            return File.ReadAllText("wwwroot/" + path);
        }

        public static List<T> LoadJson<T>(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(json);
            }
        }

        public static int dbCInt(Object o)
        {
            if (o is null) return 0;
            if (int.TryParse(o.ToString(), out var test)) return test;
            return 0;
        }

        public static object IntIfNumber(Object o)
        {
            if (o is null) return null;
            if (int.TryParse(o.ToString(), out int test)) return test;
            return o;
        }
    }
}
