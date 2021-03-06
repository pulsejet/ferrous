﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Ferrous.Misc
{
    public class Authorization : Attribute, IAuthorizationFilter
    {
        private static ImmutableList<FerrousIdentity> staticIdentities;

        public static readonly string IDENTITIES_JSON_FILE = "identities.json";

        private readonly ElevationLevels _elevationLevel;
        private readonly PrivilegeList _privilege;

        public Authorization(ElevationLevels elevationLevel, PrivilegeList privilege )
        {
            _elevationLevel = elevationLevel;
            _privilege = privilege;
        }

        public ElevationLevels elevationLevel {
            get { return _elevationLevel; }
        }

        public PrivilegeList privilege {
            get { return _privilege; }
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated) {
                context.Result = new UnauthorizedResult();

            } else if (!hasPrivilege(
                context.HttpContext.User.Identity.Name,
                _elevationLevel,
                _privilege))
            {
                context.Result = new StatusCodeResult(403);
            }
        }

        public static void reloadIdentities() {
            staticIdentities = Utilities.LoadJson<FerrousIdentity>(IDENTITIES_JSON_FILE).ToImmutableList();
        }

        public static bool hasPrivilege(string username, ElevationLevels minElevation, PrivilegeList priv = PrivilegeList.NONE)
        {
            if (staticIdentities == null) {
                reloadIdentities();
            }

            if (username == String.Empty) { return false; }

            FerrousIdentity id = staticIdentities.FirstOrDefault(m => m.username == username);
            if (id == null) { return false; }

            if (id.elevation <= (int)minElevation) { return true; }

            if (priv != PrivilegeList.NONE &&
                id.privileges.Contains((int)priv)) {
                    return true;
            }

            return false;
        }

        public static bool hasBuildingAuth(string username, string buildingShortName)
        {
            if (staticIdentities == null) {
                reloadIdentities();
            }

            if (username == String.Empty) { return false; }

            FerrousIdentity id = staticIdentities.FirstOrDefault(m => m.username == username);
            if (id == null) { return false; }

            if (id.elevation == (int) ElevationLevels.SuperUser) { return true; }

            return id.locations != null && id.locations.Contains(buildingShortName);
        }

        public class FerrousIdentity
        {
            public string username { get; set; }
            public string salt { get; set; }
            public string password { get; set; }
            public int elevation { get; set; }
            public List<string> locations { get; set; }
            public ImmutableList<int> privileges { get; set; }
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
            PERSON_DELETE = 10,

            BUILDINGS_GET = 11,
            BUILDING_GET_DETAILS = 12,
            BUILDING_PUT = 13,
            BUILDING_POST = 14,
            BUILDING_DELETE = 15,

            ROOMALLOCATIONS_GET = 16,
            ROOMALLOCATIONS_GET_DETAILS = 17,
            ROOMALLOCATIONS_PUT = 18,
            ROOMALLOCATIONS_POST = 19,
            ROOMALLOCATIONS_DELETE = 20,

            ROOMS_GET = 21,
            ROOM_GET_DETAILS = 22,
            ROOM_PUT = 23,
            ROOM_POST = 24,
            ROOM_DELETE = 25,
            DESK1_APPROVE = 101,
            DESK1_UNAPPROVE = 102,

            CONTINGENTARRIVALS_GET = 26,
            CONTINGENTARRIVALS_GET_DETAILS = 27,
            CONTINGENTARRIVALS_PUT = 28,
            CONTINGENTARRIVALS_POST = 29,
            CONTINGENTARRIVALS_DELETE = 30,

            ROOM_ALLOT = 10001,
            ROOM_MARK = 10002,
            ROOM_CREATE = 10003,

            EXPORT_DATA = 11000,

            BILL_CONTINGENT_ARRIVAL = 12001,

            UPLOAD_ROOM_SHEET = 13001,
            UPLOAD_PEOPLE_SHEET = 13002,

            MANAGE_USERS = 100000,
            VIEW_LOGS = 100001
        }

        public enum ElevationLevels
        {
            SuperUser = 0,
            CoreGroup = 1,
            Coordinatior = 2,
            Organizer = 3
        }
    }
}
