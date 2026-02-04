# Personal Task Manager API & Frontend

System demonstracyjny do zarządzania zadaniami zbudowany w architekturze klient-serwer. Projekt obejmuje zabezpieczone API w technologii .NET oraz lekki frontend w czystym JavaScript.

![Aplikacja na Microsoft Azure](https://zadaniaapi-fga4bkb6h3bubqap.canadacentral-01.azurewebsites.net)
> Uwaga: Pierwsze uruchomienie może zająć od 10 do 90 sekund (wynika to z ograniczeń darmowego planu F1 Free / tzw. "cold start").


## 🚀 Kluczowe Funkcjonalności

- **Autentykacja JWT**: Bezpieczne logowanie za pomocą tokenów JSON Web Token.
- **Zarządzanie Użytkownikami**: 
  - System ról (Admin/User).
  - Mechanizm "First User is Admin" (automatyczne nadanie uprawnień administratora pierwszemu zarejestrowanemu użytkownikowi).
- **Bezpieczeństwo**: Hasła są hashowane przy użyciu algorytmu **BCrypt**, co uniemożliwia ich odczyt nawet w przypadku wycieku bazy danych.
- **Pełne CRUD**: Dodawanie, edycja, usuwanie oraz zmiana statusu zadań.
- **Baza Danych**: Wykorzystanie lekkiej bazy **SQLite** zarządzanej przez Entity Framework Core.
- **Dokumentacja API**: Zintegrowany **Swagger UI** do testowania punktów końcowych.

## 🛠️ Technologie

- **Backend**: .NET 8 / ASP.NET Core Web API
- **Baza danych**: SQLite + Entity Framework Core
- **Bezpieczeństwo**: JWT Bearer Authentication, BCrypt.Net-Next
- **Frontend**: HTML5, CSS3, JavaScript (Vanilla JS)
- **Dokumentacja**: Swagger / OpenAPI
"# ZadaniaApi" 
