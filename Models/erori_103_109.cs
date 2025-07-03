using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class erori_103_109
    {
        [Key]
        public int id_erori { get; set; }

        public int nrcarnet { get; set; }

        public decimal SI_103 { get; set; }
        public decimal SI_109 { get; set; }

        public decimal credit_103 { get; set; }
        public decimal credit_109 { get; set; }

        public decimal debit_103 { get; set; }
        public decimal debit_109 { get; set; }

        public decimal dobanda_neplatita { get; set; }

        public decimal SF_103 { get; set; }
        public decimal SF_109 { get; set; }

        public decimal erori_103 { get; set; }
        public decimal erori_109 { get; set; }
    }


}