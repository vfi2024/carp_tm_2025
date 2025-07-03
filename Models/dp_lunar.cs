using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class dp_lunar
    {
        
	 [Key]        
     public int id_dp_lunar { get; set; }

     public int nrcarnet { get; set; }
        
     public decimal dp { get; set; }

     public DateTime luna { get; set; }   
        
        
     public decimal trei { get; set; }

    }
}