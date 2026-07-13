// ================================================================
// IMTS Authentication Helper
// ================================================================

(function() {
    'use strict';

    // ===== Configuration =====
    const SESSION_TIMEOUT = 15 * 60 * 1000; // 15 minutes
    const WARNING_BEFORE = 60; // seconds before timeout to show warning

    // ===== Session Management =====
    let sessionTimer = null;
    let countdownInterval = null;
    let warningShown = false;

    // ===== Get Current User =====
    function getCurrentUser() {
        try {
            const session = localStorage.getItem('imts_session');
            if (session) {
                return JSON.parse(session);
            }
            return null;
        } catch (e) {
            return null;
        }
    }

    // ===== Check if User is Logged In =====
    function isLoggedIn() {
        const session = localStorage.getItem('imts_session');
        const sessionStart = localStorage.getItem('imts_session_start');
        
        if (!session || !sessionStart) {
            return false;
        }
        
        try {
            const elapsed = Date.now() - parseInt(sessionStart);
            return elapsed < SESSION_TIMEOUT;
        } catch (e) {
            return false;
        }
    }

    // ===== Get User Role =====
    function getUserRole() {
        const user = getCurrentUser();
        return user ? user.role : null;
    }

    // ===== Get User Name =====
    function getUserName() {
        const user = getCurrentUser();
        return user ? user.name : null;
    }

    // ================================================================
    // ✅ LOGOUT FUNCTION - Clears session and redirects to login
    // ================================================================
    function logout() {
        // Clear all session data
        localStorage.removeItem('imts_session');
        localStorage.removeItem('imts_user_role');
        localStorage.removeItem('imts_session_start');
        sessionStorage.clear();
        
        // Clear session timers
        if (sessionTimer) {
            clearTimeout(sessionTimer);
            sessionTimer = null;
        }
        if (countdownInterval) {
            clearInterval(countdownInterval);
            countdownInterval = null;
        }
        warningShown = false;
        
        // Redirect to login page with logout parameter
        window.location.href = '../login.html?logout=true';
    }

    // ================================================================
    // ✅ GLOBAL LOGOUT - Exposed to window for use anywhere
    // ================================================================
    window.logout = logout;
    window.IMTS = window.IMTS || {};
    window.IMTS.logout = logout;

    // ================================================================
    // EXTEND SESSION (for timeout warning)
    // ================================================================
    function extendSession() {
        if (isLoggedIn()) {
            const sessionStart = Date.now().toString();
            localStorage.setItem('imts_session_start', sessionStart);
            
            // Restart timer
            if (sessionTimer) {
                clearTimeout(sessionTimer);
                sessionTimer = null;
            }
            if (countdownInterval) {
                clearInterval(countdownInterval);
                countdownInterval = null;
            }
            
            // Hide warning banner if exists
            const banner = document.getElementById('sessionTimeoutBanner');
            if (banner) {
                banner.classList.add('hidden');
            }
            
            startSessionTimer();
            return true;
        }
        return false;
    }

    // ===== Start Session Timer =====
    function startSessionTimer() {
        clearTimeout(sessionTimer);
        clearInterval(countdownInterval);
        warningShown = false;

        const banner = document.getElementById('sessionTimeoutBanner');
        const countdown = document.getElementById('timeoutCountdown');
        
        if (banner) {
            banner.classList.add('hidden');
        }

        sessionTimer = setTimeout(function() {
            expireSession();
        }, SESSION_TIMEOUT);

        let secondsLeft = Math.floor(SESSION_TIMEOUT / 1000);
        countdownInterval = setInterval(function() {
            secondsLeft--;
            if (secondsLeft <= WARNING_BEFORE && !warningShown && isLoggedIn()) {
                warningShown = true;
                if (banner) {
                    banner.classList.remove('hidden');
                }
                if (countdown) {
                    countdown.textContent = secondsLeft;
                }
            }
            if (countdown) {
                countdown.textContent = secondsLeft;
            }
            if (secondsLeft <= 0) {
                clearInterval(countdownInterval);
            }
        }, 1000);
    }

    // ===== Expire Session =====
    function expireSession() {
        clearInterval(countdownInterval);
        localStorage.removeItem('imts_session');
        localStorage.removeItem('imts_user_role');
        localStorage.removeItem('imts_session_start');
        
        // Redirect to login with expired parameter
        window.location.href = '../login.html?expired=true';
    }

    // ===== Check Session on Page Load =====
    function checkSession() {
        // Skip if on login page
        const currentPath = window.location.pathname;
        const isLoginPage = currentPath.includes('login.html') || 
                           currentPath.endsWith('/') ||
                           currentPath === '';
        
        if (isLoginPage) {
            return;
        }
        
        // Check if user is logged in
        if (!isLoggedIn()) {
            // Determine correct path to login
            const pathParts = currentPath.split('/');
            const depth = pathParts.filter(p => p && !p.includes('.html')).length;
            const loginPath = depth > 0 ? '../login.html' : 'login.html';
            window.location.href = loginPath;
            return;
        }
        
        // Start session timer
        startSessionTimer();
    }

    // ===== Initialize =====
    function init() {
        // Check session on page load
        checkSession();
        
        // Setup logout buttons - both class-based and direct onclick
        document.querySelectorAll('.logout-btn').forEach(function(btn) {
            btn.addEventListener('click', function(e) {
                e.preventDefault();
                logout();
            });
        });
        
        // Setup extend session buttons
        document.querySelectorAll('.extend-session-btn').forEach(function(btn) {
            btn.addEventListener('click', function(e) {
                e.preventDefault();
                extendSession();
            });
        });
    }

    // ===== Expose functions globally =====
    window.IMTS = {
        logout: logout,
        extendSession: extendSession,
        getCurrentUser: getCurrentUser,
        isLoggedIn: isLoggedIn,
        getUserRole: getUserRole,
        getUserName: getUserName,
        init: init,
        checkSession: checkSession
    };

    // Auto-initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    console.log('🔐 IMTS Auth System Loaded');
    console.log('👤 Logged in:', isLoggedIn());

})();