using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class nr_chitanta
    {


        [Key]
        public int idnr_chitanta { get; set; }
        public int nrchitanta { get; set; }
        public string nume_user { get; set; }
        public int id_user { get; set; }
    }
}