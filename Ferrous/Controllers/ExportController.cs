using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Ferrous.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using static Ferrous.Utilities;

namespace Ferrous.Controllers
{
    [Route("api/export")]
    public class ExportController : ControllerBase
    {
        private readonly ferrousContext _context;

        public ExportController(ferrousContext context)
        {
            _context = context;
        }

        // GET: api/export
        [HttpGet]
        public IActionResult Get()
        {
            //Response.ContentType = "application/octet-stream";
            using (ExcelPackage package = new ExcelPackage())
            {
                List<Contingents> contingents = _context.Contingents.ToList();
                List<Person> people = _context.Person.ToList();
                List<Room> rooms = _context.Room
                    .Where(m => m.Status != 0)
                    .Include(m => m.RoomAllocation).ToList();
                rooms.OrderBy(m => m.Location);

                {
                    ExcelWorksheet contingentsWorksheet = package.Workbook.Worksheets.Add("Contingents");

                    contingentsWorksheet.Column(1).Width = 11;
                    contingentsWorksheet.Column(2).Width = 25;

                    contingentsWorksheet.Row(1).Style.Font.Bold = true;
                    contingentsWorksheet.Cells[1, 1].Value = "ContingentLeaderNo";
                    contingentsWorksheet.Cells[1, 2].Value = "Remark";
    
                    int i = 2;
                    foreach (var contingent in contingents)
                    {
                        contingentsWorksheet.Cells[i, 1].Value = contingent.ContingentLeaderNo;
                        contingentsWorksheet.Cells[i, 2].Value = contingent.Remark;
                        ++i;    
                    }
                }

                {
                    ExcelWorksheet peopleWorksheet = package.Workbook.Worksheets.Add("People");

                    peopleWorksheet.Column(1).Width = 11;
                    peopleWorksheet.Column(2).Width = 25;
                    peopleWorksheet.Column(3).Width = 45;
                    peopleWorksheet.Column(4).Width = 4;
                    peopleWorksheet.Column(5).Width = 11;

                    peopleWorksheet.Row(1).Style.Font.Bold = true;
                    peopleWorksheet.Cells[1, 1].Value = "MI No.";
                    peopleWorksheet.Cells[1, 2].Value = "Name";
                    peopleWorksheet.Cells[1, 3].Value = "College";
                    peopleWorksheet.Cells[1, 4].Value = "Sex";
                    peopleWorksheet.Cells[1, 5].Value = "CL No.";

                    int i = 2;
                    foreach (var person in people)
                    {
                        peopleWorksheet.Cells[i, 1].Value = person.Mino;
                        peopleWorksheet.Cells[i, 2].Value = person.Name;
                        peopleWorksheet.Cells[i, 3].Value = person.College;
                        peopleWorksheet.Cells[i, 4].Value = person.Sex;
                        peopleWorksheet.Cells[i, 5].Value = person.ContingentLeaderNo;
                        ++i;
                    }
                }

                {
                    ExcelWorksheet roomsWorksheet = package.Workbook.Worksheets.Add("Rooms");

                    roomsWorksheet.Column(1).Width = 11;
                    roomsWorksheet.Column(2).Width = 11;
                    roomsWorksheet.Column(3).Width = 15;
                    roomsWorksheet.Column(4).Width = 4;
                    roomsWorksheet.Column(5).Width = 6;
                    roomsWorksheet.Column(6).Width = 45;

                    roomsWorksheet.Row(1).Style.Font.Bold = true;
                    roomsWorksheet.Cells[1, 1].Value = "Location";
                    roomsWorksheet.Cells[1, 2].Value = "Room";
                    roomsWorksheet.Cells[1, 3].Value = "Allocated";
                    roomsWorksheet.Cells[1, 4].Value = "Capacity";
                    roomsWorksheet.Cells[1, 5].Value = "Lock No";
                    roomsWorksheet.Cells[1, 6].Value = "Remark";

                    int i = 2;
                    foreach (var room in rooms)
                    {
                        roomsWorksheet.Cells[i, 1].Value = room.Location;
                        roomsWorksheet.Cells[i, 2].Value = IntIfNumber(room.RoomName);

                        int Partial = 0;
                        string roomAlloc = "";
                        foreach (var roomA in room.RoomAllocation)
                        {
                            if (roomA.Partial > 0)
                            {
                                roomAlloc += roomA.ContingentLeaderNo + " - " + roomA.Partial + "; ";
                                Partial += roomA.Partial;
                            }
                            else
                            {
                                roomAlloc += roomA.ContingentLeaderNo;
                                Partial = -1;
                            }
                        }

                        roomsWorksheet.Cells[i, 3].Value = roomAlloc;

                        roomsWorksheet.Cells[i, 4].Value = room.Capacity;
                        roomsWorksheet.Cells[i, 5].Value = IntIfNumber(room.LockNo);
                        roomsWorksheet.Cells[i, 6].Value = room.Remark;

                        //Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#B7DEE8");

                        if (room.Status == 4 || room.Status == 1)
                        {
                            roomsWorksheet.Row(i).Style.Fill.PatternType = ExcelFillStyle.Solid;

                            if (room.Status == 4) roomsWorksheet.Row(i).Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                            else if (room.Status == 1)
                            {
                                roomsWorksheet.Row(i).Style.Font.Color.SetColor(Color.White);
                                if (Partial == 0) roomsWorksheet.Row(i).Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                                else if (Partial < 0) roomsWorksheet.Row(i).Style.Fill.BackgroundColor.SetColor(Color.DarkRed);
                                else if (Partial > 0 && room.Capacity - Partial > 0) roomsWorksheet.Row(i).Style.Fill.BackgroundColor.SetColor(Color.Blue);
                                else if (Partial > 0 && room.Capacity - Partial <= 0) roomsWorksheet.Row(i).Style.Fill.BackgroundColor.SetColor(Color.DarkRed);
                            }
                        }

                        ++i;
                    }
                }

                /* Return the file */
                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, 
                    "aapplication/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "export.xlsx");
            }
        }
    }
}
