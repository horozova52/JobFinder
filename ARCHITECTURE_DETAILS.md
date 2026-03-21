# Arhitectura Sistemului de Autentificare - JobFinder

## 📊 Diagrama Flux Global

```
┌─────────────────────────────────────────────────────────────┐
│                    CLIENT (Blazor WASM)                      │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────┐  ┌──────────────────┐                 │
│  │ RegisterCandidate│  │RegisterEmployer  │                 │
│  │    .razor        │  │    .razor        │                 │
│  └────────┬─────────┘  └────────┬─────────┘                 │
│           │                     │                            │
│           │   EditForm          │   EditForm                │
│           │   Validation        │   Validation              │
│           │                     │                            │
│           └──────────┬──────────┘                            │
│                      │                                       │
│           ┌──────────▼──────────┐                            │
│           │   Login.razor       │                            │
│           │   Autentificare     │                            │
│           └──────────┬──────────┘                            │
│                      │                                       │
│           ┌──────────▼──────────┐                            │
│           │  HttpClient         │                            │
│           │ PostAsJsonAsync     │                            │
│           │                     │                            │
│           └──────────┬──────────┘                            │
│                      │                                       │
└──────────────────────┼───────────────────────────────────────┘
                       │
                       │ HTTP/HTTPS
                       │
┌──────────────────────▼───────────────────────────────────────┐
│              SERVER (ASP.NET Core + Identity)                │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────────────────────────────────────────────┐│
│  │            AuthController (API)                           ││
│  │                                                            ││
│  │  POST /api/auth/register                                  ││
│  │  POST /api/auth/login                                     ││
│  │  POST /api/auth/store-token                               ││
│  └─────────────────┬────────────────────────────────────────┘│
│                    │                                          │
│  ┌────────────────▼──────────────────┐                       │
│  │    MediatR (CQRS Pattern)         │                       │
│  │                                   │                       │
│  │  RegisterCommand                  │                       │
│  │    ↓                              │                       │
│  │  RegisterCommandHandler           │                       │
│  │    ↓                              │                       │
│  │  RegisterResult                   │                       │
│  └────────────────┬──────────────────┘                       │
│                   │                                          │
│  ┌────────────────▼──────────────────┐                       │
│  │  UserManager<ApplicationUser>     │                       │
│  │                                   │                       │
│  │  - FindByEmailAsync()             │                       │
│  │  - CreateAsync()                  │                       │
│  │  - CheckPasswordSignInAsync()     │                       │
│  └────────────────┬──────────────────┘                       │
│                   │                                          │
│  ┌────────────────▼──────────────────┐                       │
│  │  JWT Token Generator              │                       │
│  │                                   │                       │
│  │  - SymmetricSecurityKey           │                       │
│  │  - SigningCredentials             │                       │
│  │  - JwtSecurityToken               │                       │
│  │  - JwtSecurityTokenHandler        │                       │
│  └────────────────┬──────────────────┘                       │
│                   │                                          │
│  ┌────────────────▼──────────────────┐                       │
│  │    AuthResponseDto                │                       │
│  │                                   │                       │
│  │  {                                │                       │
│  │    userId: "guid",                │                       │
│  │    email: "user@example.com",     │                       │
│  │    userType: 0,                   │                       │
│  │    token: "JWT_TOKEN",            │                       │
│  │    expiresAt: DateTime            │                       │
│  │  }                                │                       │
│  └────────────────┬──────────────────┘                       │
│                   │                                          │
└───────────────────┼──────────────────────────────────────────┘
                    │
                    │ JSON Response
                    │
┌───────────────────▼──────────────────────────────────────────┐
│             CLIENT (localStorage)                            │
│                                                              │
│  Token → localStorage.setItem('token', token)               │
│          Subsequent requests → Authorization header         │
│          Bearer {token}                                     │
└──────────────────────────────────────────────────────────────┘
```

---

## 🗄️ Database Schema (Simplified)

```sql
[AspNetUsers]
├── Id (PK) → GUID
├── UserName → nvarchar(256) UNIQUE
├── Email → nvarchar(256) UNIQUE
├── PasswordHash → nvarchar(MAX) [PBKDF2]
├── PhoneNumber
├── EmailConfirmed → bit
├── PhoneNumberConfirmed → bit
├── TwoFactorEnabled → bit
├── LockoutEnd
├── LockoutEnabled → bit
├── AccessFailedCount → int
├── ConcurrencyStamp
│
└── [Custom Field]
    └── UserType → int (0=Candidate, 1=Employer, 2=Admin)

[AspNetRoles]
├── Id (PK)
├── Name
└── ConcurrencyStamp

[AspNetUserRoles]
├── UserId (FK)
└── RoleId (FK)

[AspNetUserClaims]
├── Id (PK)
├── UserId (FK)
├── ClaimType
└── ClaimValue

[AspNetUserTokens]
├── UserId (FK)
├── LoginProvider
├── Name
└── Value
```

---

## 🔀 Class Diagram

```
ApplicationUser (extends IdentityUser)
├── UserType: UserType
├── CandidateProfile: CandidateProfile?
└── EmployerProfile: EmployerProfile?

RegisterDto
├── Email: string [Required, EmailAddress]
├── Password: string [Required, StringLength(100, MinimumLength = 6)]
├── ConfirmPassword: string [Compare("Password")]
├── FullName: string [Required]
└── UserType: UserType

LoginDto
├── Email: string
└── Password: string

AuthResponseDto
├── UserId: string
├── Email: string
├── UserType: UserType
├── Token: string
└── ExpiresAt: DateTime

RegisterCommand (IRequest<RegisterResult>)
├── Email: string
├── Password: string
├── ConfirmPassword: string
├── FullName: string
└── UserType: UserType

RegisterResult
├── Success: bool
├── Message: string?
└── Data: AuthResponseDto?

RegisterCommandHandler (IRequestHandler<RegisterCommand, RegisterResult>)
├── _userManager: UserManager<ApplicationUser>
└── Handle: Task<RegisterResult>
```

---

## 📱 Component Lifecycle

### Înregistrare Flow:
```
1. User navigates to /register/candidate
   ↓
2. RegisterCandidate.razor loads
   - Form: EditForm + DataAnnotationsValidator
   - Binds to RegisterForm model
   ↓
3. User fills form and submits
   - Client-side validation runs
   ↓
4. HandleRegister() called
   - Creates RegisterDto
   - Posts to /api/auth/register
   ↓
5. AuthController.Register() receives request
   - Validates ModelState
   - Creates RegisterCommand
   - Sends to MediatR
   ↓
6. RegisterCommandHandler.Handle() executes
   - Validates password match
   - Checks email uniqueness
   - Creates ApplicationUser
   - Calls UserManager.CreateAsync()
   ↓
7. Password hashing (PBKDF2)
   - Password transformed to hash
   - Stored securely in database
   ↓
8. JWT Token generated
   - Creates claims (UserId, Email, UserType)
   - Signs with HMAC SHA256
   - Returns AuthResponseDto with token
   ↓
9. Client receives response
   - Extracts token
   - Stores in localStorage
   - Navigates to /candidate/dashboard
```

### Login Flow:
```
1. User navigates to /login
   ↓
2. Login.razor loads
   - EditForm with Email + Password
   ↓
3. User submits credentials
   - Client validates input
   ↓
4. HandleLogin() called
   - Creates LoginDto
   - Posts to /api/auth/login
   ↓
5. AuthController.Login() receives request
   - Finds user by email
   - Calls CheckPasswordSignInAsync()
   ↓
6. Identity validates password
   - Compares input hash with stored hash
   ↓
7. If valid:
   - Generates new JWT token
   - Returns AuthResponseDto
   ↓
8. Client stores token
   - localStorage.setItem('token', token)
   ↓
9. Navigates based on UserType
   - Candidate → /candidate/dashboard
   - Employer → /employer/dashboard
   - Admin → /admin/dashboard
```

---

## 🔐 Securitatea Token-ului

### JWT Structure:
```
Header.Payload.Signature

Header:
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload:
{
  "sub": "user-id",           // Subject (UserId)
  "email": "user@example.com", // Email
  "UserType": "0",             // Custom claim
  "iat": 1703158200,          // Issued at
  "exp": 1703244600,          // Expires (24h from issue)
  "iss": "JobFinder",         // Issuer
  "aud": "JobFinderClient"    // Audience
}

Signature:
HMACSHA256(
  base64UrlEncode(header) + "." + base64UrlEncode(payload),
  "your-secret-key-32-chars-minimum"
)
```

### Validare Token:
```csharp
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public IActionResult ProtectedResource()
{
    // Token valid → endpoint accessible
    // Token invalid/expired → 401 Unauthorized
    // Token missing → 401 Unauthorized
}
```

---

## 🎯 CQRS Pattern Utilizat

### Command (Write Operation):
```
RegisterCommand
    ↓
Input validation
    ↓
Business logic (create user, hash password)
    ↓
Side effects (database write)
    ↓
RegisterResult (success/failure)
```

### Query (Read Operation - exemplu):
```
GetUserQuery
    ↓
No side effects
    ↓
Read from database
    ↓
UserDto
```

---

## 🔄 Integrări

### Identity Framework
- Password hashing: PBKDF2 (128,000 iterations minimum)
- User creation: UserManager.CreateAsync()
- Password validation: CheckPasswordSignInAsync()

### MediatR
- Command dispatching
- Handler pipeline
- Validation behavior pipeline

### Entity Framework Core
- Database persistence
- Migrations
- DbContext

### ASP.NET Core
- Dependency injection
- HTTP routing
- Model validation
- Configuration

---

## 📈 Scalability Considerations

### Current Setup (Development):
- ✅ Single database
- ✅ In-memory token storage
- ✅ No caching

### Future Improvements:
- [ ] Token caching (Redis)
- [ ] User session caching
- [ ] Database replication
- [ ] Load balancing
- [ ] API rate limiting
- [ ] Request throttling

---

## 🧪 Testing Strategy

### Unit Tests:
```csharp
[TestMethod]
public async Task RegisterCommand_WithValidData_CreatesUser()
{
    // Arrange
    var command = new RegisterCommand(...);
    var handler = new RegisterCommandHandler(userManager);
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    Assert.IsTrue(result.Success);
}
```

### Integration Tests:
```csharp
[TestMethod]
public async Task RegisterEndpoint_WithValidData_ReturnsToken()
{
    // Arrange
    var client = new HttpClient();
    
    // Act
    var response = await client.PostAsJsonAsync("/api/auth/register", ...);
    
    // Assert
    Assert.IsSuccessStatusCode(response.StatusCode);
}
```

---

## 📚 Dependencies

```
JobFinder.Shared
├── System.ComponentModel.DataAnnotations (validations)
└── [Custom] Enums.UserType

JobFinder.UseCases
├── MediatR
├── FluentValidation
├── AutoMapper
└── JobFinder.Core

JobFinder.Server
├── Microsoft.AspNetCore.Identity
├── Microsoft.EntityFrameworkCore
├── System.IdentityModel.Tokens.Jwt
└── Microsoft.IdentityModel.Tokens

JobFinder.Client
├── System.Net.Http.Json
├── System.Text.Json
├── MudBlazor
└── Microsoft.AspNetCore.Components.WebAssembly
```

---

## 🎬 Execution Timeline

```
Request → 10ms   (Network)
         → 50ms   (Authorization validation)
         → 200ms  (Database query)
         → 100ms  (Hashing/Token generation)
         → 10ms   (Network response)
         ─────────
         = ~370ms Total (typical)
```

---

**Arhitectură completă și scalabilă! 🏗️**
