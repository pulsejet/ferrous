using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ferrous.Models;
using static Ferrous.Misc.Authorization;
using Ferrous.Misc;
using System.IO;
using System.Net.Http.Headers;
using OfficeOpenXml;
using System;

namespace Ferrous.Controllers
{
    [Produces("application/json")]
    [Route("api/People")]
    public class PeopleController : ControllerBase
    {
        private readonly ferrousContext _context;

        public PeopleController(ferrousContext context)
        {
            _context = context;
        }

        // GET: api/People
        [HttpGet][LinkRelation(LinkRelationList.self)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.PEOPLE_GET)]
        public EnumContainer GetPeople()
        {
            var people = _context.Person;
            foreach (var person in people)
            {
                new LinksMaker(User, Url).FillPersonLinks(person);
            }

            return new EnumContainer(
                people,
                new LinkHelper()
                .SetOptions(User, this.GetType(), Url)
                .AddLink(nameof(GetPeople))
                .AddLink(nameof(PostPerson))
                .GetLinks()
            );
        }

        // GET: api/People/search?id=2
        [HttpGet("find"), LinkRelation(LinkRelationList.self)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.PEOPLE_GET)]
        public async Task<IActionResult> FindPerson([FromQuery] string id) {
            return await GetPerson(id).ConfigureAwait(false);
        }

        // GET: api/People/5
        [HttpGet("{id}"), LinkRelation(LinkRelationList.self)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.PERSON_GET_DETAILS)]
        public async Task<IActionResult> GetPerson([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var person = await _context.Person
                                       .Include(p => p.allottedCA)
                                       .SingleOrDefaultAsync(
                                           m => m.Mino.ToUpperInvariant() == id.ToUpperInvariant());

            if (person == null)
            {
                return NotFound();
            } else
            {
                new LinksMaker(User, Url).FillPersonLinks(person);
            }

            return Ok(person);
        }

        // PUT: api/People/5
        [HttpPut("{id}")][LinkRelation(LinkRelationList.update)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.PERSON_PUT)]
        public async Task<IActionResult> PutPerson([FromRoute] string id, [FromBody] Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != person.Mino)
            {
                return BadRequest();
            }

            Utilities.Log(_context, HttpContext, $"Update person {person.Mino} {person.Name}", 1, true);
            _context.Entry(person).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonExists(id))
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

        // POST: api/People
        [HttpPost][LinkRelation(LinkRelationList.create)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.PERSON_POST)]
        public async Task<IActionResult> PostPerson([FromBody] Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Person.Add(person);
            Utilities.Log(_context, HttpContext, $"Add person {person.Mino} {person.Name}", 1, true);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PersonExists(person.Mino))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetPerson", new { id = person.Mino }, person);
        }

        // DELETE: api/People/5
        [HttpDelete("{id}")][LinkRelation(LinkRelationList.delete)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.PERSON_DELETE)]
        public async Task<IActionResult> DeletePerson([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var person = await _context.Person.SingleOrDefaultAsync(m => m.Mino == id);
            if (person == null)
            {
                return NotFound();
            }

            Utilities.Log(_context, HttpContext, $"Delete person {person.Mino} {person.Name}", 1, true);

            _context.Person.Remove(person);
            await _context.SaveChangesAsync();

            return Ok(person);
        }

        private bool PersonExists(string id)
        {
            return _context.Person.Any(e => e.Mino == id);
        }

        [HttpGet("forward"), LinkRelation(LinkRelationList.overridden)]
        [Misc.Authorization(ElevationLevels.CoreGroup, PrivilegeList.PERSON_GET_DETAILS)]
        public async Task<IActionResult> GetPersonForward([FromQuery] string id){
            var httpWebRequest = (System.Net.HttpWebRequest) System.Net.WebRequest.Create("https://api2.moodi.org/user/" + id);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) {
                string json = "{\"secret\":\"mimi123\"}";
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = await httpWebRequest.GetResponseAsync();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
                var result = streamReader.ReadToEnd();
                return Content(result, "application/json");
            }
        }

        private enum UploadSheetColumns {
            name = 1,
            mino = 2,
            college = 3,
            city = 4,
            purchaser = 5,
            gender = 6,
            clno = 7,
            rem = 8,
        }

        [LinkRelation(LinkRelationList.overridden)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.PERSON_POST)]
        [HttpPost("upload-sheet"), DisableRequestSizeLimit]
        public async Task<ActionResult> UploadSheet()
        {
            var file = Request.Form.Files[0];
            if (file.Length > 0)
            {
                string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                var stream = file.OpenReadStream();

                /* Maintain a list of updated things */
                var updatedPeople = new List<Person>();
                var updatedContingents = new List<Contingent>();

                /* Read the worksheet */
                using (ExcelPackage package = new ExcelPackage(stream)) {
                    ExcelWorksheet workSheet = package.Workbook.Worksheets[0];
                    for (int i = workSheet.Dimension.Start.Row + 1;
                            i <= workSheet.Dimension.End.Row;
                            i++)
                    {
                        string clno = getValue(workSheet, i, UploadSheetColumns.clno);
                        string mino = getValue(workSheet, i, UploadSheetColumns.mino);
                        Contingent contingent = _context.Contingents.SingleOrDefault(c => c.ContingentLeaderNo == clno);

                        /* Create contingent if it does not exist */
                        if (contingent == null) {
                            contingent = new Contingent();
                            contingent.ContingentLeaderNo = clno;
                            _context.Contingents.Add(contingent);
                            _context.SaveChanges();
                            updatedContingents.Add(contingent);
                        }

                        Person person = _context.Person.SingleOrDefault(p => p.Mino == mino);

                        /* Create person if does not exist */
                        if (person == null) {
                            person = new Person();
                            person.Name = getValue(workSheet, i, UploadSheetColumns.name);
                            person.ContingentLeaderNo = clno;
                            person.Mino = mino;
                            person.College = getValue(workSheet, i, UploadSheetColumns.college);
                            person.Sex = (getValue(workSheet, i, UploadSheetColumns.gender).ToLower().Contains('f')) ? "F" : "M";
                            _context.Person.Add(person);
                            _context.SaveChanges();
                            updatedPeople.Add(person);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                return Ok();
            }
            return BadRequest("Nothing found here");
        }

        private string getValue(ExcelWorksheet workSheet, int i, UploadSheetColumns column) {
            var value = workSheet.Cells[i, (int) column].Value;
            if (value != null) {
                return value.ToString();
            }
            return String.Empty;
        }
    }
}