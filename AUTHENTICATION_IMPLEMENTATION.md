# Sistemul de Autentificare JobFinder - Documentație

## Descrierea Implementării

Sistemul de autentificare a fost implementat conform Identity Framework în .NET 9 cu o arhitectură CQRS (Command Query Responsibility Segregation) utilizând MediatR.

## Componente Implementate

### 1. **DTOs (Data Transfer Objects)**
- **RegisterDto** (`JobFinder.Shared/DTOs/Identity/RegisterDto.cs`)
  - Email, Password, ConfirmPassword, FullName, UserType
  - Validări: email obligatoriu, parolă >= 6 caractere

- **LoginDto** (existent)
  - Email și Password

- **AuthResponseDto** (existent)
  - UserId, Email, UserType, Token, ExpiresAt

### 2. **Backend - API Endpoints**

#### RegisterCandidate / RegisterEmployer
**POST** `/api/auth/register`
```json
{
  "email": "user@example.com",
  "password": "securePassword123",
  "confirmPassword": "securePassword123",
  "fullName": "John Doe",
  "userType": 0 // 0 = Candidate, 1 = Employer, 2 = Admin
}
```

**Response (Succes):**
```json
{
  "userId": "user-id-uuid",
  "email": "user@example.com",
  "userType": 0,
  "token": "jwt-token-here",
  "expiresAt": "2024-12-21T10:30:00Z"
}
```

#### Login
**POST** `/api/auth/login`
```json
{
  "email": "user@example.com",
  "password": "securePassword123"
}
```

### 3. **Command Handler (CQRS)**
**RegisterCommandHandler** (`JobFinder.UseCases/Features/Identity/Commands/Register/`)
- Validează input-ul (parolele se potrivesc)
- Verifica dacă utilizatorul există deja
- Creează utilizator nou prin UserManager
- Returnează rezultat cu succes/eroare

### 4. **Frontend - Pagini Blazor WebAssembly**

#### RegisterCandidate.razor (`/register/candidate`)
- Form de înregistrare pentru candidați
- Valida client-side
- Apelează endpoint-ul `/api/auth/register` cu `UserType.Candidate`
- Redirecționează la `/candidate/dashboard` după succes

#### RegisterEmployer.razor (`/register/employer`)
- Form de înregistrare pentru angajatori
- Valida client-side
- Apelează endpoint-ul `/api/auth/register` cu `UserType.Employer`
- Redirecționează la `/employer/dashboard` după succes

#### Login.razor (`/login`)
- Form de autentificare
- Apelează endpoint-ul `/api/auth/login`
- Stochează token-ul (localStorage)
- Redirecționează în funcție de UserType:
  - Candidate -> `/candidate/dashboard`
  - Employer -> `/employer/dashboard`
  - Admin -> `/admin/dashboard`

### 5. **Configurare JWT**

**appsettings.json:**
```json
{
  "Jwt": {
    "Key": "your-super-secret-key-that-is-at-least-32-characters-long-for-security",
    "Issuer": "JobFinder",
    "Audience": "JobFinderClient"
  }
}
```

### 6. **Configurare Identity**

**Program.cs:**
```csharp
// Identity configuration
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();
```

## Fluxul de Utilizare

### Înregistrare
1. Utilizatorul accesează `/register/candidate` sau `/register/employer`
2. Completează formularul (email, parolă, nume/companie)
3. Face clic pe "Creează cont"
4. Frontend validează datele local
5. Se apelează `POST /api/auth/register`
6. Backend creează utilizatorul via Identity UserManager
7. Se generează JWT token
8. Frontend primește token și date utilizator
9. Utilizatorul este redirecționat la dashboard-ul corespunzător

### Autentificare
1. Utilizatorul accesează `/login`
2. Completează email și parolă
3. Face clic pe "Conectare"
4. Se apelează `POST /api/auth/login`
5. Backend verifică credențialele
6. Se generează JWT token
7. Frontend primește token și redirecționează utilizatorul

## Securitate

- ✅ Parolele sunt hash-ate cu Identity password hasher
- ✅ JWT tokens sunt generați cu HMAC SHA256
- ✅ Validări client-side și server-side
- ✅ Email unic pentru fiecare utilizator
- ⚠️ Jwt:Key trebuie schimbat în producție cu o cheie sigură

## Siguințe Viitoare

1. **Confirmarea Email-ului**: Adăugare verificare email înainte de login
2. **Two-Factor Authentication**: Implementare 2FA optional
3. **Token Refresh**: Implementare refresh token mechanism
4. **Password Reset**: Funcționalitate "Forgot Password"
5. **Social Login**: Google/GitHub OAuth integration
6. **localStorage Encryptie**: Encriptarea token-ului în localStorage

## Testing

### Test Manual
```bash
# Register
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123",
    "confirmPassword": "Test123",
    "fullName": "Test User",
    "userType": 0
  }'

# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123"
  }'
```

## Probleme Cunoscute și Soluții

### 1. CORS Issues
Dacă apelurile HTTP eșuează, verifica CORS policy în `Program.cs`:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

app.UseCors("AllowAll");
```

### 2. Token Storage
Token-ul este stocat în memorie. Pentru persistență, adaugă localStorage via JS:
```csharp
// În RegisterCandidate.razor OnAfterRender
await JS.InvokeVoidAsync("localStorage.setItem", "token", result.Token);
```

## Dependencies

- MediatR - CQRS pattern
- Microsoft.AspNetCore.Identity - Identity management
- System.IdentityModel.Tokens.Jwt - JWT generation
- MudBlazor - UI components
- FluentValidation - Validation (via CQRS behavior)
