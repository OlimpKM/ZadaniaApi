using System.Text.Json.Serialization;

namespace ZadaniaApi.Models
{
   public class Uzytkownik
   {
      public int Id { get; set; }
      public string Username { get; set; } = string.Empty;
      public string PasswordHash { get; set; } = string.Empty; // Szyfrowane hasło
      public string Rola { get; set; } = "User";

      // Jeden użytkownik ma wiele zadań
      // [JsonIgnore] zapobiega pętli, gdybyśmy chcieli pobrać Usera z jego zadaniami
      [JsonIgnore]
      public virtual ICollection<Zadanie> Zadania { get; set; } = new List<Zadanie>();
   }
}
