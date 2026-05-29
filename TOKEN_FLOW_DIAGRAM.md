# 🔐 LocalStorage Token Flow Diagram

## 1. LOGIN FLOW

```
┌─────────────────────────────────────────────────────────────┐
│                    User Clicks Login                        │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
        ┌────────────────────────────────┐
        │   Send email + password        │
        │   to /api/auth/login           │
        └────────────┬───────────────────┘
                     │
                     ▼
        ┌────────────────────────────────┐
        │  Backend validates & returns:  │
        │  - accessToken (JWT)           │
        │  - refreshToken               │
        │  - userInfo                   │
        └────────────┬───────────────────┘
                     │
                     ▼
        ┌────────────────────────────────┐
        │   Save to localStorage:        │
        │ • LibraryAPI_accessToken      │
        │ • LibraryAPI_refreshToken     │
        │ • LibraryAPI_userInfo         │
        └────────────┬───────────────────┘
                     │
                     ▼
        ┌────────────────────────────────┐
        │  User authenticated ✅         │
        │  Page shows user info         │
        └────────────────────────────────┘
```

---

## 2. PAGE RELOAD FLOW

```
┌─────────────────────────────────────────────────────────────┐
│              User Refreshes Page (F5)                       │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
        ┌────────────────────────────────┐
        │  auth-service.js loads:        │
        │  - Read localStorage keys      │
        │  - Load accessToken            │
        │  - Load refreshToken           │
        │  - Load userInfo               │
        └────────────┬───────────────────┘
                     │
                     ▼
        ┌────────────────────────────────┐
        │  Tokens found in localStorage  │
        │  ✅ User still authenticated   │
        └────────────┬───────────────────┘
                     │
                     ▼
        ┌────────────────────────────────┐
        │  Page displays:                │
        │  - User name & role            │
        │  - Auth status: ✅ YES         │
        │  - localStorage indicator      │
        └────────────────────────────────┘
```

**KEY POINT:** Tokens are NOT removed on page reload! ✅

---

## 3. LOGOUT FLOW

```
┌─────────────────────────────────────────────────────────────┐
│               User Clicks Logout Button                     │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
        ┌────────────────────────────────┐
        │  Send token revocation to:     │
        │  /api/auth/revoke              │
        │  (optional, best practice)     │
        └────────────┬───────────────────┘
                     │
                     ▼
        ┌────────────────────────────────┐
        │  Clear localStorage:           │
        │  - removeItem(LibraryAPI_*)   │
        │  - All tokens removed          │
        └────────────┬───────────────────┘
                     │
                     ▼
        ┌────────────────────────────────┐
        │  Reset in-memory variables:    │
        │  - accessToken = null          │
        │  - refreshToken = null         │
        │  - userInfo = null             │
        └────────────┬───────────────────┘
                     │
                     ▼
        ┌────────────────────────────────┐
        │  User logged out ✅            │
        │  Page shows:                   │
        │  - Auth status: ❌ NO          │
        │  - localStorage indicator: ❌  │
        └────────────────────────────────┘
```

**KEY POINT:** Tokens are completely removed from localStorage! ✅

---

## 4. TOKEN REFRESH FLOW

```
┌─────────────────────────────────────────────────────────────┐
│           Auto Token Refresh (Every 50 minutes)            │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
        ┌────────────────────────────────┐
        │  Check if authenticated        │
        │  Every 50 minutes              │
        └────────────┬───────────────────┘
                     │
                     ▼
        ┌────────────────────────────────┐
        │  Send refreshToken to:         │
        │  /api/auth/refresh             │
        └────────────┬───────────────────┘
                     │
                     ▼
        ┌────────────────────────────────┐
        │  Backend returns new:          │
        │  - accessToken (new JWT)       │
        │  - refreshToken (new)          │
        │  - userInfo (updated)          │
        └────────────┬───────────────────┘
                     │
                     ▼
        ┌────────────────────────────────┐
        │  Update localStorage:          │
        │  - New access token            │
        │  - New refresh token           │
        └────────────┬───────────────────┘
                     │
                     ▼
        ┌────────────────────────────────┐
        │  Continue using new tokens ✅  │
        └────────────────────────────────┘
```

---

## 5. API REQUEST FLOW

```
┌─────────────────────────────────────────────────────────────┐
│       Make API Request (e.g., GET /api/books)              │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
        ┌────────────────────────────────┐
        │  Get token from localStorage   │
        │  libraryAuth.getAuthHeader()   │
        └────────────┬───────────────────┘
                     │
                     ▼
        ┌────────────────────────────────┐
        │  Add to request header:        │
        │  Authorization: Bearer <token> │
        └────────────┬───────────────────┘
                     │
                     ▼
        ┌────────────────────────────────┐
        │  Send to API                   │
        └────────────┬───────────────────┘
                     │
         ┌───────────┴───────────┐
         │                       │
         ▼                       ▼
    ✅ 200 OK             ❌ 401 Unauthorized
    Return data           (Token expired)
                                 │
                                 ▼
                     ┌────────────────────────────┐
                     │  Try refresh token         │
                     │  /api/auth/refresh         │
                     └────────────┬───────────────┘
                                  │
                         ┌────────┴────────┐
                         │                 │
                         ▼                 ▼
                     ✅ Got new       ❌ Refresh failed
                     token            Clear tokens
                                     Redirect to login
```

---

## 6. LOCALSTORAGE STRUCTURE

```
┌────────────────────────────────────────────────────┐
│           Browser LocalStorage                    │
├────────────────────────────────────────────────────┤
│                                                    │
│  LibraryAPI_accessToken:                          │
│  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."       │
│  (JWT Token - 60 minute expiry)                    │
│                                                    │
│  LibraryAPI_refreshToken:                         │
│  "abc123def456ghi789jkl012mno345pqr..."          │
│  (Refresh Token - 7 day expiry)                    │
│                                                    │
│  LibraryAPI_userInfo:                             │
│  {                                                │
│    "id": 1,                                       │
│    "fullName": "System Admin",                    │
│    "email": "admin@library.com",                  │
│    "role": "Admin"                                │
│  }                                                │
│                                                    │
└────────────────────────────────────────────────────┘
```

---

## 7. FILE STRUCTURE

```
src/API/wwwroot/
├── index.html                    ← Auth test page
│   └─ Contains login/register/logout forms
│   └─ Shows real-time auth status
│   └─ Displays localStorage contents
│
├── auth-service.js               ← Main auth service
│   ├─ login()
│   ├─ register()
│   ├─ logout()  ← Clears localStorage
│   ├─ saveTokensToLocalStorage()
│   ├─ loadAccessToken()
│   ├─ isAuthenticated()
│   └─ getAuthHeader()
│
├── swagger-persistence.js        ← Swagger integration
│   └─ Auto-injects tokens to Swagger UI
│   └─ Shows auth status badge
│
└── swagger-ui-dark.css           ← Dark mode theme
    └─ Professional dark styling
```

---

## 8. COMPONENT INTERACTION

```
┌──────────────────────────────────────────────────────────┐
│                   Browser                               │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  ┌──────────────────────────────────────────────────┐  │
│  │  index.html (UI)                                 │  │
│  │  ├─ Login form                                   │  │
│  │  ├─ Register form                                │  │
│  │  └─ Status displays                              │  │
│  └────────┬─────────────────────────────────────────┘  │
│           │                                              │
│           ▼                                              │
│  ┌──────────────────────────────────────────────────┐  │
│  │  auth-service.js (Logic)                         │  │
│  │  ├─ Manages tokens                               │  │
│  │  ├─ Calls API endpoints                          │  │
│  │  └─ Updates localStorage                         │  │
│  └────────┬─────────────────────────────────────────┘  │
│           │                                              │
│           ▼                                              │
│  ┌──────────────────────────────────────────────────┐  │
│  │  LocalStorage                                    │  │
│  │  (Persistent across refreshes)                   │  │
│  │  - LibraryAPI_accessToken                        │  │
│  │  - LibraryAPI_refreshToken                       │  │
│  │  - LibraryAPI_userInfo                           │  │
│  └──────────────────────────────────────────────────┘  │
│                                                          │
└────────────────────┬─────────────────────────────────────┘
                     │
                     ▼
        ┌────────────────────────────┐
        │  API Server (Backend)      │
        │  - /api/auth/login         │
        │  - /api/auth/register      │
        │  - /api/auth/logout        │
        │  - /api/auth/refresh       │
        │  - /api/books (protected)  │
        │  - etc.                    │
        └────────────────────────────┘
```

---

## Key Points

| Feature          | How It Works                                         |
| ---------------- | ---------------------------------------------------- |
| **Login**        | Tokens saved to localStorage automatically           |
| **Page Reload**  | Tokens loaded from localStorage - no re-login needed |
| **Logout**       | All tokens cleared from localStorage                 |
| **Persistence**  | Tokens survive browser refresh/close/reopen          |
| **Dark Mode**    | CSS injected into Swagger UI                         |
| **Auto Refresh** | Tokens refreshed every 50 minutes                    |

---

**✅ Your complete token persistence system!**
