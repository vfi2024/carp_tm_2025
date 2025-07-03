using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class dobanda_neplatita
    {
        [Key]
        public int id_dobanda_neplatita { get; set; }
        public int nrcarnet { get; set; }
        public decimal dobanda { get; set; }
        public int luna { get; set; }


    }
}