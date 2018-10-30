using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferrous.Models
{
    public class LogEntry
    {
        public LogEntry()
        {
            Timestamp = DateTime.Now;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string username { get; set; }
        public string message { get; set; }
        public int level { get; set; }
    }
}
