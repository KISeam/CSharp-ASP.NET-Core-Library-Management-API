# 🔐 LocalStorage Token Persistence System

This guide explains how the authentication tokens are now persisted in localStorage and how to use them.

---

## ✨ Features Implemented

### 1️⃣ **LocalStorage Token Persistence**

- ✅ Tokens are automatically saved to localStorage after login
- ✅ Tokens persist across page reloads (no token loss)
- ✅ Scoped keys with project name: `LibraryAPI_accessToken`, `LibraryAPI_refreshToken`, `LibraryAPI_userInfo`

### 2️⃣ **Automatic Token Restoration**

- ✅ On page load, tokens are automatically loaded from localStorage
- ✅ User authentication state is preserved across sessions

### 3️⃣ **Logout Token Removal**

- ✅ On logout, all tokens are cleared from localStorage
- ✅ Backend token revocation (optional endpoint call)

### 4️⃣ **Dark Mode Swagger UI**

- ✅ Swagger UI now has a professional dark theme
- ✅ Better contrast and easier on the eyes
- ✅ Modern color scheme matching VS Code dark theme

---

## 🚀 How to Use

### **Step 1: Start the Project**

```bash
cd "/media/md-seam/New_Volume4/C# Projects/Web API Project/Library Management API"
dotnet run
```

### **Step 2: Access the Auth Test Page**

Open browser: **`http://localhost:5000`**

This will load the interactive auth testing page where you can:

- Login with existing credentials
- Register new members
- View token status
- Test logout (tokens cleared from localStorage)

### **Step 3: Access Swagger UI (Dark Mode)**

Navigate to: **`http://localhost:5000/swagger`**

The Swagger UI now has:

- 🌙 Dark theme automatically applied
- 📦 Tokens pre-loaded from localStorage
- ✨ Better readability

---

## 📦 LocalStorage Keys

All tokens are saved with the **LibraryAPI** prefix:

| Key                       | Value            | Purpose                                   |
| ------------------------- | ---------------- | ----------------------------------------- |
| `LibraryAPI_accessToken`  | JWT token        | Used for API authorization (Bearer token) |
| `LibraryAPI_refreshToken` | Refresh token    | Used to get new access token when expired |
| `LibraryAPI_userInfo`     | User JSON object | User details: id, fullName, email, role   |

---

## 🔄 Authentication Flow

### **Login:**

```
1. User enters email & password
   ↓
2. API validates credentials
   ↓
3. API returns accessToken + refreshToken + userInfo
   ↓
4. Tokens saved to localStorage
   ↓
5. User is authenticated ✅
```

### **Page Reload:**

```
1. User refreshes page
   ↓
2. Auth service loads tokens from localStorage
   ↓
3. User is still authenticated ✅
   ↓
4. No re-login needed
```

### **Logout:**

```
1. User clicks logout
   ↓
2. All tokens cleared from localStorage
   ↓
3. Backend token revoked (optional)
   ↓
4. User is logged out ✅
```

---

## 💻 JavaScript API

### **Check Authentication Status**

```javascript
// Global instance available in console/scripts
libraryAuth.isAuthenticated(); // true or false
```

### **Get Current User**

```javascript
libraryAuth.getUser();
// Returns: { id: 1, fullName: "John Doe", email: "john@example.com", role: "Admin" }
```

### **Get User Role**

```javascript
libraryAuth.getUserRole(); // "Admin", "Librarian", or "Member"
```

### **Login**

```javascript
const result = await libraryAuth.login("admin@library.com", "Admin@123");
if (result.success) {
  console.log(result.user);
}
```

### **Register**

```javascript
const result = await libraryAuth.register(
  "John", // firstName
  "Doe", // lastName
  "john@example.com", // email
  "Password@123", // password
  "+880123456789", // phoneNumber (optional)
);
```

### **Logout (Clears Tokens)**

```javascript
await libraryAuth.logout();
// All localStorage keys cleared ✅
```

### **Refresh Token**

```javascript
const result = await libraryAuth.refreshAccessToken();
if (result.success) {
  console.log("New token:", result.accessToken);
}
```

### **Get Authorization Header**

```javascript
const headers = libraryAuth.getAuthHeader();
// Returns: { Authorization: "Bearer eyJhbGc..." }
```

### **Print Auth Status to Console**

```javascript
libraryAuth.printAuthStatus();
// Prints detailed auth status to browser console
```

---

## 📋 Test Scenarios

### **Test 1: Login & Page Reload**

1. Go to http://localhost:5000
2. Click "Login" button (default: admin@library.com / Admin@123)
3. Verify "✅ YES" shows in "Authenticated" field
4. **Refresh page** (F5 or Ctrl+R)
5. ✅ Tokens still there! User still authenticated
6. Check localStorage: `LibraryAPI_accessToken` exists

### **Test 2: Logout Clears Tokens**

1. After login, click "Logout" button
2. Verify "❌ NO" shows in "Authenticated" field
3. Check localStorage: All `LibraryAPI_*` keys removed
4. Try to refresh page - still logged out ✅

### **Test 3: Register New Member**

1. Fill registration form with:
   - First Name: Test
   - Last Name: User
   - Email: test@example.com
   - Password: TestPass@123
2. Click "Register"
3. ✅ Tokens saved automatically
4. Refresh page - still authenticated as new member

### **Test 4: Open Swagger UI (Dark Mode)**

1. Login first at http://localhost:5000
2. Go to http://localhost:5000/swagger
3. ✅ Dark theme applied
4. Token status badge shows in top-right corner
5. Try API endpoints - auth works!

---

## 🔧 How It Works (Technical Details)

### **File Structure:**

```
src/API/wwwroot/
├── index.html                    # Auth test page
├── auth-service.js               # Main auth service (localStorage logic)
├── swagger-persistence.js        # Auto-inject tokens to Swagger
└── swagger-ui-dark.css           # Dark mode theme
```

### **auth-service.js Features:**

- **saveTokensToLocalStorage()** - Store tokens in localStorage
- **loadAccessToken()** - Retrieve access token on page load
- **clearTokensFromLocalStorage()** - Remove all tokens on logout
- **isAuthenticated()** - Check if user is logged in
- **refreshAccessToken()** - Auto-refresh when expired
- **getAuthHeader()** - Get Bearer token for API calls

### **Program.cs Changes:**

```csharp
// Added static files support
app.UseStaticFiles();

// Inject dark mode CSS & persistence script
c.InjectStylesheet("/swagger-ui-dark.css");
c.InjectJavascript("/swagger-persistence.js");
```

---

## 🌙 Dark Mode Theme

The Swagger UI now has:

- **Background:** Dark blue (#1e1e1e)
- **Text:** Light gray (#e0e0e0)
- **Accent:** Cyan blue (#0ea5e9)
- **Borders:** Medium gray (#3e3e42)

Matches the theme of:

- VS Code dark mode
- Modern dev tools (Chrome DevTools, etc.)

---

## 🛡️ Security Considerations

### **What's Secure:**

- ✅ Tokens use BCrypt hashing (password)
- ✅ JWT tokens are cryptographically signed
- ✅ Refresh token rotation on use
- ✅ CORS enabled for your domain only

### **What's Not Secure (Client-Side):**

- ⚠️ localStorage is vulnerable to XSS attacks
- ⚠️ Don't store sensitive data besides tokens
- ⚠️ Use HTTPS in production
- ⚠️ Set appropriate CORS origin in production

### **Production Recommendations:**

```csharp
// In Program.cs for production:
builder.Services.AddCorsPolicy()  // Restrict to your domain
  .WithOrigins("https://yourdomain.com")

// Enable HTTPS redirect
app.UseHttpsRedirection();
```

---

## 🧪 Testing with cURL

### **Login & Save Token:**

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@library.com","password":"Admin@123"}' \
  | jq .data.accessToken
```

### **Use Token in Request:**

```bash
TOKEN="eyJhbGc..."  # Token from login

curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:5000/api/books
```

---

## 🐛 Troubleshooting

### **Issue: Tokens not persisting**

**Solution:**

- Check browser console for errors
- Verify localStorage is enabled (not in private/incognito mode)
- Check Application tab → Storage → Local Storage in DevTools

### **Issue: Swagger UI not in dark mode**

**Solution:**

- Clear browser cache (Ctrl+Shift+Delete)
- Check browser console for CSS load errors
- Verify `/swagger-ui-dark.css` is loading

### **Issue: Login fails**

**Solution:**

- Verify server is running: `dotnet run`
- Check backend logs for errors
- Try default credentials: `admin@library.com` / `Admin@123`

### **Issue: Page reloads and tokens disappear**

**Solution (Before Fix):**

- This should NOT happen with new system
- If it does, check:
  1. Browser localStorage is enabled
  2. No errors in browser console
  3. localStorage quota not exceeded

---

## 📚 Files Created/Modified

### **New Files:**

- ✅ `src/API/wwwroot/index.html` - Auth test page (15KB)
- ✅ `src/API/wwwroot/auth-service.js` - Auth service (12KB)
- ✅ `src/API/wwwroot/swagger-persistence.js` - Swagger integration (5KB)
- ✅ `src/API/wwwroot/swagger-ui-dark.css` - Dark mode theme (6KB)

### **Modified Files:**

- ✅ `src/API/Program.cs` - Added static files & dark mode config

---

## ✅ Checklist

- ✅ Tokens saved to localStorage with project name prefix
- ✅ Tokens persist across page reloads
- ✅ Tokens cleared on logout
- ✅ Swagger UI dark mode enabled
- ✅ Auth test page created
- ✅ Auto-token refresh implemented
- ✅ Console logging for debugging

---

## 🎯 Next Steps

1. **Run the project:** `dotnet run`
2. **Test auth:** Go to http://localhost:5000
3. **Try Swagger:** Go to http://localhost:5000/swagger
4. **Open DevTools:** F12 → Application → Local Storage
5. **See localStorage keys:** `LibraryAPI_accessToken`, `LibraryAPI_refreshToken`

---

**🚀 Your authentication system is now ready with persistent localStorage tokens!**
