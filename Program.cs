using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ZadaniaApi.Data;
using ZadaniaApi.Security;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// konfiguracja klucza i autentykacji
var key = Encoding.ASCII.GetBytes(Const.SecurityKey);

// dodanie serwisu
builder.Services.AddControllers();

var conn = builder.Configuration.GetConnectionString("DefaultConnection"); 
if (string.IsNullOrWhiteSpace(conn))
{ 
   // lokalnie
   var dbPath = Path.Combine(AppContext.BaseDirectory, "zadania.db"); 
   conn = $"Data Source={dbPath}"; 
} 
// rejestruje bazê danych Sqlight
builder.Services.AddDbContext<BazaDbContext>(options => options.UseSqlite(conn));
// konfiguracja CORS (Cross-Origin Resource Sharing) znosi restrykcje:
builder.Services.AddCors(options => {
   options.AddDefaultPolicy(policy => {   // Tworzysz politykê domyœln¹
      policy.SetIsOriginAllowed(_ => true) // Zezwalaj na ¿¹dania z dowolnego adresu Ÿród³owego (nawet null)
            .AllowAnyHeader()  // Wszystkie nag³ówki
            .AllowAnyMethod(); // Wszystkie metody
   });
});
// JwtBearer jako domyœlny sposób sprawdzania to¿samoœci.
// API przy ka¿dym ¿¹daniu bêdzie szukaæ nag³ówka Authorization: Bearer<token>
builder.Services.AddAuthentication(options =>
{
   options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
   options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
   options.TokenValidationParameters = new TokenValidationParameters
   {
      ValidateIssuerSigningKey = true, // Serwer sprawdza czy token zosta³ podpisany unikalnym kluczem
      IssuerSigningKey = new SymmetricSecurityKey(key),  // tajne has³o do klucza symetrycznego u¿ywanego do weryfikacji podpisu tokena
      ValidateIssuer = false, // Wy³¹czasz sprawdzanie kto wystawi³ token
      ValidateAudience = false   // Wy³¹czasz sprawdzanie dla kogo by³ przeznaczony
   };
});
// zarz¹dzanie uprawnieniami – aktywacja mechanizmu kontroli dostêpu, który na podstawie zweryfikowanej to¿samoœci
builder.Services.AddAuthorization();
// eksplorator punktów koñcowych – us³uga zbiera informacje o strukturze API, umo¿liwiaj¹ca automatyczne generowanie dokumentacji
builder.Services.AddEndpointsApiExplorer();
// generator dokumentacji technicznej – konfiguracja interaktywnego interfejsu testowego umo¿liwiaj¹c bezpieczne testowanie endpointów
builder.Services.AddSwaggerGen(c =>
{
   c.SwaggerDoc("v1", new OpenApiInfo { Title = "Zadania API", Version = "v1" });
   // Definicja zabezpieczenia
   c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
   {
      Description = "Wpisz: Bearer {twoj_token}",
      Name = "Authorization",
      In = ParameterLocation.Header,
      Type = SecuritySchemeType.Http,
      Scheme = "Bearer"
   });
   // Dodanie wymogu
   c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {{new OpenApiSecurityScheme
            {Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer" // Musi byæ identyczny z nazw¹ powy¿ej
                }
            }, new string[] { }}
    });
});

var app = builder.Build();
// Konfiguracja ¿¹dañ w potoku HTTP
if (!app.Environment.IsDevelopment())
{
   app.UseExceptionHandler("/Home/Error");
   // Domyœlna wartoœæ HSTS to 30 dni. Mo¿esz chcieæ zmieniæ tê wartoœæ w scenariuszach produkcyjnych, patrz https://aka.ms/aspnetcore-hsts.
   app.UseHsts();
}
// przekierowanie na bezpieczny protokó³ https
app.UseHttpsRedirection();
app.UseStaticFiles();
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
   app.UseSwaggerUI(options =>
   {
      options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
      options.RoutePrefix = string.Empty; // przekierowanie na Swagger(a)
   });
//}
// routing ¿¹dañ – odpowiada za dopasowanie adresu URL do konkretnej w kontrolerach API
app.UseRouting();
// wykonanie polityk wspó³dzielenia – eliminacja blokad przegl¹darkowych
app.UseCors();
// w³¹czenie autoryzacji w potoku (Pipeline)
// identyfikacja u¿ytkownika - proces rozpoznawania to¿samoœci na podstawie przes³anych poœwiadczeñ i przypisywania ich do bie¿¹cego kontekstu ¿¹dania
app.UseAuthentication();
// zarz¹dzanie uprawnieniami - proces weryfikacji dostêpu zidentyfikowanego u¿ytkownika do chronionych zasobów i funkcji aplikacji
app.UseAuthorization();
// mapowanie punktów koñcowych
app.MapControllers();

// optymalizacja zasobów - zaawansowane zarz¹dzanie plikami statycznymi z obs³ug¹ kompresji, cache'owania i unikalnych wersji plików
app.MapStaticAssets();

app.Run();
