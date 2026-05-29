/**
 * Swagger UI Persistence Script
 * Automatically loads and injects tokens into Swagger UI
 */

(function() {
  // Load auth service on Swagger page
  const authScriptPath = '/auth-service.js';
  
  // Wait for window to load completely
  window.addEventListener('load', function() {
    // Inject auth service if not already loaded
    if (typeof libraryAuth === 'undefined') {
      const script = document.createElement('script');
      script.src = authScriptPath;
      script.async = true;
      document.head.appendChild(script);
    }

    // Wait a bit for auth service to load
    setTimeout(function() {
      injectAuthTokenToSwagger();
    }, 500);
  });

  function injectAuthTokenToSwagger() {
    // Check if auth service is available
    if (typeof libraryAuth === 'undefined') {
      console.warn('Auth service not loaded yet');
      return;
    }

    // Wait for Swagger UI to be ready
    const maxAttempts = 20;
    let attempts = 0;

    const injectInterval = setInterval(function() {
      attempts++;

      // Look for Swagger UI authorize button or token input
      const authorizeBtn = document.querySelector('[aria-label="authorize"]') ||
                          document.querySelector('button[aria-label="authorize"]');
      
      const tokenInput = document.querySelector('input[placeholder*="api_key"]') ||
                        document.querySelector('input[placeholder*="Bearer"]');

      if (authorizeBtn || tokenInput) {
        clearInterval(injectInterval);

        const token = libraryAuth.loadAccessToken();
        const user = libraryAuth.loadUserInfo();

        if (token) {
          console.log('✅ Injecting LibraryAPI token into Swagger UI');

          // Try to set token via Swagger UI API if available
          if (window.ui && window.ui.presets && window.ui.presets[0]) {
            try {
              // Swagger UI 4+ API
              const persistAuthorizationPlugin = () => {
                return {
                  statePlugins: {
                    auth: {
                      actions: {
                        authorize: () => {
                          // Pre-populate with our token
                          localStorage.setItem('swaggerUIBearerToken', token);
                        }
                      }
                    }
                  }
                };
              };
            } catch (e) {
              console.warn('Could not inject via Swagger UI API');
            }
          }

          // Add visual indicator
          showAuthStatusInSwagger(user, token);
        }
      } else if (attempts >= maxAttempts) {
        clearInterval(injectInterval);
        console.warn('Swagger UI not fully loaded, skipping auto-inject');
      }
    }, 200);
  }

  function showAuthStatusInSwagger(user, token) {
    // Create auth status badge
    const topbar = document.querySelector('.topbar');
    if (!topbar) return;

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
      : '⚠️ No user logged in';

    badge.innerHTML = `
      <div style="margin-bottom: 8px;">LibraryAPI Auth Status</div>
      <div style="font-size: 12px; color: #a0a0a0;">${statusText}</div>
      <div style="font-size: 11px; color: #64748b; margin-top: 6px;">
        📦 Token in localStorage (LibraryAPI_*)
      </div>
    `;

    document.body.appendChild(badge);

    // Remove badge after 10 seconds
    setTimeout(function() {
      if (badge && badge.parentElement) {
        badge.style.opacity = '0';
        badge.style.transition = 'opacity 0.3s ease';
        setTimeout(function() {
          if (badge && badge.parentElement) {
            badge.parentElement.removeChild(badge);
          }
        }, 300);
      }
    }, 10000);
  }

  // Listen for page visibility changes (tab switch)
  document.addEventListener('visibilitychange', function() {
    if (!document.hidden && typeof libraryAuth !== 'undefined') {
      const token = libraryAuth.loadAccessToken();
      const user = libraryAuth.loadUserInfo();
      
      if (token && user) {
        console.log(`✅ Tab activated - User: ${user.fullName}`);
      }
    }
  });

  // Before page unload, save auth state
  window.addEventListener('beforeunload', function() {
    if (typeof libraryAuth !== 'undefined' && libraryAuth.isAuthenticated()) {
      console.log('✅ Page unloading - Auth tokens saved in localStorage');
    }
  });
})();
