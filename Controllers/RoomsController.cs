﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ferrous.Models;
using Microsoft.AspNetCore.SignalR;
using static Ferrous.Misc.Authorization;
using Ferrous.Misc;
using System.Text;
using System.Net.Http.Headers;
using OfficeOpenXml;
using System.IO;
using OfficeOpenXml.Style;

namespace Ferrous.Controllers
{
    [Produces("application/json")]
    [Route("api/Rooms")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IHubContext<WebSocketHubs.BuildingUpdateHub> _hubContext;

        private readonly ferrousContext _context;

        public RoomsController(ferrousContext context, IHubContext<WebSocketHubs.BuildingUpdateHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public void UpdateLayoutWebSocket(Room[] rooms)
        {
            DataUtilities.UpdateWebSock(rooms, _hubContext);
        }

        // GET: api/Rooms/5
        [HttpGet("{id}/{clno}/{cano}")]
        [LinkRelation(LinkRelationList.self)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.ROOMS_GET)]
        public async Task<IActionResult> GetRoom([FromRoute] int id, [FromRoute] string clno, [FromRoute] int cano)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var room = await _context.Room.Include(m => m.RoomAllocation)
                                          .Where(r => r.LocationNavigation.hasAuth(User))
                                          .SingleOrDefaultAsync(m => m.RoomId == id);

            if (room == null)
            {
                return NotFound();
            }

            new LinksMaker(User, Url).FillRoomLinks(room, clno, cano);

            return Ok(room);
        }

        // POST: api/Rooms/list/clno/cano (gets list of rooms)
        [HttpPost("list/{clno}/{cano}")]
        [LinkRelation(LinkRelationList.overridden)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.ROOMS_GET)]
        public async Task<ActionResult> GetRoomList([FromRoute] string clno, [FromRoute] int cano, [FromBody] List<int> ids)
        {
            var rooms = await _context.Room.Include(m => m.RoomAllocation)
                                           .Where(r => r.LocationNavigation.hasAuth(User))
                                           .Where(m => ids.Contains(m.RoomId))
                                           .ToListAsync();

            if (rooms == null)
            {
                return NotFound();
            }

            foreach (var room in rooms) {
                new LinksMaker(User, Url).FillRoomLinks(room, clno, cano);
            }

            return Ok(rooms);
        }

        // PUT: api/Rooms/5
        [HttpPut("{id}")]
        [LinkRelation(LinkRelationList.update)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.ROOM_PUT)]
        public async Task<IActionResult> PutRoom([FromRoute] int id, [FromBody] Room room)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != room.RoomId)
            {
                return BadRequest();
            }

            Utilities.Log(_context, HttpContext, $"Update room {room.RoomId} ({room.Location} {room.RoomName})", 2, true);

            _context.Entry(room).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            UpdateLayoutWebSocket(new [] {room});
            return NoContent();
        }

        // POST: api/Rooms
        [HttpPost]
        [LinkRelation(LinkRelationList.create)]
        [Authorization(ElevationLevels.SuperUser, PrivilegeList.ROOM_POST)]
        public async Task<IActionResult> PostRoom([FromBody] Room room)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Room.Add(room);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRoom", new { id = room.RoomId }, room);
        }

        // DELETE: api/Rooms/5
        [HttpDelete("{id}")]
        [LinkRelation(LinkRelationList.delete)]
        [Authorization(ElevationLevels.SuperUser, PrivilegeList.ROOM_DELETE)]
        public async Task<IActionResult> DeleteRoom([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var room = await _context.Room.SingleOrDefaultAsync(m => m.RoomId == id);
            if (room == null)
            {
                return NotFound();
            }

            _context.Room.Remove(room);
            await _context.SaveChangesAsync();

            return Ok(room);
        }

        private bool RoomExists(int id)
        {
            return _context.Room.Any(e => e.RoomId == id);
        }

        // POST: api/Rooms/allot/CLNo/CANo
        [HttpPost("allot/{clno}/{cano}")]
        [LinkRelation(LinkRelationList.overridden)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.ROOM_ALLOT)]
        public async Task<IActionResult> AllotRooms([FromBody] RoomAllocation[] roomAllocations, [FromRoute] string clno, [FromRoute] int cano)
        {
            var ids = roomAllocations.Select(ra => ra.RoomId).ToArray();
            Room[] rooms = await _context.Room.Where(m => ids.Contains(m.RoomId))
                                    .Include(m => m.RoomAllocation)
                                    .Where(r => r.LocationNavigation.hasAuth(User))
                                    .ToArrayAsync();
            if (rooms == null) { return BadRequest(); }

            foreach (var room in rooms) {
                var NewRoomA = roomAllocations.Where(ra => ra.RoomId == room.RoomId).SingleOrDefault();
                bool partial = NewRoomA.Partial > 0;
                bool empty = true;

                foreach (var roomA in room.RoomAllocation)
                {
                    if (roomA.Partial <= 0) { empty = false; }
                    if (!partial) { empty = false; }
                }

                if (!empty || room.Status != 1) { continue; }

                if (!partial) { NewRoomA.Partial = -1; }

                NewRoomA.ContingentLeaderNo = clno;
                NewRoomA.ContingentArrivalNo = cano;

                _context.Update(NewRoomA);
                _context.Update(room);

                Utilities.Log(_context, HttpContext, $"Allot room {room.RoomId} ({room.Location} {room.RoomName}) to {clno} #{cano} (p {NewRoomA.Partial})", 2, true);
            }

            await _context.SaveChangesAsync();

            UpdateLayoutWebSocket(rooms);

            return NoContent();
        }

        // POST: api/Rooms/mark
        [HttpPost("mark")]
        [LinkRelation(LinkRelationList.overridden)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.ROOM_MARK)]
        public async Task<ActionResult> MarkRooms([FromBody] List<int> ids, [FromQuery] int status)
        {
            var rooms = await _context.Room.Where(m => ids.Contains(m.RoomId))
                                    .Include(m => m.RoomAllocation)
                                    .Where(r => r.LocationNavigation.hasAuth(User))
                                    .ToListAsync();
            if (rooms == null) { return BadRequest(); }

            foreach(var room in rooms) {
                if (room.RoomAllocation.Count > 0) { continue; }
                room.Status = (short)status;
                _context.Update(room);
                Utilities.Log(_context, HttpContext, $"Mark room {room.RoomId} ({room.Location} {room.RoomName}) status={status}", 2, true);
            }

            await _context.SaveChangesAsync();

            UpdateLayoutWebSocket(rooms.ToArray());
            return NoContent();
        }

        [HttpGet("CreateRoomRecords/{location}/{start}/{end}/{capacity}/{prefix}")]
        [LinkRelation(LinkRelationList.overridden)]
        [Authorization(ElevationLevels.SuperUser, PrivilegeList.ROOM_CREATE)]
        public IActionResult CreateRoomRecords([FromRoute] string location, [FromRoute] int start, [FromRoute] int end, [FromRoute] int capacity, [FromRoute] string prefix)
        {
            if (prefix == "NN") prefix = "";

            StringBuilder str = new StringBuilder();
            for (int i = start; i<= end; i++)
            {
                string rn = prefix + i.ToString();
                if (_context.Room.Where(m => m.Location == location && m.RoomName == rn).Count() > 0)
                {
                    str.Append(rn + " ");
                    continue;
                }
                Room room = new Room();
                room.Location = location;
                room.RoomName = rn;
                room.Capacity = capacity;
                room.Status = 0;
                _context.Add(room);
                _context.SaveChanges();
            }
            return Content("Done -- " + str);
        }

        private enum UploadSheetColumns {
            Hostel = 1,
            RoomNo = 2,
            LockNo = 3,
            Status = 4,
            Mattresses = 5,
            Capacity = 6,
            Remark = 7,
        }

        [LinkRelation(LinkRelationList.overridden)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.UPLOAD_ROOM_SHEET)]
        [HttpPost("upload-sheet"), DisableRequestSizeLimit]
        public async Task<ActionResult> UploadSheet()
        {
            var file = Request.Form.Files[0];
            if (file.Length > 0)
            {
                string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                var stream = file.OpenReadStream();

                /* Maintain a list of updated rooms to return */
                var updatedRooms = new List<Room>();
                var failedRooms = new List<Room>();

                /* Read the worksheet */
                using (ExcelPackage package = new ExcelPackage(stream)) {
                    ExcelWorksheet workSheet = package.Workbook.Worksheets[0];
                    for (int i = workSheet.Dimension.Start.Row + 1;
                            i <= workSheet.Dimension.End.Row;
                            i++)
                    {
                        string hostel = getValue(workSheet, i, UploadSheetColumns.Hostel);
                        string roomNo = getValue(workSheet, i, UploadSheetColumns.RoomNo);
                        string lockNo = getValue(workSheet, i, UploadSheetColumns.LockNo);
                        int status = getStatusInt(getValue(workSheet, i, UploadSheetColumns.Status));
                        bool hasmattresses = getValue(workSheet, i, UploadSheetColumns.Mattresses) != "";
                        bool hascapacity = getValue(workSheet, i, UploadSheetColumns.Capacity) != "";
                        string remark = getValue(workSheet, i, UploadSheetColumns.Remark);

                        /* Check for invalid entries */
                        if (hostel == String.Empty || roomNo == String.Empty) {
                            continue;
                        }

                        /* Get the room if it exists */
                        Room room = await _context.Room.Include(r => r.LocationNavigation)
                                          .SingleOrDefaultAsync(r => r.Location == hostel && r.RoomName == roomNo);

                        /* Parse numbers */
                        bool matrsuccess = true;
                        int mattresses = 0;
                        if (hasmattresses) {
                           matrsuccess = Int32.TryParse(getValue(workSheet, i, UploadSheetColumns.Mattresses), out mattresses);
                        }

                        bool capsuccess = true;
                        int capacity = 0;
                        if (hascapacity) {
                           capsuccess = Int32.TryParse(getValue(workSheet, i, UploadSheetColumns.Capacity), out capacity);
                        }

                        /* Check invalid rooms */
                        if (room == null || !room.LocationNavigation.hasAuth(User) || status == -6 || !matrsuccess || !capsuccess) {
                            string rem = "$ERROR";
                            if (room == null) {
                                rem = "$NOT FOUND";
                            } else if (room.LocationNavigation != null && !room.LocationNavigation.hasAuth(User)) {
                                rem = "$NO AUTH";
                            } else if (status == -6) {
                                rem = "$INVALID STATUS";
                            } else if (!matrsuccess) {
                                rem = "$INVALID MATTRESSES";
                            } else if (!capsuccess) {
                                rem = "$INVALID CAPACITY";
                            }

                            room = new Room();
                            room.Location = hostel;
                            room.RoomName = roomNo;
                            room.LockNo = lockNo;
                            room.Status = status;
                            room.Remark = rem;
                            failedRooms.Add(room);
                            continue;
                        }

                        /* Update valid rooms */
                        if (lockNo != "-1" && lockNo != String.Empty) {
                            room.LockNo = lockNo;
                        }

                        if (remark != "-1" && remark != String.Empty) {
                            room.Remark = remark;
                        }

                        /* Update room statuses */
                        if (status != -5) {
                            room.Status = status;
                        }

                        /* Update mattresses */
                        if (hasmattresses) {
                            room.Mattresses = mattresses;
                        }

                        /* Update capacity */
                        if (hascapacity) {
                            room.Capacity = capacity;
                        }

                        /* Save */
                        _context.Update(room);

                        /* Update final list */
                        updatedRooms.Add(room);

                        Utilities.Log(_context, HttpContext, $"Sheet-update room {room.RoomId} ({room.Location} {room.RoomName}) status={status} lockNo={lockNo}", 2, true);
                    }

                    await _context.SaveChangesAsync();
                }

                /* Update web socket */
                UpdateLayoutWebSocket(updatedRooms.ToArray());

                return Ok(updatedRooms.Concat(failedRooms));
            }
            return BadRequest("Nothing found here");
        }

        [LinkRelation(LinkRelationList.overridden)]
        [Authorization(ElevationLevels.CoreGroup, PrivilegeList.UPLOAD_ROOM_SHEET)]
        [HttpGet("upload-sheet"), DisableRequestSizeLimit]
        public ActionResult UploadSheetSample() {
            using (ExcelPackage package = new ExcelPackage()) {
                var workSheet = package.Workbook.Worksheets.Add("Rooms");
                var allWorkSheet = package.Workbook.Worksheets.Add("AllRooms");

                /* Fill all rooms worksheet */
                int row = 1;
                foreach (var room in _context.Room.Where(r => r.LocationNavigation.hasAuth(User)).ToArray()) {
                    allWorkSheet.Cells[row, 1].Value = $"{room.Location}|{room.RoomName}";
                    allWorkSheet.Cells[row, 2].Value = room.RoomId.ToString();
                    allWorkSheet.Cells[row, 3].Value = "YES";
                    allWorkSheet.Cells[row, 4].Value = getStatusStr(room.Status);
                    allWorkSheet.Cells[row, 5].Value = room.Mattresses;
                    allWorkSheet.Cells[row, 6].Value = room.Capacity;
                    allWorkSheet.Cells[row, 7].Value = room.Remark;
                    row++;
                }

                /* Protection settings */
                workSheet.Protection.IsProtected = true;
                workSheet.Cells.Style.Locked = false;
                workSheet.Protection.AllowSelectUnlockedCells = true;
                workSheet.Protection.AllowSelectLockedCells = false;

                /* Add first row */
                setValue(workSheet, 1, UploadSheetColumns.Hostel);
                setValue(workSheet, 1, UploadSheetColumns.RoomNo);
                setValue(workSheet, 1, UploadSheetColumns.LockNo);
                setValue(workSheet, 1, UploadSheetColumns.Status);
                setValue(workSheet, 1, UploadSheetColumns.Mattresses);
                setValue(workSheet, 1, UploadSheetColumns.Capacity);
                setValue(workSheet, 1, UploadSheetColumns.Remark);

                /* Show is valid */
                int lastColumn = (int) UploadSheetColumns.Remark + 1;
                workSheet.Cells[1, lastColumn].Value = "Is Valid";
                for (int i = 2; i < 200; i++) {
                    workSheet.Cells[i, lastColumn].Formula = $"IFERROR(VLOOKUP(CONCATENATE(A{i}, \"|\", B{i}), AllRooms!A:C, 3, FALSE), \"\")";
                }
                workSheet.Column(lastColumn).Style.Locked = true;
                workSheet.Column(lastColumn).Style.Border.Left.Style = ExcelBorderStyle.Thin;

                /* Show status */
                lastColumn++;
                workSheet.Cells[1, lastColumn].Value = "Current";
                for (int i = 2; i < 200; i++) {
                    workSheet.Cells[i, lastColumn].Formula = $"IFERROR(VLOOKUP(CONCATENATE(A{i}, \"|\", B{i}), AllRooms!A:D, 4, FALSE), \"\")";
                }
                workSheet.Column(lastColumn).Style.Locked = true;

                /* Show mattresses */
                lastColumn++;
                workSheet.Cells[1, lastColumn].Value = "Mattresses";
                for (int i = 2; i < 200; i++) {
                    workSheet.Cells[i, lastColumn].Formula = $"IFERROR(VLOOKUP(CONCATENATE(A{i}, \"|\", B{i}), AllRooms!A:E, 5, FALSE), \"\")";
                }
                workSheet.Column(lastColumn).Style.Locked = true;

                /* Show mattresses */
                lastColumn++;
                workSheet.Cells[1, lastColumn].Value = "Capacity";
                for (int i = 2; i < 200; i++) {
                    workSheet.Cells[i, lastColumn].Formula = $"IFERROR(VLOOKUP(CONCATENATE(A{i}, \"|\", B{i}), AllRooms!A:F, 6, FALSE), \"\")";
                }
                workSheet.Column(lastColumn).Style.Locked = true;

                /* Show Remark */
                lastColumn++;
                workSheet.Cells[1, lastColumn].Value = "Remark";
                for (int i = 2; i < 200; i++) {
                    workSheet.Cells[i, lastColumn].Formula = $"IFERROR(VLOOKUP(CONCATENATE(A{i}, \"|\", B{i}), AllRooms!A:G, 7, FALSE), \"\")";
                }
                workSheet.Column(lastColumn).Style.Locked = true;

                /* Style first row */
                workSheet.Row(1).Style.Font.Bold = true;
                workSheet.Row(1).Style.Locked = true;
                workSheet.View.FreezePanes(2, ++lastColumn);

                /* Add hostels title */
                int HOSTELS_ROW = 4;
                int HOSTELS_COLUMN = 14;
                workSheet.Cells[HOSTELS_ROW, HOSTELS_COLUMN].Value = "Code";
                workSheet.Cells[HOSTELS_ROW, HOSTELS_COLUMN + 1].Value = "Meaning";
                workSheet.Cells[HOSTELS_ROW, HOSTELS_COLUMN].Style.Font.Bold = true;
                workSheet.Cells[HOSTELS_ROW, HOSTELS_COLUMN + 1].Style.Font.Bold = true;

                /* Add hostels list */
                row = HOSTELS_ROW + 1;
                foreach (Building building in _context.Building.Where(b => b.hasAuth(User)).OrderBy(b => b.Location).ToArray()) {
                    workSheet.Cells[row, HOSTELS_COLUMN].Value = building.Location;
                    workSheet.Cells[row, HOSTELS_COLUMN + 1].Value = building.LocationFullName;
                    row++;
                }

                /* Enlist possible statuses */
                row++;
                workSheet.Cells[row, HOSTELS_COLUMN].Value = getStatusStr(0);
                workSheet.Cells[row++, HOSTELS_COLUMN + 1].Value = "Unavailable";
                workSheet.Cells[row, HOSTELS_COLUMN].Value = getStatusStr(1);
                workSheet.Cells[row++, HOSTELS_COLUMN + 1].Value = "Available";
                workSheet.Cells[row, HOSTELS_COLUMN].Value = getStatusStr(4);
                workSheet.Cells[row++, HOSTELS_COLUMN + 1].Value = "Not ready";
                workSheet.Cells[row, HOSTELS_COLUMN].Value = getStatusStr(6);
                workSheet.Cells[row++, HOSTELS_COLUMN + 1].Value = "Needs maintainance";
                workSheet.Cells[row, HOSTELS_COLUMN].Value = getStatusStr(8);
                workSheet.Cells[row++, HOSTELS_COLUMN + 1].Value = "MI Reserved";
                workSheet.Cells[row, HOSTELS_COLUMN].Value = getStatusStr(-1);
                workSheet.Cells[row++, HOSTELS_COLUMN + 1].Value = "Unknown";

                /* Return the file */
                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "rooms.xlsx");
            }
        }

        [HttpGet("ResetRoomStatus")]
        [LinkRelation(LinkRelationList.overridden)]
        [Authorization(ElevationLevels.SuperUser, PrivilegeList.ROOM_CREATE)]
        public IActionResult ResetRoomStatus()
        {

            foreach (var room in _context.Room.Where(m => m.Location == "H15B_G")) {
                // _context.Remove(room);
                // room.Status = 0;
                //_context.Update(room);
            }
            _context.SaveChanges();

            return Ok("Done");
        }

        private void setValue(ExcelWorksheet workSheet, int i, UploadSheetColumns column) {
            workSheet.Cells[i, (int) column].Value = column.ToString();
        }

        private string getValue(ExcelWorksheet workSheet, int i, UploadSheetColumns column) {
            var value = workSheet.Cells[i, (int) column].Value;
            if (value != null) {
                return value.ToString();
            }
            return String.Empty;
        }

        private int getStatusInt(string status) {
            switch (status.ToUpper()) {
                case "":
                    return -5;
                case null:
                    return -5;
                case "UAVL":
                    return 0;
                case "AVLB":
                    return 1;
                case "NRDY":
                    return 4;
                case "MAIT":
                    return 6;
                case "RESV":
                    return 8;
                default:
                    return -6;
            }
        }

        public static string getStatusStr(int? status) {
            switch (status) {
                case 0:
                    return "UAVL";
                case 1:
                    return "AVLB";
                case 4:
                    return "NRDY";
                case 6:
                    return "MAIT";
                case 8:
                    return "RESV";
                default:
                    return "UNKN";
            }
        }
    }
}