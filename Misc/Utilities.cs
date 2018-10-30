using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ferrous.Models;
using Microsoft.AspNetCore.Http;

namespace Ferrous.Misc
{
    public static class Utilities
    {
        public static readonly string HTML_MIME_TYPE = "text/html";
        public static readonly string ROOT_URL = "/";
        public static readonly string LOGIN_URL = "/account/login.html";

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

        public static void WriteJson<T>(string path, List<T> list)
        {
            using (StreamWriter w = new StreamWriter(path))
            {
                w.Write(Newtonsoft.Json.JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented));
            }
        }

        public static int dbCInt(Object o)
        {
            if (o is null) { return 0; }
            if (int.TryParse(o.ToString(), out var test)) { return test; }
            return 0;
        }

        public static object IntIfNumber(Object o)
        {
            if (o is null) { return null; }
            if (int.TryParse(o.ToString(), out int test)) { return test; }
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

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static void Log(ferrousContext dbContext, HttpContext httpContext, string message, int level, bool no_save = false) {
            var entry = new LogEntry();
            entry.Timestamp = DateTime.Now;
            entry.username = httpContext.User.Identity.Name;
            entry.message = message;
            entry.level = level;
            dbContext.Add(entry);
            if (!no_save) {
                dbContext.SaveChanges();
            }
        }
    }
}
