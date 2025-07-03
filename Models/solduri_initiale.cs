using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class solduri_initiale
    {


        [Key]
        public int id_SI { get; set; }
        public int id_CS_SI { get; set; }
        public int id_CA_SI { get; set; }
        public Decimal SI { get; set; }
        public int an { get; set; }
    }
}