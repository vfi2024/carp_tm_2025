using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class cekDPInit
    {
        [Key]
     public int id { get; set; }
     public DateTime date3RP1 { get; set; }
     public DateTime DreferintaEnd1 { get; set; }
     public DateTime Ddebit6Start1 { get; set; }
     public DateTime Ddebit6End1 { get; set; }
     public DateTime luna_ins_dp1 { get; set; }

    }
}