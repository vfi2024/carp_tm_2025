using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class imprumuturi
    {
        [Key]
        public int idimprum { get; set; }

        public int nrcarnet { get; set; }


        [Required(ErrorMessage = "Introduceti valoare imprumut")]
        [Range(typeof(decimal), "0", "90000000", ErrorMessage = "Introduceti o valoare pozitiva pentru sold imprumut")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal valoare_imprumut { get; set; }


        [Required(ErrorMessage = "Introduceti data imprumut")]
        public DateTime data_imprumut { get; set; }

        public string nume { get; set; }

        [Required(ErrorMessage = "Introduceti nr contract")]
        public int nr_contract { get; set; }

        public int mod_acordare { get; set; }


    }
 }
