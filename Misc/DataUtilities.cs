using Ferrous.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Ferrous.Misc.Utilities;

namespace Ferrous.Misc
{
    public static class DataUtilities
    {
        public async static Task<Contingent[]> GetExtendedContingents(ferrousContext _context)
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
                contingent.Person = new HashSet<Person>();
                contingent.ContingentArrival = new HashSet<ContingentArrival>();
            });

            return cts;
        }

        public static void UpdateWebSock(Room[] rooms, IHubContext<WebSocketHubs.BuildingUpdateHub> hubContext) {
            if (rooms.Length == 0) { return; }
            int[] roomIds = rooms.Select(r => r.RoomId).ToArray();
            List<string> locations = rooms.Select(r => r.Location).Distinct().ToList();
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
                    if (room.Status == 4) {
                        building.CapacityNotReady += room.Capacity;
                        building.RoomsNotReady++;
                    }

                    if (room.Status == 6) {
                        building.CapacityMaintainance += room.Capacity;
                        building.RoomsMaintainance++;
                    }

                    if (room.Status != 1) { continue; }

                    building.RoomsTotal++;
                    building.CapacityEmpty += room.Capacity;

                    int partialSum = 0;

                    foreach (var roomA in room.RoomAllocation)
                    {
                        partialSum += roomA.Partial;

                        if (roomA.Partial <= 0)
                        {
                            building.RoomsFilled++;
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
                        if (partialSum <= 0) {
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
    }
}
