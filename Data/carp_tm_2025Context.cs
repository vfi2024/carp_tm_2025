using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace carp_tm_2025.Data
{
    public class carp_tm_2025Context : DbContext
    {
        public carp_tm_2025Context (DbContextOptions<carp_tm_2025Context> options)
            : base(options)
        {
        }

        public DbSet<carp_tm_2025.Models.Pensionar> Pensionars { get; set; } = default!;

        public DbSet<carp_tm_2025.Models.Audit_Pensionar> Audit_Pensionar { get; set; } = default!;
        public DbSet<carp_tm_2025.Models.Chitanta> Chitantas { get; set; } = default!;
        public DbSet<carp_tm_2025.Models.imprumuturi> imprumuturis { get; set; } = default!;
        public DbSet<carp_tm_2025.Models.Utilizatori> utilizatoris { get; set; } = default!;
        public DbSet<carp_tm_2025.Models.ajutor_deces> ajutordeces { get; set; } = default!;
        public DbSet<carp_tm_2025.Models.desfasurator_rate> desfasurator_rate { get; set; } = default!;
        public DbSet<carp_tm_2025.Models.desfasurator_rate_modificari> desfasurator_rate_modificari { get; set; } = default!;
        public DbSet<carp_tm_2025.Models.desfasurator_rate_arhiva> desfasurator_rate_arhiva { get; set; } = default!;
        public DbSet<carp_tm_2025.Models.registru_jurnal> registru_jurnal { get; set; } = default!;
        public DbSet<carp_tm_2025.Models.conturi_analitice> conturi_analitice { get; set; } = default!;
        public DbSet<carp_tm_2025.Models.conturi_sintetice> conturi_sintetice { get; set; } = default!;
        public DbSet<carp_tm_2025.Models.registru_casa> registru_casa { get; set; } = default!;
        public DbSet<carp_tm_2025.Models.nr_chitanta> nr_chitanta { get; set; } = default!;
        public DbSet<carp_tm_2025.Models.registru_casa_centralizator> registru_casa_centralizator_delegati { get; set; } = default!;

        public DbSet<carp_tm_2025.Models.solduri_initiale> solduri_initiale { get; set; } = default!;

        public DbSet<carp_tm_2025.Models.Audit_Chitanta> Audit_Chitanta { get; set; } = default!;

        public DbSet<carp_tm_2025.Models.dp_lunar> dp_lunar { get; set; } = default!;

        public DbSet<carp_tm_2025.Models.cekDPInit> cekDPInit { get; set; } = default!;

        public DbSet<carp_tm_2025.Models.dobanda_neplatita> dobanda_neplatita { get; set; } = default!;

        public DbSet<carp_tm_2025.Models.desfasurator_rate_luna> desfasurator_rate_luna { get; set; } = default!;

        public DbSet<carp_tm_2025.Models.erori_103_109> erori_103_109 { get; set; } = default!;

        public DbSet<carp_tm_2025.Models.giranti> girantis { get; set; } = default!;

        public DbSet<carp_tm_2025.Models.go> go { get; set; } = default!;


    }
}
