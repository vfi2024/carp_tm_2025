using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace carp_tm_2025.Models
{
    public class desfasurator_rate_simulator
    {
                
        public int nr_rata { get; set; }

        public DateTime data_rata { get; set; }


        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal sold_imp { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal rata_de_plata { get; set; }

               

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal dobanda { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal total { get; set; }
    }
}