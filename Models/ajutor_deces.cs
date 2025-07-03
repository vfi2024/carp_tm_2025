
using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class ajutor_deces
    {
        [Key]
        public int idajdeces { get; set; }

        public decimal contributieanterioara { get; set; }
        public DateTime lunaajdecesveche { get; set; }
        public decimal contributieveche { get; set; }
        public DateTime lunaajdecesnoua { get; set; }
        public decimal contributienoua { get; set; }
    }
}
