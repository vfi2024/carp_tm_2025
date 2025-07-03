using System.ComponentModel.DataAnnotations;

namespace carp_tm_2025.Models
{
    public class Utilizatori
    {
        [Key]
        public int IDUser { get; set; }
        public string nume_user { get; set; }
    }
}
