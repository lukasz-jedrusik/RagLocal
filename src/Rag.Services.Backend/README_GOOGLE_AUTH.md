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

## Jak uzyskać Google ID Token do testowania?

### Metoda 1: Google OAuth 2.0 Playground (Najłatwiejsza)

1. Otwórz [Google OAuth 2.0 Playground](https://developers.google.com/oauthplayground/)
2. Kliknij ikonę ustawień (⚙️) w prawym górnym rogu
3. Zaznacz **"Use your own OAuth credentials"**
4. Wklej swój **OAuth Client ID** i **OAuth Client Secret** (z Google Cloud Console)
5. W lewym panelu wybierz **"Google OAuth2 API v2"** → **"https://www.googleapis.com/auth/userinfo.email"**
6. Kliknij **"Authorize APIs"**
7. Zaloguj się swoim kontem Google
8. W kroku 2 kliknij **"Exchange authorization code for tokens"**
9. Skopiuj wartość **"id_token"** (to jest Twój Google ID Token)

**Ważne:** Token wygasa po 1 godzinie - musisz go odświeżyć klikając "Refresh access token".

### Metoda 2: Prosty plik HTML (dla Web Client ID)

Utwórz plik `test-google-auth.html`:

```html
<!DOCTYPE html>
<html>
  <head>
    <title>Get Google ID Token</title>
    <script src="https://accounts.google.com/gsi/client" async defer></script>
  </head>
  <body>
    <h1>Pobierz Google ID Token</h1>

    <div
      id="g_id_onload"
      data-client_id="TWOJ-CLIENT-ID.apps.googleusercontent.com"
      data-callback="handleCredentialResponse"
    ></div>
    <div class="g_id_signin" data-type="standard"></div>

    <h2>Twój Token:</h2>
    <textarea id="tokenOutput" rows="10" cols="80" readonly></textarea>

    <script>
      function handleCredentialResponse(response) {
        // response.credential to Google ID Token
        document.getElementById("tokenOutput").value = response.credential;
        console.log("ID Token:", response.credential);

        // Możesz też zdekodować token żeby zobaczyć zawartość
        const payload = JSON.parse(atob(response.credential.split(".")[1]));
        console.log("Token Payload:", payload);
      }
    </script>
  </body>
</html>
```

**Jak użyć:**

1. Zastąp `TWOJ-CLIENT-ID` swoim Client ID z Google Cloud Console
2. Otwórz plik w przeglądarce (możesz go otworzyć bezpośrednio z dysku)
3. Zaloguj się przez Google
4. Skopiuj token z textarea

**Uwaga:** Jeśli otwierasz lokalnie (file://), możesz dostać błąd CORS. Alternatywnie uruchom prosty serwer:

```bash
# Python 3
python -m http.server 8000

# Node.js (jeśli masz npx)
npx http-server
```

Potem otwórz `http://localhost:8000/test-google-auth.html`

### Metoda 3: Google Cloud SDK (gcloud)

Jeśli masz zainstalowany [Google Cloud SDK](https://cloud.google.com/sdk/docs/install):

```bash
# Zaloguj się
gcloud auth login

# Uzyskaj ID Token
gcloud auth print-identity-token
```

**Ważne:** To działa tylko jeśli Twoja aplikacja akceptuje tokeny z Google Cloud SDK (może wymagać dodatkowej konfiguracji Audience).

### Testowanie w Bruno/Postman

Po uzyskaniu tokenu:

**Bruno:**

```
POST http://localhost:5000/conversations
Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6...
Content-Type: application/json

{
  "title": "Test Conversation"
}
```

**cURL:**

```bash
curl -X POST "http://localhost:5000/conversations" \
  -H "Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6..." \
  -H "Content-Type: application/json" \
  -d '{"title":"Test Conversation"}'
```

### Sprawdzenie poprawności tokenu

Możesz sprawdzić zawartość tokenu na [jwt.io](https://jwt.io/):

1. Wklej token
2. Sprawdź:
   - **"iss"** powinno być `"https://accounts.google.com"` lub `"accounts.google.com"`
   - **"aud"** powinno odpowiadać Twojemu Client ID
   - **"exp"** (expiration) - sprawdź czy token nie wygasł
   - **"email"**, **"name"** - Twoje dane

### Troubleshooting uzyskiwania tokenu

**Błąd: "Invalid OAuth Client"**

- Sprawdź czy Client ID jest poprawny
- Upewnij się, że dodałeś `http://localhost:8000` (lub swoją domenę) do "Authorized JavaScript origins" w Google Cloud Console

**Błąd: "redirect_uri_mismatch"**

- Dodaj `https://developers.google.com/oauthplayground` do "Authorized redirect URIs" w Google Cloud Console

**Token nie działa w API**

- Sprawdź czy `Google:ClientId` w `appsettings.json` odpowiada Client ID użytemu do wygenerowania tokenu
- Sprawdź czy token nie wygasł (ważny 1 godzinę)
- Sprawdź czy używasz nagłówka: `Authorization: Bearer <token>` (z przedrostkiem "Bearer")

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

**Krok po kroku:**

1. Uruchom API: `dotnet run` (w folderze Rag.Services.Backend.Api)
2. Otwórz Swagger: `http://localhost:5000/swagger` (lub inny port z konsoli)
3. Uzyskaj Google ID Token (patrz sekcja "Jak uzyskać Google ID Token do testowania?")
4. Kliknij przycisk **🔓 Authorize** w prawym górnym rogu Swagger UI
5. W polu "Value" wklej **tylko token** (bez przedrostka "Bearer") - Swagger doda go automatycznie
6. Kliknij **Authorize**, potem **Close**
7. Teraz możesz testować endpointy - token będzie automatycznie dołączany do żądań

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
