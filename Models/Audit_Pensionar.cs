using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class Audit_Pensionar
    {
        [Key]
        public int logid { get; set; }

        [Required]
        public string tip_operatie { get; set; }

        [Required]
        public DateTime data { get; set; }

        [Required]
        public int utilizator { get; set; }

        public int nrcarnet { get; set; }



        public decimal sold_imprumut { get; set; }

        public decimal sold_cotizatie { get; set; }

        public DateTime luna_ajutor_deces { get; set; }

        public DateTime ultima_plata_rata { get; set; }


    }
}