using Ferrous.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static Ferrous.Misc.Utilities;

namespace Ferrous.Misc
{
    public static class DataUtilities
    {
        public async static Task<Contingent[]> GetExtendedContingents(ferrousContext _context, bool keepIncludes = false)
        {
            Contingent[] cts = await _context.Contingents
                                        .Include(m => m.Person)
                                        .Include(m => m.ContingentArrival).ToArrayAsync();

            Parallel.ForEach(cts, contingent =>
            {
                int RegMale = 0, RegFemale = 0,
                    ArrivedM = 0, ArrivedF = 0,
                    OnSpotM = 0, OnSpotF = 0;

                foreach (var person in contingent.Person) {
                    if (person == null) { continue; }
                    if (person.Sex == "M") { RegMale += 1; }
                    else if (person.Sex == "F") { RegFemale += 1; }
                };

                foreach (var ca in contingent.ContingentArrival)
                {
                    if (ca.Approved) {
                        ArrivedM += dbCInt(ca.Male);
                        ArrivedF += dbCInt(ca.Female);
                        OnSpotM += dbCInt(ca.MaleOnSpot);
                        OnSpotF += dbCInt(ca.FemaleOnSpot);
                    }
                };

                contingent.Male = RegMale.ToString();
                contingent.Female = RegFemale.ToString();
                contingent.ArrivedM = ArrivedM.ToString() + ((OnSpotM > 0) ? " + " + OnSpotM : "");
                contingent.ArrivedF = ArrivedF.ToString() + ((OnSpotF > 0) ? " + " + OnSpotF : "");

                if (!keepIncludes) {
                    contingent.Person = new HashSet<Person>();
                    contingent.ContingentArrival = new HashSet<ContingentArrival>();
                }
            });

            return cts;
        }

        public static void UpdateWebSock(Room[] rooms, IHubContext<WebSocketHubs.BuildingUpdateHub> hubContext) {
            var locations = new List<string>();
            int[] roomIds = {};
            if (rooms != null && rooms.Length > 0) {
                roomIds = rooms.Select(r => r.RoomId).ToArray();
                locations = rooms.Select(r => r.Location).Distinct().ToList();
            }
            locations.Add("ALL");
            foreach (string group in locations) {
                hubContext.Clients.Group(group).SendAsync("updated", roomIds);
            }
        }

        public static Building[] GetExtendedBuildings(Building[] buildings, string clno)
        {
            Parallel.ForEach(buildings, building =>
            {
                building.CapacityEmpty = 0;
                foreach (var room in building.Room)
                {
                    if (room.Status > 0) {
                        building.RoomsTotal++;
                    }

                    if (room.Status == 4) {
                        building.CapacityNotReady += room.Capacity;
                        building.RoomsNotReady++;
                    }

                    if (room.Status == 6) {
                        building.CapacityMaintainance += room.Capacity;
                        building.RoomsMaintainance++;
                    }

                    if (room.Status == 8) {
                        building.CapacityReserved += room.Capacity;
                        building.RoomsReserved++;
                    }

                    if (room.Status != 1) { continue; }

                    building.CapacityEmpty += room.Capacity;

                    int partialSum = 0;

                    foreach (var roomA in room.RoomAllocation)
                    {
                        partialSum += roomA.Partial;

                        if (roomA.Partial <= 0)
                        {
                            building.CapacityFilled += room.Capacity;
                            building.CapacityEmpty -= room.Capacity;
                            if (roomA.ContingentLeaderNo == clno) {
                                building.AlreadyAllocated += room.Capacity;
                            }
                            break;
                        }

                        /* Prevent overfilled rooms from changing stats */
                        if (roomA.Partial > room.Capacity) {
                            roomA.Partial = room.Capacity;
                        }

                        building.CapacityFilled += roomA.Partial;
                        building.CapacityEmpty -= roomA.Partial;

                        if (roomA.ContingentLeaderNo == clno) {
                            building.AlreadyAllocated += roomA.Partial;
                        }
                    }

                    if (room.RoomAllocation.Count > 0) {
                        if (partialSum < 0 || partialSum >= room.Capacity) {
                            building.RoomsFilled++;
                        } else {
                            building.RoomsPartial++;
                        }
                    } else {
                        building.RoomsEmpty++;
                    }
                }
                building.Room = null;
            });

            return buildings;
        }

        public static void FillCAPerson(ClaimsPrincipal user, IUrlHelper url, CAPerson caPerson, Person[] people, string clno, bool links = true) {
            if (links) {
                new LinksMaker(user, url).FillCAPersonLinks(caPerson);
            }
            var person = people.SingleOrDefault(m => m.Mino == caPerson.Mino);
            if (person != null) {
                caPerson.person = person;

                if (links) {
                    new LinksMaker(user, url).FillPersonLinks(person);
                }

                caPerson.Sex = person.Sex;
                if (person.ContingentLeaderNo != clno) {
                    // Bad Contingent Leader
                    caPerson.flags += "BCL";
                }
                if (person.allottedCA != null) {
                    // Person already approved (in another subcontingent etc)
                    caPerson.flags += "PAA";
                }
            } else {
                // No Registered Person
                caPerson.flags += "NRP";
                caPerson.Sex = "?";
            }
        }

        public static int raMattress(RoomAllocation roomA) {
            if (roomA.Partial < 0) {
                return roomA.Room.Mattresses;
            } else {
                int leftCount = roomA.Room.Mattresses;
                foreach (RoomAllocation broomA in roomA.Room.RoomAllocation) {
                    if (broomA.Partial < 0) { return 0; }
                    if (broomA.Sno == roomA.Sno) {
                        if (roomA.Partial < leftCount) {
                            return roomA.Partial;
                        } else {
                            if (leftCount > 0) {
                                return leftCount;
                            }
                            return 0;
                        }
                    } else {
                        leftCount -= broomA.Partial;
                    }
                }
            }
            return 0;
        }

        public static int mattress(ContingentArrival ca, string sex) {
            int count = 0;
            foreach (RoomAllocation roomA in ca.RoomAllocation.Where(m => m.Room.LocationNavigation.Sex == sex)) {
                count += raMattress(roomA);
            }
            return count;
        }

        public static string clName(ContingentArrival ca) {
            foreach (Person p in ca.ContingentLeaderNoNavigation.Person) {
                if (p.Mino == ca.ContingentLeaderNo) {
                    return p.Name;
                }
            }
            return "N/A";
        }

        public static bool hasPartial(ContingentArrival ca, string sex) {
            foreach (RoomAllocation roomA in ca.RoomAllocation.Where(m => m.Room.LocationNavigation.Sex == sex)) {
                if (roomA.Partial > 0 && roomA.Partial < roomA.Room.Capacity) {
                    return true;
                }
            }
            return false;
        }

        public static string GetContingentCollege(Contingent contingent) {
            string college = "N/A";
            if (contingent.Person.Count > 0) {
                Person cl = contingent.Person.Where(m => m.Mino.ToUpper() == contingent.ContingentLeaderNo.ToUpper()).FirstOrDefault();
                if (cl != null) {
                    college = cl.College;
                } else {
                    college = "?" + contingent.Person.First().College;
                }
            }
            return college;
        }
    }
}
