
        // ===== SETTINGS FUNCTIONS =====
        function saveAdminProfile() {
            const phone = document.getElementById('adminPhone').value.trim();
            if (!phone) { alert('Please enter your phone number.'); return; }
            localStorage.setItem('imts_admin_phone', phone);
            alert('✅ Profile updated successfully!');
        }

        function saveAdminPreferences() {
            const view = document.getElementById('defaultView').value;
            const notifications = document.getElementById('adminNotifications').value;
            const dateFormat = document.getElementById('dateFormat').value;
            
            localStorage.setItem('imts_admin_preferences', JSON.stringify({
                defaultView: view,
                notifications: notifications,
                dateFormat: dateFormat
            }));
            
            alert('✅ Administrator preferences saved successfully!');
        }

        function saveSecurityPreferences() {
            const securityPrefs = {
                twoFactor: document.getElementById('adminTwoFactor').checked,
                ipWhitelisting: document.getElementById('adminIpWhitelist').checked,
                auditLogging: document.getElementById('adminAuditLogging').checked,
                sessionTimeout: document.getElementById('adminSessionTimeout').checked
            };
            localStorage.setItem('imts_admin_security', JSON.stringify(securityPrefs));
            alert('✅ Security settings saved successfully!');
        }

        function applyTheme(theme) {
            if (theme === 'dark') {
                document.documentElement.classList.add('dark');
            } else {
                document.documentElement.classList.remove('dark');
            }
        }

        function setTheme(theme, showAlert = true) {
            const resolvedTheme = theme === 'system'
                ? (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light')
                : theme;

            applyTheme(resolvedTheme);
            localStorage.setItem('imts_theme', theme);

            if (showAlert) {
                alert(`✅ Theme set to ${theme.charAt(0).toUpperCase() + theme.slice(1)}`);
            }
        }

        // ===== LOAD SETTINGS =====
        function loadSettings() {
            const prefs = JSON.parse(localStorage.getItem('imts_admin_preferences') || '{}');
            if (prefs.defaultView) {
                document.getElementById('defaultView').value = prefs.defaultView;
            }
            if (prefs.notifications) {
                document.getElementById('adminNotifications').value = prefs.notifications;
            }
            if (prefs.dateFormat) {
                document.getElementById('dateFormat').value = prefs.dateFormat;
            }

            const securityPrefs = JSON.parse(localStorage.getItem('imts_admin_security') || '{"twoFactor":true,"ipWhitelisting":false,"auditLogging":true,"sessionTimeout":true}');
            document.getElementById('adminTwoFactor').checked = securityPrefs.twoFactor;
            document.getElementById('adminIpWhitelist').checked = securityPrefs.ipWhitelisting;
            document.getElementById('adminAuditLogging').checked = securityPrefs.auditLogging;
            document.getElementById('adminSessionTimeout').checked = securityPrefs.sessionTimeout;

            const phone = localStorage.getItem('imts_admin_phone');
            if (phone) {
                document.getElementById('adminPhone').value = phone;
            }

            const theme = localStorage.getItem('imts_theme') || 'light';
            setTheme(theme, false);
        }

        // ===== INIT =====
        loadSettings();

        try {
            const session = localStorage.getItem('imts_session');
            if (session) {
                const data = JSON.parse(session);
                document.getElementById('userNameDisplay').textContent = data.name || 'S. Katumba';
            }
        } catch(e) {}
    