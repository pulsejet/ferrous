using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Ferrous.Misc
{
    public static class Utilities
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

        public static class SHA
        {
            public static string GenerateSHA256String(string inputString)
            {
                SHA256 sha256 = SHA256.Create();
                byte[] bytes = Encoding.UTF8.GetBytes(inputString);
                byte[] hash = sha256.ComputeHash(bytes);
                return GetStringFromHash(hash);
            }

            private static string GetStringFromHash(byte[] hash)
            {
                StringBuilder result = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    result.Append(hash[i].ToString("X2"));
                }
                return result.ToString();
            }
        }
    }
}
