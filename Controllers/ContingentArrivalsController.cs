using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ferrous.Models;
using static Ferrous.Misc.Authorization;
using System.Security.Claims;
using Ferrous.Misc;
using Microsoft.AspNetCore.SignalR;

namespace Ferrous.Controllers
{
    [Produces("application/json")]
    [Route("api/ContingentArrivals")]
    public class ContingentArrivalsController : ControllerBase
    {
        private readonly ferrousContext _context;
        private readonly IHubContext<WebSocketHubs.BuildingUpdateHub> _hubContext;

        public ContingentArrivalsController(ferrousContext context, IHubContext<WebSocketHubs.BuildingUpdateHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // DEPRECATED
        // GET: api/ContingentArrivals
        [HttpGet]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_GET)]
        public IEnumerable<ContingentArrival> GetContingentArrivals()
        {
            return _context.ContingentArrival;
        }

        // DEPRECATED
        // GET: api/ContingentArrivals/byCL/5
        [HttpGet("byCL/{clno}")]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_GET)]
        public async Task<IActionResult> GetContingentArrivalCl([FromRoute] string clno)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contingentArrival = await _context.ContingentArrival
                .Where(m => m.ContingentLeaderNo == clno)
                .Include(m => m.RoomAllocation)
                .SingleOrDefaultAsync();

            if (contingentArrival == null)
            {
                return NotFound();
            }

            return Ok(contingentArrival);
        }

        public void setAllotted(ContingentArrival contingentArrival) {
            foreach (var x in contingentArrival.RoomAllocation) {
                /* Get count */
                int count = 0;
                if (x.Partial < 0) {
                    count = x.Room.Capacity;
                } else {
                    count = x.Partial;
                }

                /* Check sex */
                if (x.Room.LocationNavigation != null &&
                    x.Room.LocationNavigation.Sex != null &&
                    x.Room.LocationNavigation.Sex.ToUpper() == "M")
                {
                    contingentArrival.AllottedMale += count;
                } else {
                    contingentArrival.AllottedFemale += count;
                }
            }
        }

        // GET: api/ContingentArrivals/5
        [HttpGet("{id}")]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_GET_DETAILS)]
        public async Task<IActionResult> GetContingentArrival([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contingentArrival = await _context.ContingentArrival
                                                .Include(m => m.RoomAllocation)
                                                    .ThenInclude(m => m.Room)
                                                        .ThenInclude(m => m.LocationNavigation)
                                                .SingleOrDefaultAsync(m => m.ContingentArrivalNo == id);

            setAllotted(contingentArrival);
            contingentArrival.RoomAllocation = null;

            new LinksMaker(User, Url).FillContingentArrivalLinks(contingentArrival);

            if (contingentArrival == null)
            {
                return NotFound();
            }

            return Ok(contingentArrival);
        }

        [HttpPut("desk1/approve/{cano}"), LinkRelation(LinkRelationList.overridden)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.DESK1_APPROVE)]
        public async Task<IActionResult> ApproveContingentArrival([FromRoute] int cano, [FromBody] ContingentArrival contingentArrivalPut)
        {
            var contingentArrival = await _context.ContingentArrival
                                                .Include(m => m.CAPeople)
                                                .SingleOrDefaultAsync(m => m.ContingentArrivalNo == cano);
            if (contingentArrival == null || contingentArrival.Approved)
            {
                return NotFound();
            }

            /* Set M/F count from posted data */
            contingentArrival.Male = contingentArrivalPut.Male;
            contingentArrival.Female = contingentArrivalPut.Female;
            contingentArrival.MaleOnSpotDemand = contingentArrivalPut.MaleOnSpotDemand;
            contingentArrival.FemaleOnSpotDemand = contingentArrivalPut.FemaleOnSpotDemand;
            contingentArrival.Remark = contingentArrivalPut.Remark;

            /* Mark people as done with */
            String[] minos = contingentArrival.CAPeople.Select(cap => cap.Mino).ToArray();
            Person[] people = await _context.Person.Where(m => minos.Contains(m.Mino)).ToArrayAsync();
            var linksMaker = new LinksMaker(User, Url);
            foreach (CAPerson caPerson in contingentArrival.CAPeople) {
                linksMaker.FillCAPersonLinks(caPerson);
                Person person = people.SingleOrDefault(m => m.Mino == caPerson.Mino);
                if (person != null) {
                    person.allottedCA = contingentArrival;
                    _context.Update(person);
                }
                DataUtilities.FillCAPerson(User, Url, caPerson, people, contingentArrival.ContingentLeaderNo, false);
            }

            /* Approve! */
            contingentArrival.Approved = true;

            Utilities.Log(_context, HttpContext, $"Approve subcontingent {contingentArrival.ContingentArrivalNo} {contingentArrival.ContingentLeaderNo})", 2, true);
            await _context.SaveChangesAsync();

            DataUtilities.UpdateWebSock(null, _hubContext);

            linksMaker.FillContingentArrivalLinks(contingentArrival);
            return Ok(contingentArrival);
        }

        [HttpPut("desk1/unapprove/{cano}"), LinkRelation(LinkRelationList.overridden)]
        [Authorization(ElevationLevels.SuperUser, PrivilegeList.DESK1_UNAPPROVE)]
        public async Task<IActionResult> UnApproveContingentArrival([FromRoute] int cano)
        {
            var contingentArrival = await _context.ContingentArrival
                                                .Include(m => m.CAPeople)
                                                .SingleOrDefaultAsync(m => m.ContingentArrivalNo == cano);
            if (contingentArrival == null || !contingentArrival.Approved) { return NotFound(); }

            /* Mark people as done with */
            String[] minos = contingentArrival.CAPeople.Select(cap => cap.Mino).ToArray();
            Person[] people = await _context.Person
                                                .Include(p => p.allottedCA)
                                                .Where(m => minos.Contains(m.Mino)).ToArrayAsync();

            var linksMaker = new LinksMaker(User, Url);
            foreach (CAPerson caPerson in contingentArrival.CAPeople) {
                linksMaker.FillCAPersonLinks(caPerson);
                Person person = people.SingleOrDefault(m => m.Mino == caPerson.Mino);
                if (person != null && person.allottedCA.ContingentArrivalNo == contingentArrival.ContingentArrivalNo) {
                    person.allottedCA = null;
                    _context.Update(person);
                }
                DataUtilities.FillCAPerson(User, Url, caPerson, people, contingentArrival.ContingentLeaderNo, false);
            }

            /* UnApprove! */
            contingentArrival.Approved = false;

            Utilities.Log(_context, HttpContext, $"Unapprove subcontingent {contingentArrival.ContingentArrivalNo} {contingentArrival.ContingentLeaderNo})", 1, true);
            await _context.SaveChangesAsync();

            DataUtilities.UpdateWebSock(null, _hubContext);

            linksMaker.FillContingentArrivalLinks(contingentArrival);
            return Ok(contingentArrival);
        }

        // GET: api/ContingentArrivals/desk1?id=5
        [HttpGet("desk1"), LinkRelation(LinkRelationList.self)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_GET_DETAILS)]
        public async Task<IActionResult> GetDesk1([FromQuery] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contingentArrival = await _context.ContingentArrival
                                                .Include(m => m.CAPeople)
                                                .Include(m => m.RoomAllocation)
                                                    .ThenInclude(m => m.Room)
                                                        .ThenInclude(m => m.LocationNavigation)
                                                .SingleOrDefaultAsync(m => m.ContingentArrivalNo == id);

            if (contingentArrival == null)
            {
                return NotFound();
            }

            List<string> minos = contingentArrival.CAPeople.Select(m => m.Mino).ToList();
            Person[] people = await _context.Person.Where(m => minos.Contains(m.Mino))
                                                    .Include(m => m.allottedCA)
                                                    .ToArrayAsync();

            foreach (CAPerson caPerson in contingentArrival.CAPeople) {
                DataUtilities.FillCAPerson(User, Url, caPerson, people, contingentArrival.ContingentLeaderNo);
            }

            var linksMaker = new LinksMaker(User, Url);
            foreach (var roomA in contingentArrival.RoomAllocation) {
                linksMaker.FillRoomAllocationLinks(roomA);
            }

            setAllotted(contingentArrival);

            await _context.SaveChangesAsync();

            contingentArrival.PeopleFemale = contingentArrival.CAPeople.Count(m => m.Sex.ToUpper() == "F");
            contingentArrival.PeopleMale = contingentArrival.CAPeople.Count(m => m.Sex.ToUpper() == "M");

            linksMaker.FillContingentArrivalLinks(contingentArrival);

            return Ok(contingentArrival);
        }

        // GET: api/ContingentArrivals/stats
        [HttpGet("stats"), LinkRelation(LinkRelationList.overridden)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_GET_DETAILS)]
        public async Task<ActionResult> GetStats() {
            ContingentArrival[] contingentArrivals = await _context.ContingentArrival
                                                .Include(m => m.RoomAllocation)
                                                .ToArrayAsync();

            /* Counts approved from Desk 1 */
            var d1Approved = contingentArrivals.Where(ca => ca.Approved == true);
            int? d1ApprovedMale = d1Approved.Select(m => m.Male).Sum();
            int? d1ApprovedFemale = d1Approved.Select(m => m.Female).Sum();

            /* Registered people total */
            int regMale = await _context.Person.Where(p => p.Sex == "M").CountAsync();
            int regFemale = await _context.Person.Where(p => p.Sex == "F").CountAsync();

            /* Registered people approved from Desk 1 */
            int regApprovedMale = await _context.Person.Where(p => p.Sex == "M" && p.allottedCA != null).CountAsync();
            int regApprovedFemale = await _context.Person.Where(p => p.Sex == "F" && p.allottedCA != null).CountAsync();

            /* On Spot given from Desk 2 */
            int? onSpotGivenMale = contingentArrivals.Select(m => m.MaleOnSpot).Sum();
            int? onSpotGivenFemale = contingentArrivals.Select(m => m.FemaleOnSpot).Sum();

            /* On Spot demand (forms with non-zero allocated on-spot) */
            var pendingOnSpot =  contingentArrivals.Where(m => m.MaleOnSpot == null && m.FemaleOnSpot == null);
            int? onSpotPendingDemandMale = pendingOnSpot.Select(m => m.MaleOnSpotDemand).Sum();
            int? onSpotPendingDemandFemale = pendingOnSpot.Select(m => m.FemaleOnSpotDemand).Sum();

            /* Forms filled not approved from Desk 1 */
            var pendingApproval = contingentArrivals.Where(m => !m.Approved);
            int? regPendingApprovalMale = pendingApproval.Select(m => m.Male).Sum();
            int? regPendingApprovalFemale = pendingApproval.Select(m => m.Female).Sum();

            /* Desk-1-Approved people with rooms given from desk 2 (EXCLUDING On Spot) */
            var withRooms = contingentArrivals.Where(m => m.RoomAllocation.Any());
            int? regWithRoomsMale = withRooms.Select(m => m.Male).Sum();
            int? regWithRoomsFemale = withRooms.Select(m => m.Female).Sum();

            return Ok(new {
                M = new {
                    reg = regMale,
                    d1Approved = d1ApprovedMale,
                    regApproved = regApprovedMale,
                    regPendingApproval = regPendingApprovalMale,
                    onSpotGiven = onSpotGivenMale,
                    onSpotPendingDemand = onSpotPendingDemandMale,
                    regWithRooms = regWithRoomsMale,
                }, F = new {
                    reg = regFemale,
                    d1Approved = d1ApprovedFemale,
                    regApproved = regApprovedFemale,
                    regPendingApproval = regPendingApprovalFemale,
                    onSpotGiven = onSpotGivenFemale,
                    onSpotPendingDemand = onSpotPendingDemandFemale,
                    regWithRooms = regWithRoomsFemale,
                }
            });
        }

        // POST: api/ContingentArrivals/desk1/cap?id=5
        [HttpPost("desk1/cap/{cano}"), LinkRelation(LinkRelationList.overridden)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_PUT)]
        public async Task<IActionResult> PostCAPerson([FromRoute] int cano, [FromBody] CAPerson caPerson)
        {
            caPerson.CANav = await _context.ContingentArrival.SingleOrDefaultAsync(m => m.ContingentArrivalNo == cano);

            // Do not allow changing approved CAs
            if (caPerson.CANav.Approved) {
                return Unauthorized();
            }

            _context.CAPerson.Add(caPerson);

            Utilities.Log(_context, HttpContext, $"Add person {caPerson.Mino} to subcontingent #{caPerson.CANav.ContingentArrivalNo} {caPerson.CANav.ContingentLeaderNo})", 2, true);
            await _context.SaveChangesAsync();

            Person[] people = await _context.Person.Where(m => m.Mino == caPerson.Mino).ToArrayAsync();
            DataUtilities.FillCAPerson(User, Url, caPerson, people, caPerson.CANav.ContingentLeaderNo);

            return CreatedAtAction("PostCAPerson", new { id = caPerson.Sno }, caPerson);
        }

        // DELETE: api/ContingentArrivals/desk1/5
        [HttpDelete("desk1/cap/{id}"), LinkRelation(LinkRelationList.delete)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_PUT)]
        public async Task<IActionResult> DeleteCAPerson([FromRoute] int id)
        {
            var caPerson = await _context.CAPerson.Include(m => m.CANav).SingleOrDefaultAsync(m => m.Sno == id);
            if (caPerson == null)
            {
                return NotFound();
            }

            // Do not allow changing approved CAs
            if (caPerson.CANav.Approved) {
                return Unauthorized();
            }

            _context.CAPerson.Remove(caPerson);
            Utilities.Log(_context, HttpContext, $"Remove person {caPerson.Mino} from subcontingent #{caPerson.CANav.ContingentArrivalNo} {caPerson.CANav.ContingentLeaderNo})", 2, true);
            await _context.SaveChangesAsync();

            DataUtilities.UpdateWebSock(null, _hubContext);

            return Ok(caPerson);
        }

        // PUT: api/ContingentArrivals/5
        [HttpPut("{id}")]
        [LinkRelation(LinkRelationList.update)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_PUT)]
        public async Task<IActionResult> PutContingentArrival([FromRoute] int id, [FromBody] ContingentArrival contingentArrival)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != contingentArrival.ContingentArrivalNo)
            {
                return BadRequest();
            }

            _context.Entry(contingentArrival).State = EntityState.Modified;

            try
            {
                Utilities.Log(_context, HttpContext, $"Update subcontingent #{contingentArrival.ContingentArrivalNo} {contingentArrival.ContingentLeaderNo})", 1, true);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContingentArrivalExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            DataUtilities.UpdateWebSock(null, _hubContext);

            return NoContent();
        }

        // POST: api/ContingentArrivals
        [HttpPost]
        [LinkRelation(LinkRelationList.create)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_POST)]
        public async Task<IActionResult> PostContingentArrival([FromBody] ContingentArrival contingentArrival)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            contingentArrival.CreatedOn = DateTime.UtcNow.AddHours(5).AddMinutes(30);
            _context.ContingentArrival.Add(contingentArrival);
            await _context.SaveChangesAsync();

            new LinksMaker(User, Url).FillContingentArrivalLinks(contingentArrival);
            Utilities.Log(_context, HttpContext, $"Create subcontingent #{contingentArrival.ContingentArrivalNo} {contingentArrival.ContingentLeaderNo})", 0, true);

            return CreatedAtAction("GetContingentArrival", new { id = contingentArrival.ContingentArrivalNo }, contingentArrival);
        }

        // DELETE: api/ContingentArrivals/5
        [HttpDelete("{id}")]
        [LinkRelation(LinkRelationList.delete)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.CONTINGENTARRIVALS_DELETE)]
        public async Task<IActionResult> DeleteContingentArrival([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contingentArrival = await _context.ContingentArrival.SingleOrDefaultAsync(m => m.ContingentArrivalNo == id);
            if (contingentArrival == null)
            {
                return NotFound();
            }

            Utilities.Log(_context, HttpContext, $"Delete subcontingent #{contingentArrival.ContingentArrivalNo} {contingentArrival.ContingentLeaderNo})", 1, true);
            _context.ContingentArrival.Remove(contingentArrival);
            await _context.SaveChangesAsync();

            return Ok(contingentArrival);
        }

        private bool ContingentArrivalExists(int id)
        {
            return _context.ContingentArrival.Any(e => e.ContingentArrivalNo == id);
        }
    }
}