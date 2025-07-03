using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class conturi_analitice
    {
      
	
       [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]


        public int id_cont_analitic { get; set; }


        [Required(ErrorMessage = "Introduceti nr cont sintetic")]
        public int id_cont_sintetic { get; set; }


        [Required(ErrorMessage = "Introduceti nr cont analitic")]

        public String nr_cont_analitic { get; set; }



        [Required(ErrorMessage = "Introduceti explicatie nr  cont analitic")]

        public String explicatie_nr_cont_analitic { get; set; }
        
    }

}