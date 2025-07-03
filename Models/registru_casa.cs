using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class registru_casa
    {         
        [Key]
        public int id_registru_casa { get; set; }


        [Required(ErrorMessage = "Introduceti data")]
        public DateTime data { get; set; }


        [Required(ErrorMessage = "Introduceti nr document")]
        public string nr_document { get; set; }

       public int id_chimpr { get; set; }

        [ForeignKey("conturi_sintetice")]
        [Required(ErrorMessage = "Introduceti cont sintetic")]
        public int id_cont_sintetic { get; set; }

        [ForeignKey("conturi_analitice")]
        [Required(ErrorMessage = "Introduceti cont analitic")]
        public int id_cont_analitic { get; set; }



        [Required(ErrorMessage = "Introduceti nr document")]
        public string explicatie_nr_cont_analitic { get; set; }


        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Required(ErrorMessage = "Introduceti sold initial")]
        public decimal sold_initial  { get; set; }




        [Required(ErrorMessage = "Introduceti suma")]
        [DisplayFormat(DataFormatString = "{0:N2}")]               
        public decimal suma { get; set; }



        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal intrare { get; set; }



        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal incasare { get; set; }


        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal iesire { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal plata { get; set; }


        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Required(ErrorMessage = "Introduceti sold final")]
        public decimal sold_final { get; set; }


       
        [ForeignKey("conturi_sintetice_c")]
        [Required(ErrorMessage = "Introduceti cont sintetic corespondent")]
        public int id_cont_sintetic_c { get; set; }



       
        [ForeignKey("conturi_analitice_c")]
        [Required(ErrorMessage = "Introduce-ti cont analitic corespondent")]
        public int id_cont_analitic_c { get; set; }



        [Required(ErrorMessage = "Introduceti tip")]
        public string tip { get; set; }


        public string tip_document { get; set; }

        public string sortare { get; set; }

        public conturi_sintetice conturi_sintetice { get; set; }

        public conturi_analitice conturi_analitice { get; set; }


        public conturi_sintetice conturi_sintetice_c { get; set; }

        public conturi_analitice conturi_analitice_c { get; set; }

    }
}