﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ferrous.Models;
using static Ferrous.Misc.Authorization;
using System;
using Ferrous.Misc;

namespace Ferrous.Controllers
{
    [Produces("application/json")]
    [Route("api/Buildings")]
    public class BuildingsController : ControllerBase
    {
        private readonly ferrousContext _context;

        public BuildingsController(ferrousContext context)
        {
            _context = context;
        }

        // GET: api/Buildings/e
        [HttpGet("e/{id}/{cano}")]
        [LinkRelation(LinkRelationList.self)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.BUILDINGS_GET)]
        public async Task<EnumContainer> GetBuildingsExtended([FromRoute] string id, [FromRoute] int cano)
        {
            Building[] buildings = await _context.Building
                                        .Include(m => m.Room)
                                            .ThenInclude(m => m.RoomAllocation)
                                        .Where(b => b.hasAuth(User))
                                        .ToArrayAsync();

            buildings = DataUtilities.GetExtendedBuildings(buildings, id);
            return FillEBuildingsLinks(buildings, id, cano);
        }

        // POST: api/Buildings/stats-update
        [HttpPost("stats-update")]
        [LinkRelation(LinkRelationList.overridden)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.BUILDINGS_GET)]
        public async Task<EnumContainer> GetStatsUpdate([FromBody] int[] roomIds)
        {
            Building[] buildings = await _context.Building
                                        .Include(m => m.Room)
                                            .ThenInclude(m => m.RoomAllocation)
                                        .Where(m => m.Room.Any(r => roomIds.Contains(r.RoomId)))
                                        .Where(b => b.hasAuth(User))
                                        .ToArrayAsync();

            buildings = DataUtilities.GetExtendedBuildings(buildings, "mark");
            return FillEBuildingsLinks(buildings, "mark", 0);
        }

        public EnumContainer FillEBuildingsLinks(Building[] buildings, string id, int cano) {
            foreach (var building in buildings) {
                (new LinksMaker(User,Url)).FillBuildingsLinks(building, id, cano);
            }

            return new EnumContainer(
                buildings,
                new LinkHelper()
                .SetOptions(User, this.GetType(), Url)
                .AddLink(nameof(GetBuildingsExtended), new { id, cano })
                .AddLink(nameof(PostBuilding))
                .GetLinks()
            );
        }

        // GET: api/Buildings/n
        [HttpGet("m/{id}/{cano}")]
        [LinkRelation(LinkRelationList.self)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.BUILDINGS_GET)]
        public Building[] GetBuildingsMin([FromRoute] string id, [FromRoute] int cano)
        {
            var buildings = _context.Building.Where(b => b.hasAuth(User)).ToArray();
            foreach (var building in buildings) {
                (new LinksMaker(User,Url)).FillBuildingsLinks(building, id, cano);
            }
            return buildings;
        }

        // GET: api/Buildings/5
        [HttpGet("{id}/{clno}/{cano}")]
        [LinkRelation(LinkRelationList.self)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.BUILDING_GET_DETAILS)]
        public async Task<IActionResult> GetBuilding([FromRoute] string id, [FromRoute] string clno, [FromRoute] int cano)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var building = await _context.Building
                .Where(m => m.Location == id)
                .Where(b => b.hasAuth(User))
                .Include(m => m.Room)
                .ThenInclude(m => m.RoomAllocation)
                .SingleOrDefaultAsync();

            if (building == null)
            {
                return NotFound();
            } else
            {
                new LinksMaker(User, Url).FillBuildingsLinks(building, clno, cano);
            }

            return Ok(building);
        }

        // PUT: api/Buildings/5
        [HttpPut("{id}")]
        [LinkRelation(LinkRelationList.update)]
        [Authorization(ElevationLevels.SuperUser, PrivilegeList.BUILDING_PUT)]
        public async Task<IActionResult> PutBuilding([FromRoute] string id, [FromBody] Building building)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != building.Location)
            {
                return BadRequest();
            }

            _context.Entry(building).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BuildingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Buildings
        [HttpPost]
        [LinkRelation(LinkRelationList.create)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.BUILDING_POST)]
        public async Task<IActionResult> PostBuilding([FromBody] Building building)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Building.Add(building);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (BuildingExists(building.Location))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetBuilding", new { id = building.Location }, building);
        }

        // DELETE: api/Buildings/5
        [HttpDelete("{id}")]
        [LinkRelation(LinkRelationList.delete)]
        [Authorization(ElevationLevels.SuperUser, PrivilegeList.BUILDING_DELETE)]
        public async Task<IActionResult> DeleteBuilding([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var building = await _context.Building.SingleOrDefaultAsync(m => m.Location == id);
            if (building == null)
            {
                return NotFound();
            }

            _context.Building.Remove(building);
            await _context.SaveChangesAsync();

            return Ok(building);
        }

        private bool BuildingExists(string id)
        {
            return _context.Building.Any(e => e.Location == id);
        }
    }
}