using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ZadaniaApi.Models
{
    public class Zadanie
    {
      public int Id { get; set; }
      [JsonIgnore]
      public int IdUzytkownik { get; set; }
      // Atrybut [ForeignKey] 
      [ForeignKey("IdUzytkownik")]
      [JsonIgnore] // Nie chcemy wysyłać całego obiektu User w każdym zadaniu przez API
      public virtual Uzytkownik? Uzytkownik { get; set; }

      public string Status { get; set; } = string.Empty;
      public string Tytul { get; set; } = string.Empty;
      public string Tresc { get; set; } = string.Empty;
      public bool CzyWykonane { get; set; }
      public DateTime DataUtworzenia { get; set; } = DateTime.Now;
   }

   public class ZadanieCreateDto
   {
      public string Tytul { get; set; } = string.Empty;
      public string Tresc { get; set; } = string.Empty;
      public string Status { get; set; } = string.Empty;
   }
}
