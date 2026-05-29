# 📚 CSharp-ASP.NET-Core-Library Management API

> **Production-grade ASP.NET Core 10 Web API** with Clean Architecture, JWT authentication,
> role-based access control, EF Core + SQLite, applied DSA patterns, and complete
> **localStorage token persistence** with **dark mode Swagger UI**.

**Latest Update:** ✅ LocalStorage token persistence + Dark mode Swagger UI + Frontend auth service

---

## 🎯 Key Features

✅ **JWT Authentication** - Secure token-based auth with refresh tokens  
✅ **LocalStorage Persistence** - Tokens saved and persist across page reloads  
✅ **Role-Based Access** - Admin, Librarian, Member roles with permission control  
✅ **Clean Architecture** - Domain → Application → Infrastructure → API layers  
✅ **Dark Mode Swagger UI** - Beautiful dark theme matching VS Code  
✅ **Auto Token Refresh** - Tokens auto-refreshed every 50 minutes  
✅ **Interactive Auth Page** - Test login/register/logout in browser  
✅ **Real-time Status** - Live auth status display on dashboard  
✅ **DSA Implementations** - Binary search, priority queue, soft deletes

---

## 📂 Project Structure

```
LibraryAPI/
├── src/
│   ├── Domain/                        # Core entities & interfaces
│   │   ├── Entities/                  # User, Author, Book, BorrowRecord, RefreshToken
│   │   ├── Enums/                     # UserRole, BookStatus, BorrowStatus, Genre
│   │   ├── Interfaces/Repositories/   # IGenericRepository, IUnitOfWork
│   │   ├── Interfaces/Services/       # IAuthService, IBookService, etc.
│   │   └── Common/                    # Result<T>, PagedResult<T>
│   │
│   ├── Infrastructure/                # Database, JWT, BCrypt
│   │   ├── Data/                      # DbContext, Fluent API configs, seed data
│   │   ├── Repositories/              # Generic & specific repository implementations
│   │   └── Services/                  # JwtService, AuthService
│   │
│   ├── Application/                   # Business logic layer
│   │   ├── DTOs/                      # Request/Response DTOs for all features
│   │   └── Services/                  # BookService, AuthorService, MemberService, BorrowService
│   │
│   └── API/                           # ASP.NET Core HTTP layer
│       ├── Controllers/               # BaseController + 5 feature controllers
│       ├── Middleware/                # ExceptionMiddleware, RequestLoggingMiddleware
│       ├── Extensions/                # ServiceExtensions (DI registration)
│       ├── appsettings.json           # Configuration
│       └── wwwroot/                   # ✨ NEW: Frontend files
│           ├── index.html             # Auth test page
│           ├── auth-service.js        # Auth service (login, logout, persistence)
│           ├── swagger-persistence.js # Swagger token injection
│           └── swagger-ui-dark.css    # Dark mode theme
│
├── Migrations/                        # Database migrations
├── library.db                         # SQLite database
├── LOCALSTORAGE_AUTH.md               # Token persistence guide
├── TOKEN_FLOW_DIAGRAM.md              # Visual flow diagrams
├── ARCHITECTURE.md                    # Architecture details
└── README.md                          # This file
```

---

## Design Patterns Used

| Pattern                   | Where                       | Why                                       |
| ------------------------- | --------------------------- | ----------------------------------------- |
| Repository + Unit of Work | Infrastructure/Repositories | Abstracts DB; enables atomic transactions |
| Result Pattern            | Domain/Common/Results.cs    | No exceptions across layer boundaries     |
| Dependency Injection      | Everywhere                  | Loose coupling; testability               |
| Template Method           | BaseController              | Shared `FromResult<T>` logic              |
| Guard Clauses             | All services                | Fail fast, reduce nesting                 |
| CQRS-lite                 | Separate read/write DTOs    | Clear intent, safe partial updates        |

---

## DSA Applied

| Algorithm/Structure       | File                              | Description                                  |
| ------------------------- | --------------------------------- | -------------------------------------------- |
| Binary Search O(log n)    | BookRepository.BinarySearchByIsbn | Find book by ISBN in sorted in-memory list   |
| Priority Queue O(n log n) | BorrowService.GetOverdueAsync     | Surfaces highest-overdue records first       |
| Soft Delete filter        | DbContext global query filter     | Hides deleted rows at DB level automatically |

---

## Role Permission Matrix

| Endpoint                    | Member | Librarian | Admin |
| --------------------------- | :----: | :-------: | :---: |
| GET /api/books              |   ✅   |    ✅     |  ✅   |
| POST /api/books             |   ❌   |    ✅     |  ✅   |
| PUT /api/books/:id          |   ❌   |    ✅     |  ✅   |
| DELETE /api/books/:id       |   ❌   |    ❌     |  ✅   |
| GET /api/members            |   ❌   |    ✅     |  ✅   |
| GET /api/members/me         |   ✅   |    ✅     |  ✅   |
| DELETE /api/members/:id     |   ❌   |    ❌     |  ✅   |
| POST /api/borrows           |   ✅   |    ✅     |  ✅   |
| PUT /api/borrows/:id/return |   ✅   |    ✅     |  ✅   |
| GET /api/borrows/overdue    |   ❌   |    ✅     |  ✅   |

---

## 🚀 Quick Start (5 minutes)

### Step 1: Prerequisites

```bash
dotnet --version   # Needs .NET 10.0+
```

### Step 2: Navigate to Project

```bash
cd "/media/md-seam/New_Volume4/C# Projects/Web API Project/Library Management API"
```

### Step 3: Restore & Build

```bash
dotnet restore
dotnet build
```

### Step 4: Setup Database (first time only)

```bash
dotnet ef database update
```

### Step 5: Run the Server

```bash
dotnet run
```

**Server starts at:** `http://localhost:5000`

---

## 🌐 Access Points

| URL                             | Purpose                    | Features                                                |
| ------------------------------- | -------------------------- | ------------------------------------------------------- |
| `http://localhost:5000`         | **Auth Test Page**         | Login, Register, Real-time status, localStorage display |
| `http://localhost:5000/swagger` | **Swagger UI (Dark Mode)** | API documentation, test endpoints, dark theme           |

---

## ✨ NEW: Token Persistence System

### How It Works

#### **Login → Tokens Saved to localStorage**

```
1. User enters email & password
   ↓
2. API validates & returns tokens
   ↓
3. Frontend saves to localStorage:
   - LibraryAPI_accessToken (JWT, 60 min expiry)
   - LibraryAPI_refreshToken (Refresh token, 7 day expiry)
   - LibraryAPI_userInfo (User object with role)
   ↓
4. User authenticated ✅
```

#### **Page Reload → Tokens Restored from localStorage**

```
1. User refreshes page (F5)
   ↓
2. auth-service.js loads from localStorage
   ↓
3. Tokens restored in memory
   ↓
4. User still authenticated ✅ (no re-login needed!)
```

#### **Logout → Tokens Cleared from localStorage**

```
1. User clicks logout button
   ↓
2. All localStorage keys removed
3. Backend token revoked (optional)
   ↓
4. User logged out ✅
```

### localStorage Keys

```javascript
LibraryAPI_accessToken; // JWT token (60 min expiry)
LibraryAPI_refreshToken; // Refresh token (7 day expiry)
LibraryAPI_userInfo; // User object { id, fullName, email, role }
```

### Browser Console API

Global object available in browser console:

```javascript
// Check authentication status (visual print to console)
libraryAuth.printAuthStatus();

// Login (saves tokens to localStorage)
await libraryAuth.login("admin@library.com", "Admin@123");

// Logout (clears tokens from localStorage)
await libraryAuth.logout();

// Get current user info
libraryAuth.getUser();

// Check if authenticated
libraryAuth.isAuthenticated();

// Get user role (Admin, Librarian, Member)
libraryAuth.getUserRole();

// Get access token
libraryAuth.loadAccessToken();

// Get refresh token
libraryAuth.loadRefreshToken();

// Get auth header for API calls
libraryAuth.getAuthHeader();
// Returns: { Authorization: "Bearer eyJhbGc..." }

// Make API request with auto-refresh
const response = await libraryAuth.apiRequest("/books");

// Refresh token manually (normally auto-refreshed)
await libraryAuth.refreshAccessToken();
```

---

## 🌙 Dark Mode Swagger UI

The Swagger UI now has a professional dark theme:

- **Background:** Dark blue-gray (#1e1e1e)
- **Text:** Light gray (#e0e0e0)
- **Accent:** Cyan blue (#0ea5e9)
- **Borders:** Medium gray (#3e3e42)
- **Styling:** Matches VS Code dark theme and Chrome DevTools

**Automatically Enabled:** Navigate to `http://localhost:5000/swagger` to see dark mode applied.

---

## 📡 API Endpoints

### Authentication

```
POST   /api/auth/register              # Register new member
       Body: { firstName, lastName, email, password, phoneNumber? }
       Returns: { accessToken, refreshToken, user }

POST   /api/auth/login                 # Login with email & password
       Body: { email, password }
       Returns: { accessToken, refreshToken, expiresAt, user }

POST   /api/auth/refresh               # Refresh access token
       Body: { refreshToken }
       Returns: { accessToken, refreshToken, user }

POST   /api/auth/revoke                # Logout (revoke token)
       [Authorized] Body: { refreshToken }

PUT    /api/auth/password              # Change password
       [Authorized] Body: { currentPassword, newPassword }
```

### Books

```
GET    /api/books                      # Get all books (paginated)
       ?page=1&pageSize=10&genre=Fiction&onlyAvailable=true

GET    /api/books/{id}                 # Get book by ID

GET    /api/books/isbn/{isbn}          # Get book by ISBN

GET    /api/books/search?term=harry    # Full-text search

POST   /api/books                      # Create book [Admin, Librarian]
       Body: { title, isbn, description, genre, authorId }

PUT    /api/books/{id}                 # Update book [Admin, Librarian]
       Body: { title, isbn, description, genre, authorId }

DELETE /api/books/{id}                 # Delete book [Admin only]
```

### Authors

```
GET    /api/authors                    # Get all authors (paginated)

GET    /api/authors/{id}               # Get author with books

POST   /api/authors                    # Create author [Admin, Librarian]
       Body: { firstName, lastName, nationality, bio }

PUT    /api/authors/{id}               # Update author [Admin, Librarian]

DELETE /api/authors/{id}               # Delete author [Admin only]
```

### Members

```
GET    /api/members                    # Get all members [Admin, Librarian]

GET    /api/members/{id}               # Get member [Admin, Librarian]

GET    /api/members/me                 # Get own profile [Authenticated]

PUT    /api/members/{id}               # Update profile [Self or Admin]
       Body: { firstName, lastName, phoneNumber }

DELETE /api/members/{id}               # Deactivate member [Admin only]
```

### Borrows

```
GET    /api/borrows                    # Get all borrows [Admin, Librarian]

GET    /api/borrows/overdue            # Get overdue borrows [Admin, Librarian]

GET    /api/borrows/my                 # Get own borrows [Authenticated]

POST   /api/borrows                    # Borrow a book [Authenticated]
       Body: { bookId }

PUT    /api/borrows/{borrowId}/return  # Return a book [Authenticated]

GET    /api/borrows/{borrowId}/fine    # Calculate fine [Authenticated]
```

---

## 👥 Roles & Permissions

### **Admin**

- ✅ Full access to all endpoints
- ✅ Manage books, authors, members
- ✅ View all borrow records
- ✅ Delete/deactivate users
- ✅ See overdue borrows

### **Librarian**

- ✅ Add/edit books and authors
- ✅ Manage borrow records
- ✅ View member information
- ✅ See overdue borrows & calculate fines
- ❌ Cannot delete users
- ❌ Cannot delete books

### **Member**

- ✅ View books and search
- ✅ Borrow and return books
- ✅ View own borrow history
- ✅ View own profile
- ❌ Cannot manage books/authors
- ❌ Cannot see other members' data

---

## 🔐 Default Credentials

```
Email:    admin@library.com
Password: Admin@123
Role:     Admin
```

**First Time Login:**

1. Go to `http://localhost:5000`
2. Click "Login"
3. Use credentials above
4. Tokens automatically saved to localStorage ✅

---

## 🧪 Testing Guide

### Test 1: Token Persistence

```
1. Go to http://localhost:5000
2. Login with admin@library.com / Admin@123
3. Check: Authenticated = ✅ YES
4. Press F5 (refresh page)
5. Check: Still authenticated ✅
✅ Result: Tokens persisted in localStorage
```

### Test 2: Logout Clears Tokens

```
1. After login, click "Logout" button
2. Check: Authenticated = ❌ NO
3. Open DevTools (F12) → Application → Local Storage
4. Check: No LibraryAPI_* keys
✅ Result: Tokens properly cleared
```

### Test 3: Register New Member

```
1. Go to http://localhost:5000
2. Fill registration form
3. Click "Register"
4. Check: Auto-logged in ✅
5. Press F5
6. Check: Still authenticated ✅
✅ Result: Auto-save works
```

### Test 4: Swagger Dark Mode

```
1. Login at http://localhost:5000
2. Go to http://localhost:5000/swagger
3. Check: Dark background applied
4. Check: All text visible and readable
✅ Result: Dark mode works perfectly
```

### Test 5: Browser Console API

```
1. Press F12 (open DevTools)
2. Go to Console tab
3. Type: libraryAuth.printAuthStatus()
4. Press Enter
5. Check: Detailed status printed
✅ Result: Global API works
```

---

## 📁 Frontend Files (New)

### **src/API/wwwroot/index.html** (15 KB)

Interactive authentication test page with:

- Login form (pre-filled with test credentials)
- Register form (create new members)
- Real-time auth status display
- localStorage inspection
- Console logging

### **src/API/wwwroot/auth-service.js** (12 KB)

Main authentication service with:

- `login()` - Authenticate user
- `logout()` - Clear all tokens
- `register()` - Create new account
- `refreshAccessToken()` - Manual token refresh
- `saveTokensToLocalStorage()` - Persist tokens
- `loadAccessToken()` - Restore from localStorage
- `isAuthenticated()` - Check auth status
- `getAuthHeader()` - Get Bearer token for API calls

### **src/API/wwwroot/swagger-persistence.js** (5 KB)

Swagger UI integration:

- Auto-loads tokens on Swagger page
- Injects tokens into API requests
- Shows auth status badge

### **src/API/wwwroot/swagger-ui-dark.css** (6 KB)

Dark mode theme for Swagger:

- Dark backgrounds
- Light text for contrast
- Cyan accents
- Professional styling

---

## 🛠️ Backend Modifications

### Program.cs Changes

```csharp
// Added: Enable static file serving (for wwwroot/)
app.UseStaticFiles();

// Added: Inject dark mode CSS into Swagger
c.InjectStylesheet("/swagger-ui-dark.css");

// Added: Inject token persistence script
c.InjectJavascript("/swagger-persistence.js");
```

---

## 📚 Documentation Files

### **LOCALSTORAGE_AUTH.md** (9 KB)

Complete guide to token persistence system:

- Feature descriptions
- Implementation details
- JavaScript API reference
- Test scenarios
- Troubleshooting guide
- Security considerations

### **TOKEN_FLOW_DIAGRAM.md** (11 KB)

Visual flow diagrams showing:

- Login flow
- Page reload flow
- Logout flow
- Token refresh flow
- API request flow
- localStorage structure
- Component interaction

### **ARCHITECTURE.md**

Clean Architecture overview:

- Layer responsibilities
- Design patterns
- Dependency flow

---

## 🔧 Configuration

### **appsettings.json**

```json
{
  "Jwt": {
    "Secret": "your-32-char-secret-key-here-min",
    "Issuer": "LibraryAPI",
    "Audience": "LibraryAPI",
    "AccessTokenMinutes": 60,
    "RefreshTokenDays": 7
  },
  "ConnectionStrings": {
    "Default": "Data Source=library.db"
  },
  "Library": {
    "MaxBorrowDays": 14,
    "MaxBooksPerMember": 5
  }
}
```

### **Environment Variables (Production)**

```bash
Jwt__Secret=<min 32 char secret>
ConnectionStrings__Default=Data Source=/data/library.db
Library__MaxBorrowDays=14
Library__MaxBooksPerMember=5
```

---

## 🔍 Verify Installation

### Check localStorage Keys

```javascript
// Open browser console and run:
localStorage.getItem("LibraryAPI_accessToken");
localStorage.getItem("LibraryAPI_refreshToken");
localStorage.getItem("LibraryAPI_userInfo");
```

### Check Auth Status

```javascript
// Global API:
libraryAuth.printAuthStatus();
```

### Check Swagger Dark Mode

- Navigate to `http://localhost:5000/swagger`
- Verify dark background is applied
- Verify text is light colored

---

## 🚨 Troubleshooting

| Issue                | Solution                                                         |
| -------------------- | ---------------------------------------------------------------- |
| Tokens not saving    | Check if localStorage is enabled (not in private/incognito mode) |
| Page reload logs out | Verify browser localStorage is not cleared on exit               |
| Swagger not dark     | Clear browser cache (Ctrl+Shift+Delete) and refresh              |
| Login fails          | Verify server is running (`dotnet run`) and database updated     |
| 401 Unauthorized     | Token may be expired - try clicking "Refresh Token" button       |
| API calls fail       | Ensure you're logged in and have correct role permissions        |

---

## 📊 Project Statistics

| Metric              | Value                                                   |
| ------------------- | ------------------------------------------------------- |
| **Layers**          | 4 (Domain, Infrastructure, Application, API)            |
| **Controllers**     | 5 (Auth, Books, Authors, Members, Borrows)              |
| **DTOs**            | 30+ (Request/Response pairs)                            |
| **Database Tables** | 5 (Users, Authors, Books, BorrowRecords, RefreshTokens) |
| **Roles**           | 3 (Admin, Librarian, Member)                            |
| **API Endpoints**   | 25+                                                     |
| **Frontend Files**  | 4 (HTML, CSS, 2 JS)                                     |
| **Test Page**       | 1 (index.html with full UI)                             |
| **Documentation**   | 4 files (README, Architecture, TokenFlow, Auth)         |

---

## 🎯 What's New in This Version

✅ **localStorage Token Persistence**

- Tokens saved automatically after login
- Tokens restored on page reload
- Tokens cleared on logout
- Keys scoped with project name: `LibraryAPI_*`

✅ **Dark Mode Swagger UI**

- Professional dark theme
- Matches VS Code dark theme
- Better contrast for extended use

✅ **Interactive Auth Test Page**

- Login, register, logout UI
- Real-time status display
- localStorage inspection
- Console logging

✅ **Auto Token Refresh**

- Tokens refreshed every 50 minutes
- Seamless refresh without user interruption
- Works in background

✅ **Browser Console API**

- Global `libraryAuth` object
- All auth operations available
- Easy testing and debugging

---

## 🎓 Learning Resources

This project demonstrates:

1. **Clean Architecture** - Layered separation of concerns
2. **JWT Authentication** - Secure token-based auth
3. **Role-Based Access Control** - Permission management
4. **Entity Framework Core** - ORM with SQLite
5. **Dependency Injection** - Loose coupling & testability
6. **Design Patterns** - Repository, Unit of Work, Result, CQRS
7. **Data Structures & Algorithms** - Binary search, Priority queue
8. **RESTful API Design** - Standard HTTP conventions
9. **Frontend Integration** - localStorage, JWT tokens
10. **Dark Mode UI** - CSS styling best practices

---

## 📝 License

This project is for educational purposes.

---

## 🙏 Support

For issues or questions:

1. Check **LOCALSTORAGE_AUTH.md** for token persistence guide
2. Check **TOKEN_FLOW_DIAGRAM.md** for visual explanations
3. Check **ARCHITECTURE.md** for system design
4. Review browser console logs for error messages

---

**✅ Your complete Library Management API with token persistence is ready!**

**Get Started:** `dotnet run` → Visit `http://localhost:5000`

---

## 👨‍💻 Author

Seam
Full-stack Developer

---
