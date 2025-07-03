using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace carp_tm_2025.Models
{
    public class desfasurator_rate_modificari
    {
        [Key]
        public int id_desfasurator_rate_modificari { get; set; }

        public int id_imprumut { get; set; }

        public int nrcarnet { get; set; }

        public int nr_desfasurator { get; set; }

        public int tip_modificare { get; set; }

        public DateTime data_modificare { get; set; }
               
        public decimal sold_imprumut_modificat { get; set; }

        public decimal dobanda_imprumut { get; set; }

        public int nr_rate { get; set; }



    }
}