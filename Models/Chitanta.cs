using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class Chitanta
    {     

            public Chitanta()
            {

                data = DateTime.Now;

            }


            [Key]
            public int idchitanta { get; set; }

            //uniq si calcul automat
            [Required(ErrorMessage = "Introduceti nr chitanta")]
            [Range(typeof(int), "0", "90000000", ErrorMessage = "Introduceti o valoare pozitiva pentru nr chitanta")]
            public int nrch { get; set; }


            public string serie { get; set; }

            [Required(ErrorMessage = "Introduceti data")]
            public DateTime data { get; set; }

            public int nrcarnet { get; set; }

            [Required(ErrorMessage = "Introduceti cotizatie")]
            [DisplayFormat(DataFormatString = "{0:N2}")]
            public decimal cotizatie { get; set; }

            [DisplayFormat(DataFormatString = "{0:N2}")]
            [Required(ErrorMessage = "Introduceti aj deces")]
            public decimal ajdeces { get; set; }

            [Required(ErrorMessage = "Introduceti luna aj deces")]
            public DateTime lunaajdeces { get; set; }

            [DisplayFormat(DataFormatString = "{0:N2}")]
            public decimal rata_de_plata { get; set; }

            [Required(ErrorMessage = "Introduceti rata")]
            [DisplayFormat(DataFormatString = "{0:N2}")]
            public decimal rata { get; set; }

            [DisplayFormat(DataFormatString = "{0:N2}")]
            public decimal diferenta_rata { get; set; }


            [DisplayFormat(DataFormatString = "{0:N2}")]
            public decimal dobanda_penalizatoare { get; set; }

            [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
            [Required(ErrorMessage = "Introduceti dobanda")]
            public decimal dobanda { get; set; }

            public decimal credit_imprumut { get; set; }

            public decimal debit_imprumut { get; set; }

            [Required(ErrorMessage = "Introduceti taxa inscriere")]
            [Range(typeof(decimal), "0", "90000000", ErrorMessage = "Introduceti o valoare pozitiva pentru taxa inscriere")]
            public decimal taxainscr { get; set; }

            [DisplayFormat(DataFormatString = "{0:N2}")]
            public decimal total { get; set; }

            public string nume { get; set; }

            public int id_utilizator { get; set; }

            public Boolean analitic { get; set; }

            public string tip_operatie { get; set; }

            public int tip_document { get; set; }

        }
    }
