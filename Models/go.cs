using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class go
    {    
	
        [Key]
        public int id_girant { get; set; }      

        public string nume_prenume { get; set;}

        [DisplayFormat(DataFormatString = "{0:0}")]
        public long CNP { get; set; }




        public int nr_contract { get; set; }

        public int an { get; set; }



    }
}