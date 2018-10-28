using Ferrous.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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

            foreach (var contingent in cts)
            {
                int RegMale = 0, RegFemale = 0,
                    ArrivedM = 0, ArrivedF = 0,
                    OnSpotM = 0, OnSpotF = 0;

                Parallel.ForEach(contingent.Person, person => {
                    if (person == null) { return; }
                    if (person.Sex == "M") { RegMale += 1; }
                    else if (person.Sex == "F") { RegFemale += 1; }
                });

                Parallel.ForEach(contingent.ContingentArrival, ca =>
                {
                    ArrivedM += dbCInt(ca.Male);
                    ArrivedF += dbCInt(ca.Female);
                    OnSpotM += dbCInt(ca.MaleOnSpot);
                    OnSpotF += dbCInt(ca.FemaleOnSpot);
                });

                contingent.Male = RegMale.ToString();
                contingent.Female = RegFemale.ToString();
                contingent.ArrivedM = ArrivedM.ToString() + ((OnSpotM > 0) ? " + " + OnSpotM : "");
                contingent.ArrivedF = ArrivedF.ToString() + ((OnSpotF > 0) ? " + " + OnSpotF : "");
                contingent.Person = new HashSet<Person>();
                contingent.ContingentArrival = new HashSet<ContingentArrival>();
            }

            return cts;
        }

        public async static Task<Building[]> GetExtendedBuildings(ferrousContext _context, string clno)
        {
            Building[] buildings = await _context.Building.Include(m => m.Room)
                                                 .ThenInclude(m => m.RoomAllocation)
                                                 .ToArrayAsync();

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
