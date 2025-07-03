using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class D_4612
    {
        [Key]
        public int id_d4612 { get; set; }

        public int nrcarnet_d4612 { get; set; }

        public string nume_d4612 { get; set; }

        public string localitate_d4612 { get; set; }

        public DateTime data_impr_d4612 { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal val_impr_d4612 { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal rest_impr_d4612 { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal sold_impr_d4612 { get; set; }

        // public DateTime lunaimp_impr_d4612 { get; set; }
        public string lunaimp_impr_d4612 { get; set; }


        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal debit_impr_d4612 { get; set; }


    }


}