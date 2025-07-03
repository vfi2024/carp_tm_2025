using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace carp_tm_2025.Models
{
    public class desfasurator_rate_arhiva
    {
        [Key]
        public int id_desfasurator_rate_arhiva { get; set; }

        public int id_imprumut { get; set; }

        public int nrcarnet { get; set; }

        public int nr_rata { get; set; }

        public int nr_rata_noua { get; set; }


        public DateTime data_rata { get; set; }


        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal sold_imp { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal rata_de_plata { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal rata_platita { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal rata_credit { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal rata_platita_initial { get; set; }


        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal dobanda { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal total { get; set; }


        public int nr_desfasurator { get; set; }

    }
}