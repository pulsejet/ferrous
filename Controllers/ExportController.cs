using System.Drawing;
using System.IO;
using System.Linq;
using Ferrous.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Threading.Tasks;
using static Ferrous.Misc.Utilities;
using static Ferrous.Misc.Authorization;
using Ferrous.Misc;
using System.Collections.Generic;
using System.Text;

namespace Ferrous.Controllers
{
    [Route("api/export")]
    public class ExportController : Controller
    {
        private readonly ferrousContext _context;

        public ExportController(ferrousContext context)
        {
            _context = context;
        }

        [HttpGet("api_spec")]
        [LinkRelation(LinkRelationList.self)]
        [Produces("application/json")]
        public List<Link> GetApiSpec()
        {
            return new LinksMaker(User, Url).API_SPEC();
        }

        [HttpGet("contingent-arrival-bill/{id}")]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.BILL_CONTINGENT_ARRIVAL)]
        public async Task<ActionResult> GetContingentArrivalBill([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contingentArrival = await _context.ContingentArrival
                .Include(m => m.RoomAllocation)
                    .ThenInclude(m => m.Room)
                .SingleOrDefaultAsync(m => m.ContingentArrivalNo == id);

            contingentArrival.RoomAllocation = contingentArrival.RoomAllocation
                    .OrderBy(m => m.Room.RoomName)
                    .OrderBy(m => m.Room.Location).ToArray();

            if (contingentArrival == null)
            {
                return NotFound();
            }

            return View("Bill", contingentArrival);
        }

        // GET: api/export
        [HttpGet]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.EXPORT_DATA)]
        public async Task<IActionResult> Get()
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                /* Fill up the package */
                await FillContingentsWorksheet(package.Workbook.Worksheets.Add("Contingents")).ConfigureAwait(false);
                FillPeopleWorksheet(package.Workbook.Worksheets.Add("People"));
                FillRoomsWorksheet(package.Workbook.Worksheets.Add("Rooms"));

                /* Return the file */
                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "export.xlsx");
            }
        }

        /// <summary>
        /// Fill the given worksheet with contingents data
        /// </summary>
        /// <param name="contingentsWorksheet">Passed worksheet</param>
        async Task<int> FillContingentsWorksheet(ExcelWorksheet contingentsWorksheet)
        {
            var contingents = await DataUtilities.GetExtendedContingents(_context);

            int[] widths = { 11, 11, 11, 11, 11, 25 };
            setColumnWidths(widths, contingentsWorksheet);

            for (int k = 2; k <= 5; k++) {
                contingentsWorksheet.Column(k).Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Right;
            }

            string[] headers = {
                "CL No",
                "Reg (M)",
                "Reg (F)",
                "D1 Approved (M)",
                "D1 Approved (F)",
                "Remark"
            };
            setColumnHeaders(headers, contingentsWorksheet);

            int rowno = 2;
            foreach (var contingent in contingents)
            {
                object[] cells = {
                    contingent.ContingentLeaderNo,
                    IntIfNumber(contingent.Male),
                    IntIfNumber(contingent.Female),
                    IntIfNumber(contingent.ArrivedM),
                    IntIfNumber(contingent.ArrivedF),
                    contingent.Remark
                };
                setRow(cells, rowno++, contingentsWorksheet);
            }
            return 1;
        }

        /// <summary>
        /// Fills the passed worksheet with people data
        /// </summary>
        /// <param name="peopleWorksheet">ExcelWorksheet to fill</param>
        void FillPeopleWorksheet(ExcelWorksheet peopleWorksheet)
        {
            var people = _context.Person.ToList()
                    .OrderBy(m => m.ContingentLeaderNo);

            int[] widths = { 11, 25, 45, 4, 11 };
            setColumnWidths(widths, peopleWorksheet);

            string[] headers = {
                        "MI No",
                        "Name",
                        "College",
                        "Sex",
                        "CL No"
                    };
            setColumnHeaders(headers, peopleWorksheet);

            Color lightGray = ColorTranslator.FromHtml("#ecf0f1");

            int i = 2; string prevCl = ""; bool col2 = false;
            foreach (var person in people)
            {
                if (person.ContingentLeaderNo != prevCl)
                {
                    prevCl = person.ContingentLeaderNo;
                    col2 = !col2;
                }

                if (col2)
                {
                    peopleWorksheet.Row(i).Style.Fill.PatternType = ExcelFillStyle.Solid;
                    peopleWorksheet.Row(i).Style.Fill.BackgroundColor.SetColor(lightGray);
                }

                if (person.Mino == person.ContingentLeaderNo)
                {
                    peopleWorksheet.Row(i).Style.Font.Color.SetColor(Color.Blue);
                    peopleWorksheet.Row(i).Style.Font.Bold = true;
                }

                object[] cells = {
                    person.Mino,
                    person.Name,
                    person.College,
                    person.Sex,
                    person.ContingentLeaderNo
                };
                setRow(cells, i++, peopleWorksheet);
            }
        }

        /// <summary>
        /// Fills the passed ExcelWorksheet with rooms data
        /// </summary>
        /// <param name="roomsWorksheet">ExcelWorksheet to fill</param>
        void FillRoomsWorksheet(ExcelWorksheet roomsWorksheet)
        {
            var rooms = _context.Room
                    .Where(m => m.Status != 0)
                    .Include(m => m.RoomAllocation).ToList()
                    .OrderBy(m => m.Location);

            int[] widths = { 11, 11, 15, 4, 6, 45 };
            setColumnWidths(widths, roomsWorksheet);

            string[] headers = {
                "Location",
                "Room",
                "Allocated",
                "Capacity",
                "Lock No",
                "Remark"
            };
            setColumnHeaders(headers, roomsWorksheet);

            int i = 2;
            foreach (var room in rooms)
            {
                int Partial = 0;
                StringBuilder roomAlloc = new StringBuilder();
                foreach (var roomA in room.RoomAllocation)
                {
                    if (roomA.Partial > 0)
                    {
                        roomAlloc.Append(roomA.ContingentLeaderNo + " - " + roomA.Partial + "; ");
                        Partial += roomA.Partial;
                    }
                    else
                    {
                        roomAlloc.Append(roomA.ContingentLeaderNo);
                        Partial = -1;
                    }
                }

                if (room.Status == 6 || room.Status == 4 || room.Status == 1)
                {
                    roomsWorksheet.Row(i).Style.Fill.PatternType = ExcelFillStyle.Solid;

                    if (room.Status == 4)
                    {
                        roomsWorksheet.Row(i).Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                    }
                    else if (room.Status == 6)
                    {
                        roomsWorksheet.Row(i).Style.Font.Color.SetColor(Color.White);
                        roomsWorksheet.Row(i).Style.Fill.BackgroundColor.SetColor(Color.Magenta);
                    }
                    else if (room.Status == 1)
                    {
                        roomsWorksheet.Row(i).Style.Font.Color.SetColor(Color.White);
                        if (Partial == 0) {
                            roomsWorksheet.Row(i).Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                        } else if (Partial < 0) {
                            roomsWorksheet.Row(i).Style.Fill.BackgroundColor.SetColor(Color.DarkRed);
                        } else if (Partial > 0) {
                            roomsWorksheet.Row(i).Style.Fill.BackgroundColor.SetColor(
                                room.Capacity - Partial > 0 ? Color.Blue : Color.DarkRed);
                        }
                    }
                }

                object[] cells = {
                    room.Location,
                    IntIfNumber(room.RoomName),
                    roomAlloc,
                    room.Capacity,
                    IntIfNumber(room.LockNo),
                    room.Remark
                };
                setRow(cells, i++, roomsWorksheet);
            }
        }

        /* =========== Utilities ========== */
        /// <summary>
        /// Sets the widths for columns
        /// </summary>
        /// <param name="widths">Array with width</param>
        /// <param name="ws">ExcelWorksheet to set</param>
        void setColumnWidths(int[] widths, ExcelWorksheet ws)
        {
            for (int c = 0; c <= widths.Count() - 1; ++c) {
                ws.Column(c + 1).Width = widths[c];
            }
        }

        /// <summary>
        /// Sets the column headers to the first row and makes it bold
        /// </summary>
        /// <param name="headers">Headers</param>
        /// <param name="ws">ExcelWorksheet to set</param>
        void setColumnHeaders(string[] headers, ExcelWorksheet ws)
        {
            for (int c = 0; c <= headers.Count() - 1; ++c) {
                ws.Cells[1, c + 1].Value = headers[c];
            }
            ws.Row(1).Style.Font.Bold = true;
        }

        /// <summary>
        /// Set row with no rowno
        /// </summary>
        /// <param name="cells">Array of cell data</param>
        /// <param name="rowno">Number of the row (1 is header)</param>
        /// <param name="ws">ExcelWorksheet to set</param>
        void setRow(object[] cells, int rowno, ExcelWorksheet ws)
        {
            for (int c = 0; c <= cells.Count() - 1; ++c) {
                ws.Cells[rowno, c + 1].Value = cells[c];
            }
        }
    }
}
