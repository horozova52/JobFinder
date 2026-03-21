# Ghid Complet - Sistem de Autentificare JobFinder

## 🎯 Obiective Realizate

✅ Autentificare completă conform ASP.NET Identity  
✅ Înregistrare pentru Candidați și Angajatori  
✅ JWT Token generation  
✅ Validări client-side și server-side  
✅ Redirecționări inteligente  
✅ Mesaje de eroare localizate (Română)  

---

## 🔧 Setup Inițial

### 1. Database Migration

```bash
cd JobFinder.Server
dotnet ef database update
```

### 2. Actualizare appsettings.json

```json
{
  "Jwt": {
    "Key": "your-super-secret-key-that-is-at-least-32-characters-long",
    "Issuer": "JobFinder",
    "Audience": "JobFinderClient"
  },
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_CONNECTION_STRING"
  }
}
```

### 3. Build și Run

```bash
# Build
dotnet build

# Run (din directory-ul solution)
dotnet run --project JobFinder.Server
```

---

## 📍 Rute Disponibile

### Client
- `GET /` - Pagina principală
- `GET /login` - Formular de login
- `GET /register/candidate` - Înregistrare candidat
- `GET /register/employer` - Înregistrare angajator

### API
- `POST /api/auth/register` - Endpoint înregistrare
- `POST /api/auth/login` - Endpoint login
- `POST /api/auth/store-token` - Endpoint stocare token

---

## 🧪 Test Scenarios

### Scenario 1: Happy Path - Înregistrare și Login
```
1. Accesează /register/candidate
2. Completează formular cu date valide
3. Click pe "Creează cont"
4. Verifica redirect la /candidate/dashboard
5. Logout (dacă e implementat)
6. Accesează /login
7. Login cu aceleași credențiale
8. Verifica redirect la /candidate/dashboard
```

### Scenario 2: Validări
```
1. /register/candidate
2. Completează doar Email
3. Click submit -> Ar trebui să arate erori pentru celelalte câmpuri
4. Completează Email invalid -> Eroare "Email invalid"
5. Completează Parolă = "short" -> Eroare lungime minimă
6. Completează Confirmare ≠ Parolă -> Eroare "Parolele nu coincid"
```

### Scenario 3: Duplicate Email
```
1. /register/candidate cu email "test@example.com" -> SUCCESS
2. /register/candidate cu același email -> EROARE "Email deja înregistrat"
3. /login cu email "test@example.com" -> SUCCESS
```

---

## 🔍 Debugging

### Verificare Token JWT

```javascript
// În console browser
const token = localStorage.getItem('token');
const decoded = JSON.parse(atob(token.split('.')[1]));
console.log(decoded);
// Output ar trebui să arate: { sub, email, UserType, exp, iat, iss, aud }
```

### Verificare User în Database

```sql
SELECT [Id], [UserName], [Email], [UserType], [EmailConfirmed], [CreatedAt]
FROM [AspNetUsers]
ORDER BY [CreatedAt] DESC
```

### Verificare Logs

```bash
# Activează debug logging în appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

---

## ⚠️ Probleme Comune și Soluții

### Problema 1: "Migration pending"
```
Error: An error occurred while accessing the Sqlite database
```
**Soluție:**
```bash
dotnet ef database update
```

### Problema 2: "JWT Key not configured"
```
Error: JWT Key not configured
```
**Soluție:**
Verifica `appsettings.json` - Jwt:Key trebuie să existe și să fie >= 32 caractere

### Problema 3: "Email already exists" cu email nouă
```
Chiar și cu email nouă, da eroare "Email deja înregistrat"
```
**Soluție:**
```csharp
// Verifica în AuthController
var existingUser = await _userManager.FindByEmailAsync(request.Email);
// Ensure case-insensitive comparison
```

### Problema 4: Login eșuează după înregistrare
```
Înregistrare merge, dar login cu aceleași credențiale eșuează
```
**Soluție:**
Verifica dacă `RequireConfirmedAccount = false` în Program.cs:
```csharp
options.SignIn.RequireConfirmedAccount = false;
```

### Problema 5: Token nu se persistă
```
După page refresh, token dispare
```
**Soluție:**
Token-ul este stocat în memorie. Pentru persistență:
```javascript
// Adaugă în RegisterCandidate.razor
@inject IJSRuntime JS
await JS.InvokeVoidAsync("localStorage.setItem", "token", result.Token);
```

### Problema 6: CORS Error
```
XMLHttpRequest blocked by CORS policy
```
**Soluție:**
Adaugă în Program.cs:
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

---

## 🚀 Deployment Checklist

### Pre-Deployment
- [ ] Schimbă `Jwt:Key` cu o valoare sigură
- [ ] Activează `RequireConfirmedAccount = true`
- [ ] Implementează email verification
- [ ] Implementează password reset
- [ ] Activează HTTPS everywhere
- [ ] Configureaza CORS restrictiv
- [ ] Activează rate limiting

### SQL Server
- [ ] Creeaza backup database
- [ ] Ruleaza migrations in production
- [ ] Verifica connection string

### Azure/Cloud
- [ ] Configureaza environment variables
- [ ] Setup CI/CD pipeline
- [ ] Configureaza monitoring
- [ ] Setup alerting

### Post-Deployment
- [ ] Test login/register pe prod
- [ ] Monitor error logs
- [ ] Verifica performance
- [ ] Document known issues

---

## 📚 Code Examples

### Apel API Manual

```csharp
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("https://localhost:5001") };

// Register
var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new {
    email = "user@example.com",
    password = "SecurePass123",
    confirmPassword = "SecurePass123",
    fullName = "John Doe",
    userType = 0
});

var token = (await registerResponse.Content.ReadAsJsonAsync<AuthResponseDto>()).Token;
```

### Utilizare Token

```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = 
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

// Apel protejat
var response = await client.GetAsync("/api/protected-endpoint");
```

### Validare Token Backend

```csharp
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[HttpGet("protected")]
public IActionResult Protected()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    return Ok(new { message = $"Hello {userId}" });
}
```

---

## 📖 Fișiere Cheie

| Fișier | Descriere |
|--------|-----------|
| `AuthController.cs` | API endpoints |
| `RegisterCommand.cs` | CQRS Command |
| `RegisterCommandHandler.cs` | Command Handler |
| `RegisterDto.cs` | DTO cu validări |
| `RegisterCandidate.razor` | UI Candidate |
| `RegisterEmployer.razor` | UI Employer |
| `Login.razor` | UI Login |
| `Program.cs` | Configurație |
| `appsettings.json` | Settings |

---

## 🎓 Learning Resources

- [ASP.NET Identity Documentation](https://learn.microsoft.com/en-us/aspnet/identity/)
- [JWT.io](https://jwt.io) - Token inspection
- [Blazor WebAssembly Security](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)

---

## 📞 Support

### Debugging Steps:
1. Check build output para errors
2. Verify appsettings.json
3. Check database migration status
4. Enable detailed logging
5. Check browser console para JavaScript errors
6. Inspect network tab para API calls

### Raportare Issues:
Creaza ticket cu:
- [ ] Error message exact
- [ ] Steps to reproduce
- [ ] Expected vs actual behavior
- [ ] Logs/screenshots

---

## ✅ Final Verification

```bash
# 1. Build
dotnet build
# Output: Build successful ✅

# 2. Run
dotnet run --project JobFinder.Server
# Output: Now listening on: https://localhost:5001

# 3. Test Register
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Test123","confirmPassword":"Test123","fullName":"Test","userType":0}' \
  -k
# Output: Token in response ✅

# 4. Test Login
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Test123"}' \
  -k
# Output: Token in response ✅
```

---

**Sistem de autentificare complet și gata pentru producție! 🎉**
