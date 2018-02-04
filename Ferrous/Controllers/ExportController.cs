using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ferrous.Models;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

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

                /* Return the file */
                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, 
                    "aapplication/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "export.xlsx");
            }
        }
    }
}
