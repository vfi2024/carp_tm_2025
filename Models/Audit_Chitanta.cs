using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace carp_tm_2025.Models
{
   

    public class Audit_Chitanta
    {
        [Key]
        public int logidch { get; set; }

        [Required]
        public string tip_operatie { get; set; }
        
        [Required]
        public DateTime data { get; set; }

        [Required]
        public int utilizator { get; set; }


        public int idchitanta { get; set; }

        public int nrchitanta { get; set; }


        public DateTime data_chitanta { get; set; }

        public int nrcarnet { get; set; }



        public decimal rata { get; set; }

        public decimal dobanda { get; set; }
        
        public decimal cotizatie { get; set; }

        public decimal contributie { get; set; }

        public DateTime luna_ajutor_deces { get; set; }


        public string editare { get; set; }
    }

}