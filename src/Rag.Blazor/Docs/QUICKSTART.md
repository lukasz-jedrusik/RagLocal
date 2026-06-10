# Szybki Start - Autentykacja Google i Historia Konwersacji

## Wymagane Kroki Konfiguracji

### 1. Skonfiguruj Google Cloud Console

1. Przejdź do [Google Cloud Console](https://console.cloud.google.com/)
2. Utwórz nowy projekt lub wybierz istniejący
3. Przejdź do **APIs & Services** > **Credentials**
4. Kliknij **Create Credentials** > **OAuth 2.0 Client ID**
5. Wybierz **Web application**
6. W sekcji **Authorized JavaScript origins** dodaj:
   ```
   http://localhost:5000
   https://localhost:5001
   ```
   (lub Twoje URL-e aplikacji)
7. Zapisz i skopiuj **Client ID** (np. `123456789.apps.googleusercontent.com`)

### 2. Zaktualizuj appsettings.json

W pliku `wwwroot/appsettings.json` zamień `YOUR-GOOGLE-CLIENT-ID` na skopiowany Client ID:

```json
{
  "Api": {
    "BaseUrl": "https://localhost:5001",
    "StreamEndpoint": "/ask/stream"
  },
  "UI": {
    "ScrollDelay": 10,
    "StreamingDelay": 30
  },
  "Google": {
    "ClientId": "123456789.apps.googleusercontent.com"
  }
}
```

### 3. Uruchom Backend API

Upewnij się, że backend API działa na porcie określonym w `Api.BaseUrl` (domyślnie `https://localhost:5001`).

Backend musi być skonfigurowany zgodnie z dokumentacją w `README_AUTH_HISTORY.md`.

### 4. Uruchom Aplikację Blazor

```bash
dotnet run
```

Aplikacja uruchomi się domyślnie na `http://localhost:5000`

### 5. Przetestuj Funkcjonalność

1. **Logowanie:**
   - Kliknij przycisk "Sign in with Google" w sidebar
   - Zaloguj się kontem Google
   - Powinieneś zobaczyć swój avatar i dane użytkownika

2. **Nowa Konwersacja:**
   - Kliknij "New chat"
   - Zadaj pytanie
   - Konwersacja pojawi się w sidebar po otrzymaniu odpowiedzi

3. **Wznowienie Konwersacji:**
   - Kliknij na konwersację w sidebar
   - Historia wiadomości zostanie załadowana

4. **Usuwanie Konwersacji:**
   - Najedź myszką na konwersację
   - Kliknij ikonę 🗑
   - Potwierdź usunięcie

5. **Wylogowanie:**
   - Kliknij przycisk ⏻ obok danych użytkownika
   - Zostaniesz wylogowany i historia zostanie wyczyszczona

## Częste Problemy

### Nie widzę przycisku Google Sign-In

- Sprawdź konsolę przeglądarki (F12)
- Upewnij się, że skrypt Google został załadowany
- Sprawdź czy Google Client ID jest ustawiony w appsettings.json

### 401 Unauthorized przy requestach API

- Upewnij się, że backend API jest uruchomiony
- Sprawdź czy Google Client ID w frontend i backend jest taki sam
- Wyloguj się i zaloguj ponownie (token mógł wygasnąć po 1 godzinie)

### Lista konwersacji nie ładuje się

- Sprawdź konsolę przeglądarki
- Sprawdź czy endpoint `/conversations` działa w backend API
- Sprawdź czy jesteś zalogowany

### CORS errors

- Upewnij się, że backend API ma skonfigurowany CORS dla `http://localhost:5000`
- Sprawdź czy backend API działa na właściwym porcie

## Struktura UI

```
┌─────────────────────────────────────────┐
│ Sidebar                                 │
│ ┌─────────────────────────────────────┐ │
│ │ [Logo] Local Knowledge Assistant    │ │
│ ├─────────────────────────────────────┤ │
│ │ [Avatar] Jan Kowalski               │ │
│ │          jan@example.com        [⏻] │ │
│ ├─────────────────────────────────────┤ │
│ │ [✚ New chat]                        │ │
│ ├─────────────────────────────────────┤ │
│ │ Conversations:                      │ │
│ │ ┌─────────────────────────────────┐ │ │
│ │ │ My Conversation        [🗑]     │ │ │
│ │ │ 2h ago • 5 msg                  │ │ │
│ │ └─────────────────────────────────┘ │ │
│ │ ┌─────────────────────────────────┐ │ │
│ │ │ Another Chat           [🗑]     │ │ │
│ │ │ 1d ago • 12 msg                 │ │ │
│ │ └─────────────────────────────────┘ │ │
│ └─────────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

## API Endpoints Używane

- `GET /conversations` - Lista konwersacji użytkownika
- `GET /conversations/{id}` - Szczegóły konwersacji
- `POST /conversations` - Utworzenie nowej konwersacji
- `DELETE /conversations/{id}` - Usunięcie konwersacji
- `POST /ask/stream` - Wysłanie pytania (z conversationId)

Wszystkie endpointy wymagają nagłówka:

```
Authorization: Bearer <google-id-token>
```

## Następne Kroki

Po pomyślnym uruchomieniu możesz:

1. Dostosować URL API w `appsettings.json` dla środowiska produkcyjnego
2. Zmienić Client ID dla produkcji w Google Cloud Console
3. Dodać własne style CSS w `wwwroot/css/chat.css`
4. Rozszerzyć funkcjonalność zgodnie z sugestiami w `README_UI_IMPLEMENTATION.md`

## Pomoc

Jeśli napotkasz problemy, sprawdź:

- `README_UI_IMPLEMENTATION.md` - szczegółowa dokumentacja implementacji
- `README_AUTH_HISTORY.md` - dokumentacja API backend
- `README_GOOGLE_AUTH.md` - dokumentacja autentykacji Google

W razie pytań, sprawdź konsolę przeglądarki (F12) i logi backendu.
