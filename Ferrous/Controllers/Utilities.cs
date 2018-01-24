using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ferrous
{
    public class Utilities
    {
        public const string IDENTITIES_JSON_FILE = "identities.json";
        public const string ROOT_URL = "/";
        public const string LOGIN_URL = "/account/login.html";

        public static List<T> LoadJson<T>(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(json);
            }
        }

        public static bool HasPrivilege(string username)
        {
            if (username == String.Empty) return false;
            List<FerrousIdentity> identities = LoadJson<FerrousIdentity>(IDENTITIES_JSON_FILE);
            FerrousIdentity id = identities.FirstOrDefault(m => m.username == username);

            if ((id != null) && id.privilege > 0) return true;

            return false;
        }

        public class FerrousIdentity
        {
            public string username;
            public string password;
            public int privilege;
        }
    }
}
