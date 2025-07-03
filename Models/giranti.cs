using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class giranti
    {    
	
        [Key]
        public int id_girant { get; set; }

        public int nrcarnet { get; set; }

        public string nume_prenume { get; set;}

        [DisplayFormat(DataFormatString = "{0:0}")]
        public long CNP { get; set; }

        public string judet { get; set; }

        public string localitate { get; set; }

        public string strada { get; set; }

        public int nr { get; set; }

        public string bloc { get; set; }

        public string ap { get; set; }

        public decimal venit { get; set; }

        public long telefon { get; set; }


        public int nr_contract { get; set; }

        public DateTime data_imprumut { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal valoare_imprumut { get; set; }

       

    }
}