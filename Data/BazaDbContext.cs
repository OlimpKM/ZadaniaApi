using Microsoft.EntityFrameworkCore;
using ZadaniaApi.Models;

namespace ZadaniaApi.Data
{
   public class BazaDbContext : DbContext
   {
      public BazaDbContext(DbContextOptions<BazaDbContext> options) : base(options) { }

      // To stworzy tabelę "Zadania" w bazie danych
      public DbSet<Zadanie> Zadania { get; set; }
      public DbSet<Uzytkownik> Uzytkownicy { get; set; }
   }
}
