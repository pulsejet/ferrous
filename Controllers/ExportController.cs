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
using System;

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
                        .ThenInclude(m => m.LocationNavigation)
                .Include(m => m.ContingentLeaderNoNavigation)
                    .ThenInclude(m => m.Person)
                .Include(m => m.CAPeople)
                .SingleOrDefaultAsync(m => m.ContingentArrivalNo == id);

            if (contingentArrival == null)
            {
                return NotFound();
            }

            contingentArrival.RoomAllocation = contingentArrival.RoomAllocation
                    .OrderBy(m => m.Room.RoomName)
                    .OrderBy(m => m.Room.Location).ToArray();

            contingentArrival.CAPeople = contingentArrival.CAPeople.OrderBy(m => m.Mino).ToArray();

            foreach (RoomAllocation roomA in contingentArrival.RoomAllocation) {
                if (roomA.Partial > 0) {
                    roomA.Room = await _context.Room
                                            .Include(m => m.LocationNavigation)
                                            .Include(m => m.RoomAllocation)
                                            .SingleOrDefaultAsync(m => m.RoomId == roomA.Room.RoomId);
                    roomA.Room.RoomAllocation = roomA.Room.RoomAllocation.OrderBy(m => m.Sno).ToArray();
                }
            }

            return View("Bill", contingentArrival);
        }

        [HttpGet("logs/{page}")]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.VIEW_LOGS)]
        public ActionResult GetLogs([FromRoute] int page)
        {
            const int maxNo = 5000;
            var entries = _context.LogEntry.OrderByDescending(m => m.Timestamp)
                                           .Skip(maxNo * (page - 1)).Take(maxNo)
                                           .ToArray();

            ViewData["prev"] = page == 1 ? 1 : page - 1;
            ViewData["next"] = page + 1;
            return View("Logs", entries);
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

                foreach (var hostel in _context.Building.OrderBy(m => m.Location)) {
                    FillRoomsWorksheet(package.Workbook.Worksheets.Add(hostel.Location), hostel.Location);
                }

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
            var contingents = await DataUtilities.GetExtendedContingents(_context, true);

            int[] widths = { 13, 30, 11, 11, 11, 11, 25 };
            setColumnWidths(widths, contingentsWorksheet);

            for (int k = 3; k <= 6; k++) {
                contingentsWorksheet.Column(k).Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Right;
            }

            string[] headers = {
                "CL No",
                "College",
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
                    DataUtilities.GetContingentCollege(contingent),
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
            var people = _context.Person.Include(m => m.allottedCA).ToList()
                    .OrderBy(m => m.ContingentLeaderNo);

            int[] widths = { 11, 25, 45, 4, 13, 12 };
            setColumnWidths(widths, peopleWorksheet);

            string[] headers = {
                        "MI No",
                        "Name",
                        "College",
                        "Sex",
                        "CL No",
                        "D1 Approved"
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
                    person.ContingentLeaderNo,
                    person.allottedCA != null ? "Y - " + person.allottedCA.ContingentArrivalNo : ""
                };
                setRow(cells, i++, peopleWorksheet);
            }
        }

        /// <summary>
        /// Fills the passed ExcelWorksheet with rooms data
        /// </summary>
        /// <param name="roomsWorksheet">ExcelWorksheet to fill</param>
        void FillRoomsWorksheet(ExcelWorksheet roomsWorksheet, string location)
        {
            var rooms = _context.Room
                    .Where(m => m.Status != 0 && m.Location == location)
                    .Include(m => m.RoomAllocation)
                        .ThenInclude(m => m.ContingentLeaderNoNavigation)
                    .ToList()
                    .OrderBy(m => m.Location);

            int[] widths = { 9, 10, 10, 9, 9, 9, 30, 45, 45 };
            setColumnWidths(widths, roomsWorksheet);

            string[] headers = {
                "Location",
                "Room",
                "Status",
                "Capacity",
                "Mattresses",
                "Lock No",
                "Allocated",
                "College",
                "Remark"
            };
            setColumnHeaders(headers, roomsWorksheet);

            int i = 2;
            foreach (var room in rooms)
            {
                int partialCount = 0;
                StringBuilder roomAlloc = new StringBuilder();
                StringBuilder roomAllocCollege = new StringBuilder();
                bool addPlus = false;
                foreach (var roomA in room.RoomAllocation)
                {
                    partialCount += roomA.Partial;
                    if (roomA.Partial > 0)
                    {
                        if (addPlus) {
                            roomAlloc.Append(" + ");
                            roomAllocCollege.Append(" + ");
                        }
                        roomAlloc.Append($"{roomA.ContingentLeaderNo} ({roomA.Partial})");
                        roomAllocCollege.Append($"{DataUtilities.GetContingentCollege(roomA.ContingentLeaderNoNavigation)} ({roomA.Partial})");
                        addPlus = true;
                    }
                    else
                    {
                        roomAlloc.Append(roomA.ContingentLeaderNo);
                        roomAllocCollege.Append(DataUtilities.GetContingentCollege(roomA.ContingentLeaderNoNavigation));
                    }
                }

                /* Update status with partial count */
                string status = RoomsController.getStatusStr(room.Status);
                if (partialCount < 0 || partialCount > room.Capacity) {
                    status = "FULL";
                } else if (partialCount > 0 && room.Capacity > partialCount) {
                    status = "PART";
                }

                object[] cells = {
                    room.Location,
                    IntIfNumber(room.RoomName),
                    status,
                    room.Capacity,
                    room.Mattresses,
                    IntIfNumber(room.LockNo),
                    roomAlloc,
                    roomAllocCollege,
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
