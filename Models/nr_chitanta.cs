using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class CD
    {
        [Key]
        public int id_CD { get; set; }
        public string nr_cont_CD { get; set; }

        public string explicatie_nr_cont_CD { get; set; }

        public decimal SI_CD { get; set; }

        public decimal debit_CD { get; set; }

        public decimal credit_CD { get; set; }

        public decimal SF_CD { get; set; }

    }


}