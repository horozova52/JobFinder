# 🚀 Quick Reference - Sistem Autentificare JobFinder

## ⚡ Setup Rapid

```bash
# 1. Database
dotnet ef database update

# 2. Build
dotnet build

# 3. Run
dotnet run --project JobFinder.Server
```

## 🌐 URL-uri Principale

| Pagina | URL | Descriere |
|--------|-----|-----------|
| Login | `/login` | Autentificare |
| Register Candidat | `/register/candidate` | Înregistrare candidat |
| Register Angajator | `/register/employer` | Înregistrare angajator |
| API Register | `POST /api/auth/register` | Endpoint API |
| API Login | `POST /api/auth/login` | Endpoint API |

## 📝 API Request/Response Examples

### Register Request
```json
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123",
  "confirmPassword": "SecurePass123",
  "fullName": "John Doe",
  "userType": 0
}
```

### Register Response (Success)
```json
200 OK
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "userType": 0,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-12-21T10:30:00Z"
}
```

### Login Request
```json
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123"
}
```

## ✅ Testing Checklist

- [ ] Register candidat → Success → Redirect /candidate/dashboard
- [ ] Register angajator → Success → Redirect /employer/dashboard
- [ ] Login valid → Success → Redirect based on UserType
- [ ] Login invalid → Error message "Email sau parolă incorectă"
- [ ] Duplicate email → Error "Email deja înregistrat"
- [ ] Invalid email format → Error "Email invalid"
- [ ] Password < 6 chars → Error "Parolă între 6-100"
- [ ] Passwords mismatch → Error "Parolele nu coincid"
- [ ] Token in localStorage → Verify via browser DevTools
- [ ] Token JWT valid → Decode la jwt.io

## 🔧 Configurație

### appsettings.json
```json
{
  "Jwt": {
    "Key": "your-super-secret-32-chars-key",
    "Issuer": "JobFinder",
    "Audience": "JobFinderClient"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=...;Initial Catalog=jobfinder-db;..."
  }
}
```

### Program.cs (Key Settings)
```csharp
// Password policy
options.Password.RequireDigit = false;
options.Password.RequireNonAlphanumeric = false;
options.Password.RequireUppercase = false;
options.SignIn.RequireConfirmedAccount = false;

// Sau în producție:
options.SignIn.RequireConfirmedAccount = true;
```

## 📁 Fișiere Importante

```
JobFinder.Server/
├── Controllers/
│   └── AuthController.cs ⭐
├── appsettings.json ⭐
└── Program.cs ⭐

JobFinder.Client/
├── Pages/
│   ├── Login.razor ⭐
│   ├── RegisterCandidate.razor ⭐
│   └── RegisterEmployer.razor ⭐
└── Program.cs

JobFinder.UseCases/
└── Features/Identity/Commands/Register/
    ├── RegisterCommand.cs ⭐
    └── RegisterCommandHandler.cs ⭐

JobFinder.Shared/
├── DTOs/Identity/
│   ├── RegisterDto.cs ⭐
│   ├── LoginDto.cs
│   └── AuthResponseDto.cs
└── Enums/
    └── UserType.cs
```

## 🐛 Troubleshooting Quick Fixes

| Problem | Solution |
|---------|----------|
| "JWT Key not configured" | Check `appsettings.json` Jwt:Key |
| "Migration pending" | Run `dotnet ef database update` |
| CORS errors | Add CORS in `Program.cs` |
| Login fails after register | Set `RequireConfirmedAccount = false` |
| Token not persisting | Implement localStorage in JS |
| Password always invalid | Check password policy settings |
| Email not unique | Verify database has unique constraint |

## 🔐 Security Quick Check

- [ ] JWT Key >= 32 characters
- [ ] HTTPS enabled in production
- [ ] Password hashing active (PBKDF2)
- [ ] Email validation enabled
- [ ] CORS restricted in production
- [ ] Rate limiting implemented
- [ ] SQL injection prevention (EF Core)
- [ ] XSS protection (Blazor default)

## 📊 Database Query Quick Reference

```sql
-- View all users
SELECT [Id], [UserName], [Email], [UserType], [CreatedAt] 
FROM [AspNetUsers]
ORDER BY [CreatedAt] DESC

-- Delete test user
DELETE FROM [AspNetUsers] 
WHERE [Email] = 'test@example.com'

-- Count users by type
SELECT [UserType], COUNT(*) 
FROM [AspNetUsers] 
GROUP BY [UserType]

-- Reset failed login attempts
UPDATE [AspNetUsers] 
SET [AccessFailedCount] = 0 
WHERE [Email] = 'user@example.com'
```

## 🎯 Common Commands

```bash
# Build
dotnet build

# Build with verbose
dotnet build --verbosity detailed

# Run
dotnet run

# Run specific project
dotnet run --project JobFinder.Server

# Watch for changes
dotnet watch run

# Database
dotnet ef migrations add InitialCreate
dotnet ef database update

# Clean build
dotnet clean && dotnet build

# Publish
dotnet publish -c Release
```

## 📱 Browser DevTools Tips

### Verify Token
```javascript
// Console
const token = localStorage.getItem('token');
const decoded = JSON.parse(atob(token.split('.')[1]));
console.log(decoded);
```

### Clear All Storage
```javascript
localStorage.clear();
sessionStorage.clear();
```

### View All LocalStorage
```javascript
for(let i = 0; i < localStorage.length; i++) {
  console.log(localStorage.key(i), localStorage.getItem(localStorage.key(i)));
}
```

## 📞 Support Resources

1. **Documentation**: 
   - `AUTHENTICATION_IMPLEMENTATION.md` - Detalii complete
   - `TESTING_INSTRUCTIONS.md` - Teste manuale
   - `COMPLETE_GUIDE.md` - Ghid complet
   - `ARCHITECTURE_DETAILS.md` - Arhitectură

2. **Official Docs**:
   - [ASP.NET Identity](https://learn.microsoft.com/en-us/aspnet/identity/)
   - [JWT.io](https://jwt.io)
   - [Blazor Security](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/)

3. **Debugging**:
   - Check build output
   - Review Program.cs configuration
   - Inspect API responses in Network tab
   - Check browser console for errors

## ✨ Pro Tips

1. **Local Token Testing**:
   ```javascript
   // Paste in browser console after login
   JSON.parse(atob(localStorage.getItem('token').split('.')[1]))
   ```

2. **Quick DB Check**:
   ```sql
   SELECT COUNT(*) FROM [AspNetUsers]
   ```

3. **Reset Everything**:
   ```bash
   dotnet ef database drop -f
   dotnet ef database update
   ```

4. **View Logs**:
   Set in `appsettings.json`:
   ```json
   "Logging": {
     "LogLevel": {
       "Default": "Debug"
     }
   }
   ```

## 🎉 Success Indicators

✅ Register pagă opens without errors  
✅ Form validation works client-side  
✅ API endpoint responds with token  
✅ Token stored in localStorage  
✅ Redirect to appropriate dashboard  
✅ Login works with registered credentials  
✅ Invalid credentials show error  
✅ Database shows new users  

---

**Sistem gata de productie! 🚀**

Pentru detalii complete, consultă documentația din project root.
