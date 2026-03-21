# Instrucțiuni pentru Testarea Sistemului de Autentificare

## 1. Pregătire Mediu

### Actualizare Database
```bash
cd JobFinder.Server
dotnet ef database update
```

Aceasta va crea tabelele Identity pe baza migrations existente.

## 2. Pornire Aplicație

```bash
# Din directorul rădăcină
dotnet run
```

Aplicația va fi disponibilă pe `https://localhost:5001` și API pe `https://localhost:5001/api`

## 3. Testare Manual în Browser

### Test 1: Înregistrare Candidat
1. Accesează `https://localhost:5001/register/candidate`
2. Completează formularul:
   - **Nume complet**: John Doe
   - **Email**: john.doe@example.com
   - **Parolă**: Test123456
   - **Confirmă parolă**: Test123456
3. Fă clic pe "Creează cont"
4. **Rezultat așteptat**: Mesaj de succes + redirecționare la `/candidate/dashboard`

### Test 2: Înregistrare Angajator
1. Accesează `https://localhost:5001/register/employer`
2. Completează formularul:
   - **Numele companiei**: Tech Corp
   - **Email**: contact@techcorp.com
   - **Parolă**: Test123456
   - **Confirmă parolă**: Test123456
3. Fă clic pe "Creează cont"
4. **Rezultat așteptat**: Mesaj de succes + redirecționare la `/employer/dashboard`

### Test 3: Login Candidat
1. Accesează `https://localhost:5001/login`
2. Completează formularul:
   - **Email**: john.doe@example.com
   - **Parolă**: Test123456
3. Fă clic pe "Conectare"
4. **Rezultat așteptat**: Mesaj de succes + redirecționare la `/candidate/dashboard`

### Test 4: Login cu Credențiale Greșite
1. Accesează `https://localhost:5001/login`
2. Completează:
   - **Email**: john.doe@example.com
   - **Parolă**: WrongPassword
3. Fă clic pe "Conectare"
4. **Rezultat așteptat**: Eroare "Email sau parolă incorectă"

## 4. Testare cu Postman/cURL

### Test 5: Register via API
```bash
curl -X POST "https://localhost:5001/api/auth/register" \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "email": "api@test.com",
    "password": "Test123456",
    "confirmPassword": "Test123456",
    "fullName": "API Test User",
    "userType": 0
  }'
```

**Răspuns așteptat:**
```json
{
  "userId": "guid-here",
  "email": "api@test.com",
  "userType": 0,
  "token": "eyJhbGc...",
  "expiresAt": "2024-12-21T10:30:00Z"
}
```

### Test 6: Login via API
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "email": "api@test.com",
    "password": "Test123456"
  }'
```

**Răspuns așteptat:** Același JSON ca Test 5

### Test 7: Register cu Email Duplicat
```bash
curl -X POST "https://localhost:5001/api/auth/register" \
  -H "Content-Type: application/json" \
  -k \
  -d '{
    "email": "api@test.com",
    "password": "Test123456",
    "confirmPassword": "Test123456",
    "fullName": "Another User",
    "userType": 1
  }'
```

**Răspuns așteptat (400):**
```json
{
  "message": "Acest email este deja înregistrat"
}
```

## 5. Validări Client-Side

### Test 8: Validare Email
1. Accesează formularul de înregistrare
2. Completează: test@invalid (email invalid)
3. Fă clic pe "Creează cont"
4. **Rezultat așteptat**: Eroare "Email invalid"

### Test 9: Validare Parolă
1. Accesează formularul de înregistrare
2. Completează parolă cu mai puțin de 6 caractere (ex: "Test1")
3. Fă clic pe "Creează cont"
4. **Rezultat așteptat**: Eroare "Parola trebuie să aibă între 6 și 100 caractere"

### Test 10: Validare Confirmare Parolă
1. Accesează formularul de înregistrare
2. Completează parolă: Test123456
3. Completează confirmare: Test12345
4. Fă clic pe "Creează cont"
5. **Rezultat așteptat**: Eroare "Parolele nu coincid"

## 6. Verificare Database

```sql
-- Conectează-te la SQL Server și rulează:
SELECT [Id], [UserName], [Email], [UserType], [EmailConfirmed] 
FROM [jobfinder-db].[dbo].[AspNetUsers]
ORDER BY [CreatedAt] DESC
```

Ar trebui să vezi utilizatorii înregistrați cu EmailConfirmed = 0 (false)

## 7. Verificare Token JWT

1. Copiază token-ul din răspunsul API
2. Accesează https://jwt.io
3. Lipește token-ul în secțiunea "Encoded"
4. **Verifică payload-ul:**
   - `sub`: UserId (NameIdentifier)
   - `email`: Email utilizatorului
   - `UserType`: Tipul utilizatorului (0, 1, sau 2)
   - `exp`: Timestamp expirare (should be 24 hours from now)

## 8. Troubleshooting

### Eroare: "Connection string 'DefaultConnection' not found"
- **Soluție**: Verifica appsettings.json și editează connection string-ul

### Eroare: "JWT Key not configured"
- **Soluție**: Verifica Jwt:Key în appsettings.json

### CORS Errors
- **Soluție**: Adaugă CORS policy în Program.cs

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

### Token nu se stochează
- **Soluție**: Token-ul este stocat în memorie pe client. Pentru persistență permanentă, implementează localStorage via JavaScript

## 9. Cleanup (Ștergere Date Test)

```sql
-- Ștergere utilizatori de test (ATENȚIE: irreversibil!)
DELETE FROM [jobfinder-db].[dbo].[AspNetUsers]
WHERE [Email] LIKE '%test%' OR [Email] LIKE '%@example.com'
```

## Checklist Final

- [ ] Înregistrare candidat reușită
- [ ] Înregistrare angajator reușită
- [ ] Login cu credențiale corecte
- [ ] Login cu credențiale greșite arată eroare
- [ ] Parolele nu sunt salvate în plaintext (hash-ate în DB)
- [ ] Token JWT este generat și conține informațiile corecte
- [ ] Validări client-side funcționează
- [ ] Redirecționări la dashboard-uri funcționează
