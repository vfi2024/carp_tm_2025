using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class registru_casa_centralizator
    {
        [Key]
        public int idrccd { get; set; }

        public string nrcont_CS { get; set; }

        public string explicatie_cont_CS { get; set; }       


        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal incasare { get; set; }


        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal plata { get; set; }
        
        public string sortare { get; set; }            


    }
}