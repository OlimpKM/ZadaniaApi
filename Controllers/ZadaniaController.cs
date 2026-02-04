using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ZadaniaApi.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ZadaniaApi.Data;
using ZadaniaApi.Models;

namespace ZadaniaApi.Controllers
{
   [Route("api/[controller]")] // adres: api/zadania
   [ApiController]
   [Authorize]

   public class ZadaniaController : ControllerBase
   {
      private readonly BazaDbContext _context;

      public ZadaniaController(BazaDbContext context)
      {
         _context = context;
      }

      // Pobieranie zadań: limit, strona i status
      // Przykłady wywołań:
      // GET api/zadania?limit=10&page=1
      // GET api/zadania?limit=20&page=2&onlyPending=true
      [HttpGet]
      public async Task<IActionResult> GetAll(
          [FromQuery] int limit = 0,       // 1. Limit (0 = wszystko)
          [FromQuery] int page = 1,        // 2. Numer strony
          [FromQuery] bool onlyPending = false) // 3. Status
      {
         // pobieramy ID użytkownika z tokena
         var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

         if (string.IsNullOrEmpty(userIdString))
            return Unauthorized("Brak identyfikatora użytkownika w tokenie.");

         int currentUserId = int.Parse(userIdString);

         // A. Bezpieczeństwo: Pobieramy tylko zadania aktualnego użytkownika
         var currentUser = User.Identity?.Name;
         var query = _context.Zadania
             .Where(z => z.IdUzytkownik == currentUserId)
             .AsQueryable();

         // B. Filtrowanie: Tylko te do realizacji
         if (onlyPending)
         {
            query = query.Where(z => z.CzyWykonane == false);
         }

         // C. Sortowanie: Od najnowszych (kluczowe przy stronicowaniu)
         query = query.OrderByDescending(z => z.Id);

         // D. Logika stronicowania (Skip & Take)
         if (limit > 0)
         {
            // Obliczamy ile rekordów pominąć
            // Strona 1: (1-1)*10 = 0 (nie pomijaj)
            // Strona 2: (2-1)*10 = 10 (pomiń pierwsze 10)
            query = query
                .Skip((page - 1) * limit)
                .Take(limit);
         }

         // E. Wykonanie zapytania asynchronicznie
         var zadania = await query.ToListAsync();

         return Ok(zadania);
      }

      // pobieranie jednego zadania po ID
      // adres: GET api/zadania/{id} (np. api/zadania/1)
      [HttpGet("{id}")]
      public async Task<IActionResult> GetById(int id)
      {
         // Szukamy zadania w bazie danych po jego kluczu głównym
         var zadanie = await _context.Zadania.FindAsync(id);

         // Jeśli nie znaleziono zadania o takim ID, zwracamy 404 Not Found
         if (zadanie == null)
         {
            return NotFound(new { message = $"Nie znaleziono zadania o ID {id}" });
         }

         return Ok(zadanie);
      }

      // dodanie zadania
      // adres: POST api/zadania
      [HttpPost]
      public async Task<ActionResult<Zadanie>> PostZadanie(ZadanieCreateDto dto)
      {
         // Pobieramy ID z tokena (z Claims)
         var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

         if (userIdClaim == null)
            return Unauthorized("Brak ID użytkownika w tokenie.");

         // mapujemy DTO na pełny model bazodanowy
         var zadanie = new Zadanie
         {
            Tytul = dto.Tytul,
            Tresc = dto.Tresc,
            Status = dto.Status,
            IdUzytkownik = int.Parse(userIdClaim),
            DataUtworzenia = DateTime.Now,
            CzyWykonane = false // Domyślnie nowe zadanie jest niewykonane
         };

         // 2. Nadpisujemy idUzytkownik – bezpieczeństwo przede wszystkim!
         zadanie.IdUzytkownik = int.Parse(userIdClaim);

         // Opcjonalnie: ustawiamy datę utworzenia na serwerową
         zadanie.DataUtworzenia = DateTime.Now;

         _context.Zadania.Add(zadanie);
         await _context.SaveChangesAsync();

         return CreatedAtAction(nameof(GetById), new { id = zadanie.Id }, zadanie);
      }

      // usunięcie zadania po ID
      // adres: DELETE api/zadania/{id}
      [HttpDelete("{id}")]
      public async Task<IActionResult> DelById(int id)
      {
         var zadanie = await _context.Zadania.FindAsync(id);
         if (zadanie == null)
         {
            return NotFound(new { message = $"Nie znaleziono zadania o ID {id}" });
         }

         _context.Zadania.Remove(zadanie);
         await _context.SaveChangesAsync();

         return Ok(zadanie);
      }

      // aktualizacja zadania
      // adres: PUT api/zadania/3
      [HttpPut("{id}")]
      public async Task<IActionResult> UpdateZadanie(int id, Zadanie zadanieZmienione)
      {
         // Pobieramy ID z tokena (z Claims)
         var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

         if (userIdClaim == null)
            return Unauthorized("Brak ID użytkownika w tokenie.");

         if (id != zadanieZmienione.Id)
         {
            return BadRequest(new { message = "ID w adresie i w obiekcie nie są zgodne" });
         }
         _context.Entry(zadanieZmienione).State = EntityState.Modified;

         try
         {
            zadanieZmienione.IdUzytkownik = int.Parse(userIdClaim);
            await _context.SaveChangesAsync();
         }
         catch (DbUpdateConcurrencyException)
         {
            if (!_context.Zadania.Any(e => e.Id == id))
            {
               return NotFound();
            }
            else { throw; }
         }

         return NoContent(); // Zwracamy status 204 (sukces, brak treści do odesłania)
      }
   }
}
