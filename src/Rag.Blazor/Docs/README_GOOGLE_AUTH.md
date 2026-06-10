# API - Autentykacja Google JWT i Historia Rozmów

## Przegląd

API wykorzystuje **Google ID Token** do autentykacji użytkowników. Nie ma dedykowanego endpointu `/auth/google` - walidacja tokenu odbywa się bezpośrednio w middleware przy każdym żądaniu.

## Jak to działa?

1. **Frontend** uzyskuje Google ID Token od Google (przez Google Sign-In)
2. **Frontend** wysyła żądania do API z nagłówkiem: `Authorization: Bearer <google-id-token>`
3. **Middleware** (AddJwtBearer) waliduje token bezpośrednio z Google
4. **Przy pierwszym logowaniu** użytkownik jest automatycznie tworzony w bazie SQL Server
5. **User ID** jest dodawany do claimów i dostępny we wszystkich endpointach

## Konfiguracja

### 1. appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RagBackendDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;"
  },
  "Google": {
    "ClientId": "YOUR-GOOGLE-CLIENT-ID.apps.googleusercontent.com"
  }
}
```

### 2. Google Cloud Console

1. Utwórz projekt w [Google Cloud Console](https://console.cloud.google.com/)
2. Włącz **Google Sign-In API**
3. Utwórz **OAuth 2.0 Client ID** (Web Application)
4. Dodaj swoje domeny do **Authorized JavaScript origins**
5. Skopiuj **Client ID** do `appsettings.json`

### 3. Baza danych

```bash
cd Rag.Services.Backend.Infrastructure
dotnet ef database update --context DataContext --startup-project ..\Rag.Services.Backend.Api\Rag.Services.Backend.Api.csproj
```

## Użycie w Aplikacji

### JavaScript/TypeScript (Frontend)

```javascript
// 1. Inicjalizacja Google Sign-In
gapi.load("auth2", function () {
  gapi.auth2.init({
    client_id: "YOUR-CLIENT-ID.apps.googleusercontent.com",
  });
});

// 2. Logowanie użytkownika
const auth2 = gapi.auth2.getAuthInstance();
const googleUser = await auth2.signIn();
const idToken = googleUser.getAuthResponse().id_token;

// 3. Wywołanie API z tokenem
const response = await fetch("https://api.example.com/ask", {
  method: "POST",
  headers: {
    Authorization: `Bearer ${idToken}`,
    "Content-Type": "application/json",
  },
  body: JSON.stringify({
    question: "My question",
    conversationId: "",
  }),
});
```

### Blazor WebAssembly

```csharp
@inject HttpClient Http
@inject IJSRuntime JS

// 1. Uzyskaj token od Google (przez JSInterop)
var idToken = await JS.InvokeAsync<string>("getGoogleIdToken");

// 2. Ustaw nagłówek autoryzacji
Http.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", idToken);

// 3. Wywołaj API
var conversations = await Http.GetFromJsonAsync<List<ConversationDto>>("/conversations");
```

## Endpointy API

Wszystkie chronione endpointy wymagają nagłówka:

```
Authorization: Bearer <google-id-token>
```

### Konwersacje

**GET /conversations**

- Pobiera wszystkie konwersacje użytkownika

**GET /conversations/{id}**

- Pobiera szczegóły konwersacji z pełną historią wiadomości

**POST /conversations**

- Tworzy nową konwersację

```json
{
  "title": "My Conversation"
}
```

**DELETE /conversations/{id}**

- Usuwa konwersację (soft delete)

### Pytania

**POST /ask**

- Zadaje pytanie AI (wymagana autoryzacja)

```json
{
  "question": "What is...?",
  "conversationId": "optional-guid"
}
```

**POST /ask/stream**

- Streaming odpowiedzi AI (wymagana autoryzacja)

## Architektura

### Extension/GmailAuth/Extension.cs

Konfiguruje autentykację Google JWT:

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://accounts.google.com";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidAudience = configuration["Google:ClientId"],
            ValidIssuers = new[] { "https://accounts.google.com", "accounts.google.com" }
        };

        // Event handler - tworzy użytkownika przy pierwszym logowaniu
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                // Pobierz dane z tokenu
                var googleId = context.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = context.Principal.FindFirst(ClaimTypes.Email)?.Value;

                // Znajdź lub utwórz użytkownika w DB
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
                if (user == null)
                {
                    user = new User { GoogleId = googleId, Email = email, ... };
                    dbContext.Users.Add(user);
                    await dbContext.SaveChangesAsync();
                }

                // Dodaj user_id do claimów
                context.Principal.AddIdentity(new ClaimsIdentity(new[]
                {
                    new Claim("user_id", user.Id.ToString())
                }));
            }
        };
    });
```

### DataContext

Używany zamiast ApplicationDbContext. Zawiera tabele:

- `Users` - użytkownicy Google
- `Conversations` - konwersacje użytkowników
- `Messages` - wiadomości w konwersacjach

## Bezpieczeństwo

✅ **Google JWT Validation** - tokeny walidowane bezpośrednio z Google  
✅ **HTTPS Required** - RequireHttpsMetadata = true w produkcji  
✅ **Token Expiration** - Google ID Token wygasa po 1 godzinie  
✅ **User Isolation** - użytkownicy widzą tylko swoje konwersacje  
✅ **Automatic User Creation** - użytkownicy tworzeni automatycznie  
✅ **SQL Injection Protection** - EF Core z parametryzowanymi zapytaniami

## Troubleshooting

**"User not authenticated"**

- Sprawdź czy token Google jest ważny (nie wygasł)
- Sprawdź nagłówek: `Authorization: Bearer <token>`
- Sprawdź ClientId w appsettings.json

**"Invalid token"**

- Upewnij się, że ClientId w appsettings.json jest poprawny
- Token musi pochodzić z poprawnego projektu Google

**Błąd bazy danych**

- Sprawdź ConnectionString
- Uruchom: `dotnet ef database update --context DataContext`

## Rozwój

### Dodanie nowej migracji

```bash
cd Rag.Services.Backend.Infrastructure
dotnet ef migrations add MigrationName --context DataContext --startup-project ..\Rag.Services.Backend.Api\Rag.Services.Backend.Api.csproj
dotnet ef database update --context DataContext --startup-project ..\Rag.Services.Backend.Api\Rag.Services.Backend.Api.csproj
```

### Testowanie w Swagger

W Swagger UI kliknij **Authorize** i wklej Google ID Token uzyskany z frontendu.

## Różnice względem poprzedniej implementacji

❌ **Usunięto:**

- Endpoint `/auth/google`
- GoogleAuthService
- JwtService
- JwtMiddleware
- ApplicationDbContext

✅ **Dodano:**

- GmailAuth/Extension.cs (AddJwtBearer)
- OnTokenValidated event handler
- DataContext (zamiast ApplicationDbContext)
- Automatyczne tworzenie użytkowników
