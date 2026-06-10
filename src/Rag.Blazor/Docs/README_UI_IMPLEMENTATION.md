# Implementacja Autentykacji Google i Historii Konwersacji w UI Blazor

## Przegląd

Aplikacja Blazor została zaktualizowana o integrację z API używającym autentykacji Google JWT oraz zarządzanie historią konwersacji. Użytkownicy mogą teraz:

- Logować się przez Google Sign-In
- Przeglądać swoją historię konwersacji
- Wznawiać poprzednie konwersacje
- Usuwać konwersacje
- Automatycznie zapisywać nowe rozmowy

## Zmiany w Architekturze

### Nowe Modele

**Models/ConversationDto.cs** - DTO dla listy konwersacji
**Models/ConversationDetailDto.cs** - DTO dla szczegółów konwersacji z wiadomościami
**Models/CreateConversationDto.cs** - DTO dla tworzenia nowej konwersacji
**Models/UserInfo.cs** - Model informacji o użytkowniku Google
**Models/GoogleSettings.cs** - Konfiguracja Google Client ID

### Nowe Serwisy

**Services/AuthService.cs**

- Zarządza stanem autentykacji użytkownika
- Przechowuje Google ID Token w sessionStorage
- Udostępnia informacje o zalogowanym użytkowniku
- Event `OnAuthStateChanged` dla reaktywności

**Services/AuthenticatedHttpClient.cs**

- Wrapper dla HttpClient z automatycznym dodawaniem Bearer token
- Aktualizuje nagłówek Authorization przy zmianie stanu autentykacji
- Subskrybuje `AuthService.OnAuthStateChanged`

**Services/ChatApiClient.cs** (rozszerzony)

- `GetConversationsAsync()` - pobiera listę konwersacji użytkownika
- `GetConversationDetailAsync(conversationId)` - pobiera szczegóły konwersacji
- `CreateConversationAsync(title)` - tworzy nową konwersację
- `DeleteConversationAsync(conversationId)` - usuwa konwersację (soft delete)

### Zaktualizowane Komponenty

**State/ChatState.cs**

- Dodano `LoadConversation(ConversationDetailDto)` - ładuje konwersację z API
- Dodano `ClearConversation()` - czyści aktywną konwersację
- Event `OnConversationUpdated` - informuje o aktualizacji konwersacji
- Event `OnStateChanged` - informuje o zmianach stanu

**Components/Chat/ChatSidebar.razor**

- Przycisk Google Sign-In dla niezalogowanych użytkowników
- Wyświetlanie informacji o zalogowanym użytkowniku (avatar, nazwa, email)
- Lista konwersacji z możliwością przełączania
- Usuwanie konwersacji (z potwierdzeniem)
- Wylogowanie użytkownika
- Automatyczne odświeżanie listy po wysłaniu wiadomości

**Components/Chat/ChatContainer.razor**

- Wywołuje `ChatState.NotifyConversationUpdated()` po zakończeniu konwersacji
- Umożliwia automatyczne odświeżanie listy konwersacji w sidebar

**wwwroot/index.html**

- Dodano skrypt Google Sign-In
- Funkcje JavaScript do inicjalizacji Google Auth
- Dekodowanie JWT tokenu po stronie klienta
- Callback do Blazor po pomyślnym logowaniu

**Program.cs**

- Rejestracja `AuthService` jako Scoped
- Rejestracja `AuthenticatedHttpClient` jako Scoped
- Konfiguracja `GoogleSettings` z appsettings.json
- HttpClient z automatycznym nagłówkiem Authorization

## Konfiguracja

### 1. Google Cloud Console

1. Utwórz projekt w [Google Cloud Console](https://console.cloud.google.com/)
2. Włącz **Google Sign-In API**
3. Utwórz **OAuth 2.0 Client ID** (Web Application)
4. Dodaj autoryzowane źródła JavaScript:
   - `http://localhost:5000` (dla development)
   - Twoja domena produkcyjna
5. Skopiuj **Client ID**

### 2. appsettings.json

Zaktualizuj `wwwroot/appsettings.json`:

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
    "ClientId": "YOUR-GOOGLE-CLIENT-ID.apps.googleusercontent.com"
  }
}
```

### 3. appsettings.Development.json / appsettings.Production.json

Możesz również konfigurować różne Client ID dla różnych środowisk.

## Przepływ Autentykacji

### Logowanie

1. Użytkownik klika przycisk Google Sign-In w sidebar
2. Google wyświetla okno logowania
3. Po pomyślnym logowaniu JavaScript otrzymuje Google ID Token
4. Token jest dekodowany aby wyciągnąć informacje o użytkowniku (sub, email, name, picture)
5. `OnGoogleSignIn` wywołuje `AuthService.SignInAsync()`
6. Token jest zapisywany w sessionStorage
7. `OnAuthStateChanged` event jest wywoływany
8. ChatSidebar ładuje listę konwersacji z API
9. AuthenticatedHttpClient automatycznie dodaje token do wszystkich requestów

### Wylogowanie

1. Użytkownik klika przycisk wylogowania
2. `AuthService.SignOutAsync()` jest wywoływany
3. Token jest usuwany z sessionStorage
4. Google Sign-In jest wyłączany
5. `OnAuthStateChanged` event jest wywoływany
6. Lista konwersacji jest czyszczona
7. Aktualna konwersacja jest czyszczona

## Przepływ Konwersacji

### Nowa Konwersacja

1. Użytkownik klika "New chat"
2. `ChatState.ClearConversation()` czyści obecny stan
3. Użytkownik zadaje pytanie
4. StreamingClient wysyła request z `conversationId = null`
5. API tworzy nową konwersację i zwraca `conversationId` w meta event
6. `conversationId` jest zapisywany w `ChatState`
7. `OnConversationUpdated` event odświeża listę w sidebar

### Wznowienie Konwersacji

1. Użytkownik klika na konwersację w sidebar
2. `LoadConversation(conversationId)` jest wywoływany
3. `ChatApiClient.GetConversationDetailAsync()` pobiera szczegóły
4. `ChatState.LoadConversation()` ładuje wiadomości
5. UI wyświetla historię konwersacji
6. Użytkownik może kontynuować rozmowę

### Usuwanie Konwersacji

1. Użytkownik klika ikonę 🗑 na konwersacji
2. Wyświetlane jest okno potwierdzenia
3. `ChatApiClient.DeleteConversationAsync()` wysyła request
4. Konwersacja jest usuwana z listy (soft delete w bazie)
5. Jeśli usunięta konwersacja była aktywna, czat jest czyszczony

## Bezpieczeństwo

### Token Management

- Google ID Token jest przechowywany w sessionStorage (wygasa po zamknięciu karty)
- Token jest automatycznie dodawany do każdego requesta API przez AuthenticatedHttpClient
- Token wygasa po 1 godzinie - wymagane jest ponowne logowanie

### API Authorization

- Wszystkie endpointy API wymagają nagłówka `Authorization: Bearer <google-id-token>`
- Backend waliduje token bezpośrednio z Google
- Użytkownicy widzą tylko swoje konwersacje (filtrowane po userId w backend)

## Stylowanie CSS

Dodano nowe style w `wwwroot/css/chat.css`:

- `.auth-section` - sekcja logowania
- `.user-info` - informacje o użytkowniku
- `.user-avatar` - avatar użytkownika
- `.sign-out-btn` - przycisk wylogowania
- `.conversations-list` - lista konwersacji
- `.conversation-item` - pojedyncza konwersacja
- `.conversation-item.active` - aktywna konwersacja
- `.delete-conversation-btn` - przycisk usuwania

## Testowanie

### Uruchomienie Aplikacji

```bash
dotnet run
```

### Testowanie Logowania

1. Otwórz aplikację w przeglądarce
2. Kliknij przycisk Google Sign-In w sidebar
3. Zaloguj się kontem Google
4. Sprawdź czy wyświetla się avatar i dane użytkownika

### Testowanie Konwersacji

1. Po zalogowaniu kliknij "New chat"
2. Zadaj pytanie
3. Sprawdź czy konwersacja pojawia się w sidebar
4. Kliknij na konwersację aby ją wznowić
5. Usuń konwersację klikając ikonę 🗑

### Debug

Sprawdź konsolę przeglądarki (F12) aby zobaczyć:

- Logi Google Sign-In
- Dekodowany JWT token
- Błędy API requests
- SessionStorage entries

## Rozwiązywanie Problemów

### "Invalid Client ID"

- Sprawdź czy `Google:ClientId` w appsettings.json jest poprawny
- Sprawdź czy dodałeś `http://localhost:5000` do autoryzowanych źródeł w Google Cloud Console

### 401 Unauthorized

- Sprawdź czy backend API działa
- Sprawdź czy token nie wygasł (1 godzina)
- Wyloguj się i zaloguj ponownie
- Sprawdź konsolę przeglądarki - czy token jest wysyłany w nagłówku

### Lista konwersacji nie ładuje się

- Sprawdź konsolę przeglądarki
- Sprawdź czy backend API działa
- Sprawdź czy token jest prawidłowy
- Sprawdź Network tab w DevTools - status code requestu

### Google Sign-In nie działa

- Sprawdź czy skrypt Google został załadowany: `https://accounts.google.com/gsi/client`
- Sprawdź konsolę przeglądarki
- Sprawdź czy `initGoogleSignIn` jest wywoływany
- Sprawdź czy `#google-signin-button` istnieje w DOM

## Następne Kroki

### Możliwe Rozszerzenia

1. **Automatyczne odświeżanie tokenu** - implementacja refresh token logic
2. **Tytuły konwersacji** - automatyczne generowanie tytułów na podstawie pierwszego pytania
3. **Wyszukiwanie konwersacji** - dodanie search box w sidebar
4. **Sortowanie konwersacji** - sortowanie po dacie, alfabetycznie, itp.
5. **Eksport konwersacji** - możliwość eksportu do PDF/TXT
6. **Sharing** - udostępnianie konwersacji innym użytkownikom
7. **Tagi/Kategorie** - organizacja konwersacji w kategorie

### Optymalizacja

1. **Lazy loading** - ładowanie konwersacji w porcjach (pagination)
2. **Caching** - cache dla często używanych konwersacji
3. **Debouncing** - opóźnienie odświeżania listy
4. **Virtual scrolling** - dla dużej liczby konwersacji

## Struktura Plików

```
Rag.Blazor/
├── Components/
│   └── Chat/
│       ├── ChatContainer.razor (zaktualizowany)
│       └── ChatSidebar.razor (zaktualizowany)
├── Models/
│   ├── ConversationDto.cs (nowy)
│   ├── ConversationDetailDto.cs (nowy)
│   ├── CreateConversationDto.cs (nowy)
│   ├── UserInfo.cs (nowy)
│   └── GoogleSettings.cs (nowy)
├── Services/
│   ├── AuthService.cs (nowy)
│   ├── AuthenticatedHttpClient.cs (nowy)
│   └── ChatApiClient.cs (zaktualizowany)
├── State/
│   └── ChatState.cs (zaktualizowany)
├── wwwroot/
│   ├── appsettings.json (zaktualizowany)
│   ├── css/
│   │   └── chat.css (zaktualizowany)
│   └── index.html (zaktualizowany)
└── Program.cs (zaktualizowany)
```

## Podsumowanie

Implementacja dodaje pełną integrację z API wykorzystującym autentykację Google JWT oraz funkcjonalność zarządzania historią konwersacji. Aplikacja jest teraz w pełni funkcjonalna i gotowa do użycia z backendem opisanym w dokumentacji API.
