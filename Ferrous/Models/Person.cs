﻿namespace Ferrous.Models
{
    public partial class Person
    {
        public string Name { get; set; }
        public string Mino { get; set; }
        public string College { get; set; }
        public string ContingentLeaderNo { get; set; }
        public string Sex { get; set; }

        public Contingents ContingentLeaderNoNavigation { get; set; }
    }
}
