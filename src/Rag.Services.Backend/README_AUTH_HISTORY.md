# Nowe funkcjonalności API - Autentykacja i Historia Rozmów

## Dodane Funkcjonalności

### 1. Autentykacja Google z JWT

- Autentykacja użytkowników za pomocą Google ID Token (bezpośrednio jako Bearer token)
- Automatyczne tworzenie i aktualizacja użytkownika w bazie danych
- Walidacja tokenów Google po stronie serwera
- Autoryzacja wszystkich endpointów za pomocą JWT Bearer Authentication

### 2. Historia Rozmów w SQL Server

- Zapisywanie wszystkich rozmów użytkowników w bazie danych
- Możliwość listowania, odtwarzania i wznawiania rozmów
- Automatyczne przypisywanie wiadomości do konwersacji
- Soft delete dla rozmów (nie są fizycznie usuwane)

## Konfiguracja

### 1. Ustawienia w appsettings.json

Zaktualizuj plik `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RagBackendDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true;"
  },
  "Google": {
    "ClientId": "your-google-client-id.apps.googleusercontent.com"
  }
}
```

**Uwaga:** Konfiguracja `Jwt` (SecretKey, Issuer, Audience) nie jest już używana - system waliduje tokeny bezpośrednio z Google.

### 2. Konfiguracja Google OAuth

1. Przejdź do [Google Cloud Console](https://console.cloud.google.com/)
2. Utwórz nowy projekt lub wybierz istniejący
3. Włącz Google Identity API
4. Utwórz OAuth 2.0 Client ID (Web Application)
5. Dodaj autoryzowane źródła JavaScript (np. `http://localhost:3000`)
6. Skopiuj Client ID do `appsettings.json`

### 3. Migracja Bazy Danych

Zastosuj migrację do bazy danych:

```bash
cd Rag.Services.Backend.Infrastructure
dotnet ef database update --context DataContext --startup-project ..\Rag.Services.Backend.Api\Rag.Services.Backend.Api.csproj
```

**Utworzenie nowej migracji (jeśli potrzebna):**

```bash
cd Rag.Services.Backend.Infrastructure
dotnet ef migrations add NazwaMigracji --context DataContext --startup-project ..\Rag.Services.Backend.Api\Rag.Services.Backend.Api.csproj
```

## Endpointy API

### Autentykacja

System używa **bezpośrednio Google ID Token** jako Bearer token. Nie ma dedykowanego endpointu `/auth/google`.

**Mechanizm działania:**

1. Klient otrzymuje Google ID Token od Google (np. za pomocą Google Sign-In dla Web)
2. Klient wysyła ten token w nagłówku `Authorization: Bearer <google-id-token>` przy każdym żądaniu
3. Serwer waliduje token z Google
4. Przy pierwszym logowaniu użytkownik jest automatycznie tworzony w bazie danych
5. Przy kolejnych logowaniach aktualizowana jest data ostatniego logowania

**Wymagany nagłówek dla wszystkich chronionych endpointów:**

```
Authorization: Bearer <google-id-token>
```

**Ważność tokenu:** Google ID Token wygasa po 1 godzinie - klient musi go regularnie odświeżać.

### Konwersacje

Wszystkie poniższe endpointy wymagają nagłówka autoryzacji z Google ID Token.

#### GET /conversations

Pobierz wszystkie konwersacje użytkownika.

**Response:**

```json
[
  {
    "conversationId": "guid",
    "title": "My Conversation",
    "createdAt": "2024-01-01T10:00:00Z",
    "updatedAt": "2024-01-01T11:00:00Z",
    "messageCount": 10
  }
]
```

#### GET /conversations/{conversationId}

Pobierz szczegóły konwersacji wraz z wszystkimi wiadomościami.

**Response:**

```json
{
  "conversationId": "guid",
  "title": "My Conversation",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-01T11:00:00Z",
  "messages": [
    {
      "role": "user",
      "content": "Question?",
      "createdAt": "2024-01-01T10:00:00Z"
    },
    {
      "role": "assistant",
      "content": "Answer...",
      "createdAt": "2024-01-01T10:00:05Z"
    }
  ]
}
```

#### POST /conversations

Utwórz nową konwersację.

**Request:**

```json
{
  "title": "My New Conversation"
}
```

**Response:**

```json
{
  "conversationId": "guid",
  "title": "My New Conversation",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-01T10:00:00Z",
  "messageCount": 0
}
```

#### DELETE /conversations/{conversationId}

Usuń konwersację (soft delete).

**Response:** 204 No Content

### Pytania

#### POST /ask

Zadaj pytanie i otrzymaj odpowiedź AI.

**Request:**

```json
{
  "question": "Jakie jest pytanie?",
  "conversationId": "guid-konwersacji-lub-pusty-string"
}
```

**Response:**

```json
{
  "answer": "Odpowiedź AI...",
  "conversationId": "guid",
  "citations": [...]
}
```

**Uwagi:**

- Wymaga autoryzacji (Google ID Token w nagłówku Authorization)
- Automatycznie zapisuje pytanie i odpowiedź do bazy danych
- Jeśli `conversationId` jest pusty lub nieprawidłowy, tworzona jest nowa konwersacja

#### POST /ask/stream

Zadaj pytanie i otrzymaj odpowiedź AI w strumieniu (Server-Sent Events).

**Request:**

```json
{
  "question": "Jakie jest pytanie?",
  "conversationId": "guid-konwersacji-lub-pusty-string"
}
```

**Response:** Stream typu `text/event-stream` z eventami SSE

**Uwagi:**

- Wymaga autoryzacji (Google ID Token w nagłówku Authorization)
- Automatycznie zapisuje pytanie i odpowiedź do bazy danych
- Odpowiedź jest streamowana w czasie rzeczywistym

## Architektura

### Warstwy

**Domain Layer:**

- `User` - Model użytkownika (GoogleId, Email, Name, PictureUrl)
- `Conversation` - Model konwersacji (ConversationId, Title, UserId, IsActive)
- `Message` - Model wiadomości (Role, Content, ConversationId)

**Application Layer:**

- DTOs: `ConversationDto`, `ConversationDetailDto`, `MessageDto`, `AskRequestDto`, `AskResponseDto`
- Commands: `CreateConversationCommand`, `DeleteConversationCommand`
- Queries: `GetConversationsQuery`, `GetConversationDetailQuery`, `AskQuestionQuery`, `AskQuestionStreamQuery`
- Interfejsy: `IIdentityService`, `IConversationRepository`, `IMessageRepository`, `IUserRepository`

**Infrastructure Layer:**

- `DataContext` - DbContext EF Core
- Repozytoria: `UserRepository`, `ConversationRepository`, `MessageRepository`
- Serwisy: `IdentityService` - wyciąga userId z kontekstu HTTP
- Extensions: `GmailAuth/Extension.cs` - konfiguracja JWT Bearer Authentication z Google

**API Layer:**

- JWT Bearer Authentication middleware (wbudowany w ASP.NET Core)
- Automatyczne tworzenie użytkownika w `OnTokenValidated` event
- Endpointy: `ConversationEndpoints`, `AskEndpoints` (z automatycznym zapisem do bazy)

## Integracja z Aplikacją Frontendową

### Przepływ autentykacji

1. **Logowanie użytkownika przez Google:**
   - Użyj Google Sign-In dla Web (JavaScript)
   - Biblioteka: [Google Identity Services](https://developers.google.com/identity/gsi/web)
2. **Otrzymanie Google ID Token:**
   - Po pomyślnym logowaniu otrzymasz `credential` (Google ID Token)
3. **Wysyłanie żądań do API:**
   - Dołącz token do nagłówka `Authorization: Bearer <google-id-token>`
   - Token jest automatycznie walidowany przez backend
   - Użytkownik jest automatycznie tworzony/aktualizowany w bazie

### Przykład użycia w aplikacji Blazor/C#

```csharp
public class ApiService
{
    private readonly HttpClient _httpClient;
    private string _googleIdToken;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Ustaw token otrzymany z Google Sign-In
    public void SetGoogleToken(string googleIdToken)
    {
        _googleIdToken = googleIdToken;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", googleIdToken);
    }

    // Pobieranie wszystkich konwersacji
    public async Task<List<ConversationDto>> GetConversationsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<ConversationDto>>("/conversations");
    }

    // Pobieranie szczegółów konwersacji
    public async Task<ConversationDetailDto> GetConversationDetailAsync(string conversationId)
    {
        return await _httpClient
            .GetFromJsonAsync<ConversationDetailDto>($"/conversations/{conversationId}");
    }

    // Tworzenie nowej konwersacji
    public async Task<ConversationDto> CreateConversationAsync(string title)
    {
        var request = new CreateConversationDto { Title = title };
        var response = await _httpClient.PostAsJsonAsync("/conversations", request);
        return await response.Content.ReadFromJsonAsync<ConversationDto>();
    }

    // Zadanie pytania
    public async Task<AskResponseDto> AskQuestionAsync(string question, string conversationId = "")
    {
        var request = new AskRequestDto
        {
            Question = question,
            ConversationId = conversationId
        };
        var response = await _httpClient.PostAsJsonAsync("/ask", request);
        return await response.Content.ReadFromJsonAsync<AskResponseDto>();
    }
}
```

### Przykład HTML/JavaScript z Google Sign-In

```html
<!DOCTYPE html>
<html>
  <head>
    <script src="https://accounts.google.com/gsi/client" async defer></script>
  </head>
  <body>
    <div
      id="g_id_onload"
      data-client_id="YOUR_GOOGLE_CLIENT_ID"
      data-callback="handleCredentialResponse"
    ></div>
    <div class="g_id_signin" data-type="standard"></div>

    <script>
      function handleCredentialResponse(response) {
        // response.credential zawiera Google ID Token
        const googleIdToken = response.credential;

        // Użyj tego tokenu do wywołań API
        fetch("https://your-api.com/conversations", {
          headers: {
            Authorization: `Bearer ${googleIdToken}`,
          },
        })
          .then((res) => res.json())
          .then((data) => console.log(data));
      }
    </script>
  </body>
</html>
```

### Odświeżanie tokenu

**Ważne:** Google ID Token wygasa po **1 godzinie**.

Aby uniknąć błędów autoryzacji:

1. **Automatyczne odświeżanie:** Użyj Google Identity Services, które automatycznie odświeżają token
2. **Obsługa błędów 401:** Wykrywaj błędy 401 Unauthorized i ponownie loguj użytkownika
3. **Proaktywne odświeżanie:** Sprawdzaj ważność tokenu przed wysłaniem żądania

```javascript
// Przykład z obsługą wygasłego tokenu
async function callApiWithTokenRefresh(url) {
  try {
    const response = await fetch(url, {
      headers: { Authorization: `Bearer ${googleIdToken}` },
    });

    if (response.status === 401) {
      // Token wygasł - poproś o nowy
      await refreshGoogleToken();
      // Ponów żądanie z nowym tokenem
      return await fetch(url, {
        headers: { Authorization: `Bearer ${googleIdToken}` },
      });
    }

    return response;
  } catch (error) {
    console.error("API call failed:", error);
  }
}
```

## Bezpieczeństwo

1. **Google Token Validation** - Tokeny są walidowane bezpośrednio z serwerami Google (Authority: `https://accounts.google.com`)
2. **HTTPS** - Wymagane w produkcji (`RequireHttpsMetadata = true`)
3. **Token Expiration** - Google ID Token wygasa po 1 godzinie (zarządzane przez Google)
4. **SQL Injection** - Używamy EF Core z parametryzowanymi zapytaniami
5. **User Isolation** - Każdy użytkownik ma dostęp tylko do swoich konwersacji (walidacja `userId` w query handlers)
6. **Audience Validation** - Token musi być wystawiony dla Twojego Google Client ID
7. **Clock Skew** - Ustawiony na zero dla precyzyjnej walidacji czasu

### Konfiguracja dla produkcji

W `appsettings.json` dla produkcji:

```json
{
  "Google": {
    "ClientId": "twoj-google-client-id.apps.googleusercontent.com"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=production-server;Database=RagBackendDb;..."
  }
}
```

**Ważne dla produkcji:**

- Ustaw silne hasło do bazy danych
- Użyj HTTPS dla API
- Skonfiguruj CORS tylko dla zaufanych domen
- Regularnie aktualizuj biblioteki NuGet

## Rozwój

### Dodanie nowej migracji

Po zmianach w modelach domenowych:

```bash
cd Rag.Services.Backend.Infrastructure
dotnet ef migrations add NazwaMigracji --context DataContext --startup-project ..\Rag.Services.Backend.Api\Rag.Services.Backend.Api.csproj
dotnet ef database update --context DataContext --startup-project ..\Rag.Services.Backend.Api\Rag.Services.Backend.Api.csproj
```

### Testowanie z Swagger

1. Uruchom aplikację w trybie Development
2. Otwórz Swagger UI: `http://localhost:{port}/swagger`
3. Uzyskaj Google ID Token (patrz sekcja poniżej)
4. W Swagger UI kliknij przycisk "Authorize"
5. Wpisz **tylko token** (bez przedrostka "Bearer") - Swagger doda go automatycznie
6. Testuj endpointy

### Jak uzyskać Google ID Token do testowania?

#### Metoda 1: Google OAuth 2.0 Playground (Najłatwiejsza)

1. Otwórz [Google OAuth 2.0 Playground](https://developers.google.com/oauthplayground/)
2. Kliknij ikonę ustawień (⚙️) w prawym górnym rogu
3. Zaznacz **"Use your own OAuth credentials"**
4. Wklej swój **OAuth Client ID** i **OAuth Client Secret** (z Google Cloud Console)
5. W lewym panelu wybierz **"Google OAuth2 API v2"** → **"https://www.googleapis.com/auth/userinfo.email"**
6. Kliknij **"Authorize APIs"** i zaloguj się
7. W kroku 2 kliknij **"Exchange authorization code for tokens"**
8. Skopiuj wartość **"id_token"**

**Uwaga:** Token wygasa po 1 godzinie - odśwież klikając "Refresh access token".

**Jeśli potrzebujesz dodać redirect URI:**

- W Google Cloud Console → Twoja aplikacja → "Authorized redirect URIs"
- Dodaj: `https://developers.google.com/oauthplayground`

#### Metoda 2: Prosty plik HTML

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
        document.getElementById("tokenOutput").value = response.credential;
        console.log("ID Token:", response.credential);
      }
    </script>
  </body>
</html>
```

**Jak użyć:**

1. Zastąp `TWOJ-CLIENT-ID` swoim Client ID
2. Uruchom lokalny serwer: `python -m http.server 8000` lub `npx http-server`
3. Otwórz `http://localhost:8000/test-google-auth.html`
4. Zaloguj się i skopiuj token

**Uwaga:** Dodaj `http://localhost:8000` do "Authorized JavaScript origins" w Google Cloud Console.

#### Metoda 3: Google Cloud SDK

```bash
gcloud auth login
gcloud auth print-identity-token
```

### Testowanie z cURL / Bruno / Postman

```bash
# Pobierz konwersacje
curl -X GET "http://localhost:5000/conversations" \
  -H "Authorization: Bearer YOUR_GOOGLE_ID_TOKEN"

# Utwórz nową konwersację
curl -X POST "http://localhost:5000/conversations" \
  -H "Authorization: Bearer YOUR_GOOGLE_ID_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"Moja konwersacja"}'

# Zadaj pytanie
curl -X POST "http://localhost:5000/ask" \
  -H "Authorization: Bearer YOUR_GOOGLE_ID_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"question":"Jak działa AI?","conversationId":"guid-lub-pusty"}'
```

**W Bruno/Postman:**

- Ustaw nagłówek: `Authorization: Bearer <token>`
- Content-Type: `application/json`

### Weryfikacja tokenu

Sprawdź zawartość tokenu na [jwt.io](https://jwt.io/):

- **"iss"** powinno być `"https://accounts.google.com"`
- **"aud"** powinno odpowiadać Twojemu Client ID z `appsettings.json`
- **"exp"** (expiration) - sprawdź czy nie wygasł
- **"email"**, **"name"** - Twoje dane użytkownika

## Troubleshooting

### Problem: 401 Unauthorized - "User not authenticated"

**Możliwe przyczyny:**

1. **Brak tokenu w nagłówku:**
   - Upewnij się, że wysyłasz nagłówek: `Authorization: Bearer <google-id-token>`

2. **Token wygasł:**
   - Google ID Token jest ważny przez 1 godzinę
   - Odśwież token używając Google Sign-In

3. **Nieprawidłowy Google Client ID:**
   - Sprawdź czy `Google:ClientId` w `appsettings.json` odpowiada Client ID z Google Cloud Console
   - Token musi być wystawiony dla tego samego Client ID

4. **Token nie jest od Google:**
   - Upewnij się, że używasz Google ID Token (z `accounts.google.com`)
   - Nie używaj tokenów z innych dostawców OAuth

**Diagnostyka:**

```bash
# Sprawdź zawartość tokenu (bez wysyłania do API)
# Wklej token na: https://jwt.io/
# Sprawdź:
# - "iss": powinno być "https://accounts.google.com" lub "accounts.google.com"
# - "aud": powinno odpowiadać Twojemu Google Client ID
# - "exp": sprawdź czy token nie wygasł
```

### Problem: "Conversation not found" (404)

**Możliwe przyczyny:**

1. **Nieprawidłowy conversationId:**
   - Sprawdź czy conversationId istnieje (użyj GET `/conversations`)
   - ConversationId musi być prawidłowym GUID

2. **Brak dostępu do konwersacji:**
   - Konwersacja należy do innego użytkownika
   - System automatycznie filtruje konwersacje po userId

3. **Konwersacja usunięta (soft delete):**
   - Sprawdź czy `isActive = false` w bazie danych
   - Usunięte konwersacje nie są zwracane przez API

### Problem: Błąd połączenia z bazą danych

**Możliwe przyczyny:**

1. **SQL Server nie działa:**

   ```bash
   # Windows - sprawdź status usługi
   sc query MSSQLSERVER
   # Lub MSSQL$SQLEXPRESS dla SQL Server Express
   ```

2. **Nieprawidłowy ConnectionString:**
   - Sprawdź `appsettings.json` → `ConnectionStrings:DefaultConnection`
   - Format: `Server=localhost;Database=RagBackendDb;User Id=sa;Password=...;TrustServerCertificate=True;MultipleActiveResultSets=true;`

3. **Baza danych nie istnieje:**

   ```bash
   cd Rag.Services.Backend.Infrastructure
   dotnet ef database update --context DataContext --startup-project ..\Rag.Services.Backend.Api\Rag.Services.Backend.Api.csproj
   ```

4. **Problemy z migracjami:**

   ```bash
   # Sprawdź jakie migracje są zastosowane
   dotnet ef migrations list --context DataContext --startup-project ..\Rag.Services.Backend.Api\Rag.Services.Backend.Api.csproj

   # Jeśli potrzeba, usuń bazę i utwórz od nowa
   dotnet ef database drop --context DataContext --startup-project ..\Rag.Services.Backend.Api\Rag.Services.Backend.Api.csproj
   dotnet ef database update --context DataContext --startup-project ..\Rag.Services.Backend.Api\Rag.Services.Backend.Api.csproj
   ```

### Problem: CORS - Żądania blokowane w przeglądarce

**Objaw:** Błąd w konsoli przeglądarki: "Access to fetch at ... has been blocked by CORS policy"

**Rozwiązanie:**

W `Program.cs` jest już skonfigurowana polityka CORS, która pozwala na wszystkie źródła (dla development):

```csharp
.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
})
```

**Dla produkcji** ogranicz dozwolone źródła:

```csharp
.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://twoja-aplikacja.com")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
})
```

### Problem: Automatyczne tworzenie użytkownika nie działa

**Diagnostyka:**

1. **Sprawdź logi aplikacji** - powinny zawierać informacje o walidacji tokenu
2. **Sprawdź czy context DbContext jest prawidłowo wstrzyknięty** w `OnTokenValidated` event
3. **Sprawdź strukturę tabeli Users** - musi być zgodna z modelem `User`

```sql
-- Sprawdź użytkowników w bazie
SELECT * FROM Users;

-- Sprawdź czy tabela istnieje
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users';
```
