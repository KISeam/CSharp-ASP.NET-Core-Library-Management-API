/**
 * LibraryAPI Authentication Service with localStorage Token Persistence
 * ────────────────────────────────────────────────────────────────────
 * Features:
 * ✅ Save tokens to localStorage (persists across page reloads)
 * ✅ Restore tokens on page load
 * ✅ Clear tokens on logout
 * ✅ Auto-refresh expired tokens
 * ✅ Scoped keys with project name: LibraryAPI_accessToken, LibraryAPI_refreshToken
 */

const AUTH_CONFIG = {
  PROJECT_NAME: "LibraryAPI",
  API_BASE_URL: "http://localhost:5000/api",
  ACCESS_TOKEN_KEY: "LibraryAPI_accessToken",
  REFRESH_TOKEN_KEY: "LibraryAPI_refreshToken",
  USER_INFO_KEY: "LibraryAPI_userInfo",
  TOKEN_EXPIRY_KEY: "LibraryAPI_tokenExpiry",
};

class LibraryAuthService {
  constructor() {
    this.accessToken = this.loadAccessToken();
    this.refreshToken = this.loadRefreshToken();
    this.userInfo = this.loadUserInfo();
    this.setupTokenRefreshInterval();
  }

  // ──────────────────────────────────────────────────────
  // LOAD TOKENS FROM LOCALSTORAGE (On Page Load)
  // ──────────────────────────────────────────────────────
  loadAccessToken() {
    const token = localStorage.getItem(AUTH_CONFIG.ACCESS_TOKEN_KEY);
    return token || null;
  }

  loadRefreshToken() {
    const token = localStorage.getItem(AUTH_CONFIG.REFRESH_TOKEN_KEY);
    return token || null;
  }

  loadUserInfo() {
    try {
      const userJson = localStorage.getItem(AUTH_CONFIG.USER_INFO_KEY);
      return userJson ? JSON.parse(userJson) : null;
    } catch (e) {
      return null;
    }
  }

  // ──────────────────────────────────────────────────────
  // SAVE TOKENS TO LOCALSTORAGE (After Login)
  // ──────────────────────────────────────────────────────
  saveTokensToLocalStorage(accessToken, refreshToken, userInfo) {
    localStorage.setItem(AUTH_CONFIG.ACCESS_TOKEN_KEY, accessToken);
    localStorage.setItem(AUTH_CONFIG.REFRESH_TOKEN_KEY, refreshToken);
    localStorage.setItem(AUTH_CONFIG.USER_INFO_KEY, JSON.stringify(userInfo));

    this.accessToken = accessToken;
    this.refreshToken = refreshToken;
    this.userInfo = userInfo;

    console.log("✅ Tokens saved to localStorage (LibraryAPI_*)");
    this.printAuthStatus();
  }

  // ──────────────────────────────────────────────────────
  // CLEAR TOKENS FROM LOCALSTORAGE (On Logout)
  // ──────────────────────────────────────────────────────
  clearTokensFromLocalStorage() {
    localStorage.removeItem(AUTH_CONFIG.ACCESS_TOKEN_KEY);
    localStorage.removeItem(AUTH_CONFIG.REFRESH_TOKEN_KEY);
    localStorage.removeItem(AUTH_CONFIG.USER_INFO_KEY);
    localStorage.removeItem(AUTH_CONFIG.TOKEN_EXPIRY_KEY);

    this.accessToken = null;
    this.refreshToken = null;
    this.userInfo = null;

    console.log("✅ All tokens cleared from localStorage");
  }

  // ──────────────────────────────────────────────────────
  // REGISTER
  // ──────────────────────────────────────────────────────
  async register(firstName, lastName, email, password, phoneNumber = "") {
    try {
      const response = await fetch(
        `${AUTH_CONFIG.API_BASE_URL}/auth/register`,
        {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            firstName,
            lastName,
            email,
            password,
            phoneNumber,
          }),
        },
      );

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.error || "Registration failed");
      }

      this.saveTokensToLocalStorage(
        data.data.accessToken,
        data.data.refreshToken,
        data.data.user,
      );

      console.log(`✅ Registered as ${data.data.user.fullName}`);
      return { success: true, user: data.data.user };
    } catch (error) {
      console.error("❌ Register error:", error.message);
      return { success: false, error: error.message };
    }
  }

  // ──────────────────────────────────────────────────────
  // LOGIN
  // ──────────────────────────────────────────────────────
  async login(email, password) {
    try {
      const response = await fetch(`${AUTH_CONFIG.API_BASE_URL}/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password }),
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.error || "Login failed");
      }

      this.saveTokensToLocalStorage(
        data.data.accessToken,
        data.data.refreshToken,
        data.data.user,
      );

      console.log(
        `✅ Logged in as ${data.data.user.fullName} (${data.data.user.role})`,
      );
      return { success: true, user: data.data.user };
    } catch (error) {
      console.error("❌ Login error:", error.message);
      return { success: false, error: error.message };
    }
  }

  // ──────────────────────────────────────────────────────
  // LOGOUT - CLEAR ALL TOKENS
  // ──────────────────────────────────────────────────────
  async logout() {
    try {
      const refreshToken = this.loadRefreshToken();
      const accessToken = this.loadAccessToken();

      // Revoke token on backend (optional)
      if (refreshToken && accessToken) {
        try {
          await fetch(`${AUTH_CONFIG.API_BASE_URL}/auth/revoke`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
              Authorization: `Bearer ${accessToken}`,
            },
            body: JSON.stringify({ refreshToken }),
          });
        } catch (e) {
          console.warn("Could not revoke on server");
        }
      }

      // Clear localStorage
      this.clearTokensFromLocalStorage();

      console.log("✅ Logged out successfully");
      return { success: true };
    } catch (error) {
      console.error("❌ Logout error:", error.message);
      // Force clear even if error
      this.clearTokensFromLocalStorage();
      return { success: false, error: error.message };
    }
  }

  // ──────────────────────────────────────────────────────
  // REFRESH TOKEN
  // ──────────────────────────────────────────────────────
  async refreshAccessToken() {
    try {
      const refreshToken = this.loadRefreshToken();
      if (!refreshToken) {
        throw new Error("No refresh token available");
      }

      const response = await fetch(`${AUTH_CONFIG.API_BASE_URL}/auth/refresh`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ refreshToken }),
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.error || "Token refresh failed");
      }

      this.saveTokensToLocalStorage(
        data.data.accessToken,
        data.data.refreshToken,
        data.data.user,
      );

      console.log("✅ Access token refreshed");
      return { success: true, accessToken: data.data.accessToken };
    } catch (error) {
      console.error("❌ Token refresh failed:", error.message);
      this.clearTokensFromLocalStorage();
      return { success: false, error: error.message };
    }
  }

  // ──────────────────────────────────────────────────────
  // AUTO-REFRESH INTERVAL (Every 50 minutes)
  // ──────────────────────────────────────────────────────
  setupTokenRefreshInterval() {
    setInterval(
      async () => {
        if (this.isAuthenticated()) {
          await this.refreshAccessToken();
        }
      },
      50 * 60 * 1000,
    ); // 50 minutes
  }

  // ──────────────────────────────────────────────────────
  // CHECK IF AUTHENTICATED
  // ──────────────────────────────────────────────────────
  isAuthenticated() {
    return !!(this.loadAccessToken() && this.loadUserInfo());
  }

  // ──────────────────────────────────────────────────────
  // GET AUTHORIZATION HEADER
  // ──────────────────────────────────────────────────────
  getAuthHeader() {
    const token = this.loadAccessToken();
    return token ? { Authorization: `Bearer ${token}` } : {};
  }

  // ──────────────────────────────────────────────────────
  // GET USER ROLE
  // ──────────────────────────────────────────────────────
  getUserRole() {
    const userInfo = this.loadUserInfo();
    return userInfo ? userInfo.role : null;
  }

  // ──────────────────────────────────────────────────────
  // GET USER INFO
  // ──────────────────────────────────────────────────────
  getUser() {
    return this.loadUserInfo();
  }

  // ──────────────────────────────────────────────────────
  // CHANGE PASSWORD
  // ──────────────────────────────────────────────────────
  async changePassword(currentPassword, newPassword) {
    try {
      const response = await fetch(
        `${AUTH_CONFIG.API_BASE_URL}/auth/password`,
        {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
            ...this.getAuthHeader(),
          },
          body: JSON.stringify({
            currentPassword,
            newPassword,
          }),
        },
      );

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.error || "Password change failed");
      }

      console.log("✅ Password changed successfully");
      return { success: true };
    } catch (error) {
      console.error("❌ Change password error:", error.message);
      return { success: false, error: error.message };
    }
  }

  // ──────────────────────────────────────────────────────
  // API REQUEST WITH AUTO-REFRESH
  // ──────────────────────────────────────────────────────
  async apiRequest(endpoint, options = {}) {
    const headers = {
      "Content-Type": "application/json",
      ...this.getAuthHeader(),
      ...options.headers,
    };

    let response = await fetch(`${AUTH_CONFIG.API_BASE_URL}${endpoint}`, {
      ...options,
      headers,
    });

    // If 401 and we have refresh token, try refreshing
    if (response.status === 401 && this.loadRefreshToken()) {
      const refreshResult = await this.refreshAccessToken();
      if (refreshResult.success) {
        headers["Authorization"] = `Bearer ${this.loadAccessToken()}`;
        response = await fetch(`${AUTH_CONFIG.API_BASE_URL}${endpoint}`, {
          ...options,
          headers,
        });
      }
    }

    return response;
  }

  // ──────────────────────────────────────────────────────
  // PRINT AUTH STATUS
  // ──────────────────────────────────────────────────────
  printAuthStatus() {
    console.log(
      "%c═══════ LibraryAPI AUTH STATUS ═══════",
      "color: #007acc; font-weight: bold; font-size: 14px;",
    );
    console.log(`%c✓ Project: ${AUTH_CONFIG.PROJECT_NAME}`, "color: #4ec9b0");
    console.log(
      `%c✓ Authenticated: ${this.isAuthenticated() ? "✅ YES" : "❌ NO"}`,
      this.isAuthenticated() ? "color: #22c55e" : "color: #ef4444",
    );
    console.log(
      `%c✓ User: ${this.loadUserInfo()?.fullName || "Not logged in"}`,
      "color: #4ec9b0",
    );
    console.log(
      `%c✓ Role: ${this.loadUserInfo()?.role || "N/A"}`,
      "color: #4ec9b0",
    );
    console.log(
      `%c✓ Access Token: ${this.loadAccessToken() ? "✅ Stored in localStorage" : "❌ Not stored"}`,
      this.loadAccessToken() ? "color: #22c55e" : "color: #ef4444",
    );
    console.log(
      `%c✓ Refresh Token: ${this.loadRefreshToken() ? "✅ Stored in localStorage" : "❌ Not stored"}`,
      this.loadRefreshToken() ? "color: #22c55e" : "color: #ef4444",
    );
    console.log(
      `%c✓ Storage Keys: LibraryAPI_accessToken, LibraryAPI_refreshToken, LibraryAPI_userInfo`,
      "color: #f59e0b",
    );
    console.log(
      "%c═════════════════════════════════════",
      "color: #007acc; font-weight: bold; font-size: 14px;",
    );
  }
}

// Create and expose global instance
const libraryAuth = new LibraryAuthService();

// Auto-print status on page load
if (typeof window !== "undefined") {
  window.addEventListener("load", () => {
    libraryAuth.printAuthStatus();
  });
}

// Expose globally
if (typeof window !== "undefined") {
  window.libraryAuth = libraryAuth;
}
