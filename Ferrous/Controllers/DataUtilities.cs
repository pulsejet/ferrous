using Ferrous.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Ferrous.Utilities;

namespace Ferrous.Controllers
{
    public class DataUtilities
    {
        public async static Task<Contingents[]> GetExtendedContingents(ferrousContext _context)
        {
            Contingents[] cts = await _context.Contingents
                                        .Include(m => m.Person)
                                        .Include(m => m.ContingentArrival).ToArrayAsync();

            foreach (var contingent in cts)
            {
                int RegMale = 0, RegFemale = 0,
                    ArrivedM = 0, ArrivedF = 0,
                    OnSpotM = 0, OnSpotF = 0;

                Parallel.ForEach(contingent.Person, person => {
                    if (person == null) return;
                    if (person.Sex == "M") RegMale += 1;
                    else if (person.Sex == "F") RegFemale += 1;
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
            }

            return cts;
        }
    }
}
