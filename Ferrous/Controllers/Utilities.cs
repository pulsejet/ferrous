﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public static bool HasPrivilege(string username, int minElevation, PrivilegeList HasPrivilege = PrivilegeList.NONE)
        {
            if (username == String.Empty) return false;

            List<FerrousIdentity> identities = LoadJson<FerrousIdentity>(IDENTITIES_JSON_FILE);
            FerrousIdentity id = identities.FirstOrDefault(m => m.username == username);
            if (id == null) return false;

            if (id.elevation <= minElevation) return true;
            if (HasPrivilege != PrivilegeList.NONE && 
                id.privileges.Contains( (int) HasPrivilege) ) return true;

            return false;
        }

        public static int dbCInt(Object o)
        {
            int test;
            if (int.TryParse(o.ToString(), out test)) return test;
            else return 0;
        }

        public class FerrousIdentity
        {
            public string username;
            public string password;
            public int elevation;
            public List<int> privileges;
        }

        public enum PrivilegeList
        {
             NONE = 0,
             CONTINGENT_GET_DETAILS = 1,
             CONTINGENTS_GET = 2,
             CONTINGENT_PUT = 3,
             CONTINGENT_POST = 4,
             CONTINGENT_DELETE = 5,

             PEOPLE_GET = 6,
             PERSON_GET_DETAILS = 7,
             PERSON_PUT = 8,
             PERSON_POST = 9,
             PERSON_DELETE = 10
        }
    }
}
