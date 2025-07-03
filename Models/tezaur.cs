using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class tezaur
    {

        [Key]
        public int id_tezaur { get; set; }

        public string nr_cont_casa { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public string explicatie_cont_casa { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal SI_T  { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal incas_T { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal plata_T { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal SF_T { get; set; }
     
    }
}