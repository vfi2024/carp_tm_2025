using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class conturi_sintetice
    {
      
	
  [Key]
        public int id_cont_sintetic { get; set; }

        public int nr_cont_sintetic { get; set; }

        public  string  explicatie_nr_cont_sintetic { get; set; }


        public Boolean activ { get; set; }

    }

}