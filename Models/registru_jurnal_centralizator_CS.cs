using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class registru_jurnal_centralizator_CS
    {
       
        public string nrcont_CS_D { get; set; }

        public string explicatie_cont_CS_D { get; set; }

        public string nrcont_CS_C { get; set; }

        public string explicatie_cont_CS_C { get; set; }


        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal total_debit { get; set; }


        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal total_credit { get; set; }
        
        public string sortare { get; set; }


      


    }
}