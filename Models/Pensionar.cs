using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace carp_tm_2025.Models
{

    public class Pensionar
    { 


        [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
     
        public int nrcarnet { get; set; }

        [Required(ErrorMessage="Introduceti nume") ] 
        public string nume { get; set; }
        [Required(ErrorMessage = "Introduceti prenume")] 
        public string prenume { get; set; }
        [Required(ErrorMessage = "Introduceti cnp")] 
        public string cnp { get; set; }

         [Required(ErrorMessage = "Introduceti judet")] 
        public string judet { get; set; }
        [Required(ErrorMessage = "Introduceti localitate")] 
        public string localitate { get; set; }
        [Required(ErrorMessage = "Introduceti strada")] 
        public string strada { get; set; }
         [Required(ErrorMessage = "Introduceti nr")] 
        public string nr { get; set; }
        [Required(ErrorMessage = "Introduceti bloc")] 
        public string bloc { get; set; }
         [Required(ErrorMessage = "Introduceti ap")] 
        public string ap { get; set; }

        [Required(ErrorMessage = "Introduceti telefon")]
        public string telefon { get; set; }

        [Required(ErrorMessage = "Introduceti e-mail")]
        public string email { get; set; }


        [Required(ErrorMessage = "Introduceti nr cupon")]    
        public string nrcupon { get; set; }

        [Required(ErrorMessage = "Introduceti pensie")] 
        public decimal pensie { get; set; }


         [Required(ErrorMessage = "Introduceti taxa inscriere")] 
         public decimal taxainscriere { get; set; }

        public DateTime datainscriere { get; set; }

        public DateTime lunaimp { get; set; }


        [DisplayFormat(DataFormatString = "{0:N2}")]
        [DefaultValue(0)]
        public decimal soldimp { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal credit_imprumut { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal debit_imprumut { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        [DefaultValue(0)]
        public decimal restimp { get; set; }

        [DefaultValue(0)]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal acimp { get; set; }
        
        public DateTime? dataimp { get; set; }
        public DateTime? LLL { get; set; }

        public int desfasurator { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal sold_461 { get; set; }


        [DefaultValue(0)]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal soldcotiz { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        [DefaultValue(0)]
        public decimal incascotiz { get; set; }

        
        public DateTime lunaajdeces { get; set; }

       
        [DefaultValue("-")]
        public string EXCL { get; set; }
                  


        public decimal soldimp2016 { get; set; }
        public decimal soldcotiz2016 { get; set; }
        public decimal soldajdeces2016 { get; set; }
        public DateTime lunaajdeces2016 { get; set; }
        public decimal sold_DP2016 { get; set; }
        public decimal credit_imprumut2016 { get; set; }
        public decimal debit_imprumut2016 { get; set; }
        public decimal sold_dobanda2016 { get; set; }

        public decimal sold_461_2016 { get; set; }
        public decimal sold_462_2016 { get; set; }


        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal soldajdeces { get; set; }

        public decimal solddobanda { get; set; }

        public decimal sold_DP { get; set; }
        public DateTime luna_DP { get; set; }

        public decimal soldtaxainscr { get; set; }

        public int id_delegat { get; set; }

        public int id_stare { get; set; }

    }


  






   
        
    








}