using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace carp_tm_2025.Models
{
    public class registru_jurnal
    //OK cek all de jos dec 2023
    {
        [Key]
        public int id_registru_jurnal { get; set; }



        [Required(ErrorMessage = "Introduceti data")]
        public DateTime data { get; set; }
                
        public int id_document { get; set; }

        [Required(ErrorMessage = "Introduceti nr document")]
        public string nr_document { get; set; }


        [ForeignKey("conturi_sintetice_D")]
        public int id_CS_debitor { get; set; }
                
        
        [ForeignKey("conturi_analitice_D")]
        public int id_CA_debitor { get; set; }


        [ForeignKey("conturi_sintetice_C")]
        public int id_CS_creditor { get; set; }


        [ForeignKey("conturi_analitice_C")]
        public int id_CA_creditor { get; set; }

        [Required(ErrorMessage = "Introduceti explicatie nr cont analitic")]
        public string explicatie_nr_cont_analitic { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal SI_CS_debit { get; set; }


        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal SI_CA_debit { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal SI_CS_credit { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal SI_CA_credit { get; set; }


        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal debit { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]        
        [Required(ErrorMessage = "Introduceti suma")]
        public decimal credit { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal SF_CS_debit { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal SF_CA_debit { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal SF_CS_credit { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal SF_CA_credit { get; set; }


        public int tip_document { get; set; }

        public string sortare { get; set; }

        public conturi_sintetice conturi_sintetice_D { get; set; }

        public conturi_sintetice conturi_sintetice_C { get; set; }

        public conturi_analitice conturi_analitice_C { get; set; }

        public conturi_analitice conturi_analitice_D { get; set; }


    }
}