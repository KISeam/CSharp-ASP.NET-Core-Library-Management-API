/**
 * Swagger UI Persistence Script
 * Automatically loads and injects tokens into Swagger UI
 */

(function () {
  const STORAGE_KEY = "LibraryAPI_accessToken";
  const USER_INFO_KEY = "LibraryAPI_userInfo";
  const REFRESH_KEY = "LibraryAPI_refreshToken";

  // Wait for window to load completely
  window.addEventListener('load', function () {
    // Wait for Swagger UI to be initialized and window.ui to be available
    let attempts = 0;
    const maxAttempts = 50;

    const interval = setInterval(function () {
      attempts++;
      if (window.ui && window.ui.getStore) {
        clearInterval(interval);
        initSwaggerPersistence(window.ui);
      } else if (attempts >= maxAttempts) {
        clearInterval(interval);
        console.warn('⚠️ Swagger UI window.ui not found after maximum attempts');
      }
    }, 100);
  });

  function initSwaggerPersistence(ui) {
    const store = ui.getStore();

    // 1. Try to restore token from localStorage on page load
    const savedToken = localStorage.getItem(STORAGE_KEY);
    if (savedToken) {
      console.log('✅ Restoring LibraryAPI authorization from localStorage...');
      store.dispatch(ui.authActions.authorize({
        Bearer: {
          name: "Bearer",
          schema: {
            type: "apiKey",
            in: "header",
            name: "Authorization"
          },
          value: savedToken
        }
      }));

      // Render the auth badge if user info is present
      const userInfo = loadUserInfo();
      showAuthStatusInSwagger(userInfo, savedToken);
    }

    // 2. Subscribe to store changes to keep localStorage in sync
    let lastAuthorizedState = null;

    store.subscribe(() => {
      try {
        const state = store.getState();
        const auth = state.get("auth");
        if (!auth) return;

        const authorized = auth.get("authorized");
        if (authorized === lastAuthorizedState) return;

        lastAuthorizedState = authorized;

        if (authorized && authorized.size > 0) {
          const authJS = authorized.toJS();
          let tokenValue = null;
          for (const key in authJS) {
            if (authJS[key] && authJS[key].value) {
              tokenValue = authJS[key].value;
              break;
            }
          }

          if (tokenValue) {
            // Strip "Bearer " prefix if user accidentally pasted it in
            if (tokenValue.toLowerCase().startsWith("bearer ")) {
              tokenValue = tokenValue.substring(7).trim();
            }

            const currentSaved = localStorage.getItem(STORAGE_KEY);
            if (currentSaved !== tokenValue) {
              console.log('✅ Token authorized in Swagger. Saving to localStorage...');
              localStorage.setItem(STORAGE_KEY, tokenValue);

              // Decode JWT payload to sync user info for the badge
              const userInfo = decodeJwtPayload(tokenValue);
              if (userInfo) {
                localStorage.setItem(USER_INFO_KEY, JSON.stringify(userInfo));
              }

              showAuthStatusInSwagger(userInfo || loadUserInfo(), tokenValue);
            }
          }
        } else {
          // User logged out from Swagger UI
          if (localStorage.getItem(STORAGE_KEY)) {
            console.log('❌ Token removed in Swagger. Clearing localStorage...');
            localStorage.removeItem(STORAGE_KEY);
            localStorage.removeItem(USER_INFO_KEY);
            localStorage.removeItem(REFRESH_KEY);

            const existingBadge = document.getElementById('auth-status-badge');
            if (existingBadge) existingBadge.remove();
          }
        }
      } catch (err) {
        console.error('Error handling Swagger authorization synchronization:', err);
      }
    });
  }

  function loadUserInfo() {
    try {
      const userJson = localStorage.getItem(USER_INFO_KEY);
      return userJson ? JSON.parse(userJson) : null;
    } catch (e) {
      return null;
    }
  }

  function decodeJwtPayload(token) {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(atob(base64).split('').map(function (c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
      }).join(''));

      const payload = JSON.parse(jsonPayload);

      // Extract standard claims
      const userId = payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] || payload.sub;
      const fullName = payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || payload.unique_name || "Swagger User";
      const email = payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] || payload.email;
      const role = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || payload.role;

      return { id: userId, fullName, email, role };
    } catch (e) {
      console.warn("Failed to decode JWT payload", e);
      return null;
    }
  }

  function showAuthStatusInSwagger(user, token) {
    // Create auth status badge
    const existingBadge = document.getElementById('auth-status-badge');
    if (existingBadge) existingBadge.remove();

    const badge = document.createElement('div');
    badge.id = 'auth-status-badge';
    badge.style.cssText = `
      position: fixed;
      top: 20px;
      right: 20px;
      padding: 12px 20px;
      background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%);
      border: 2px solid #0ea5e9;
      border-radius: 8px;
      color: #e0e0e0;
      font-size: 13px;
      font-weight: 600;
      z-index: 9999;
      box-shadow: 0 4px 12px rgba(14, 165, 233, 0.2);
      font-family: 'Courier New', monospace;
    `;

    const statusText = user
      ? `✅ Logged in as <strong>${user.fullName}</strong> (${user.role})`
      : '⚠️ Token loaded successfully';

    badge.innerHTML = `
      <div style="margin-bottom: 8px; color: #0ea5e9;">LibraryAPI Auth Status</div>
      <div style="font-size: 12px; color: #a0a0a0;">${statusText}</div>
      <div style="font-size: 11px; color: #64748b; margin-top: 6px;">
        📦 Token: localStorage.LibraryAPI_accessToken
      </div>
    `;

    document.body.appendChild(badge);

    // Remove badge after 8 seconds
    setTimeout(function () {
      if (badge && badge.parentElement) {
        badge.style.opacity = '0';
        badge.style.transition = 'opacity 0.5s ease';
        setTimeout(function () {
          if (badge && badge.parentElement) {
            badge.parentElement.removeChild(badge);
          }
        }, 500);
      }
    }, 8000);
  }
})();
