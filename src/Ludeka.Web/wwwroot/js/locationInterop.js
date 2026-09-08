// Interop de detección de país y ubicación para Ludeka (Incremento 29)
(function () {
    const TIMEZONE_TO_COUNTRY = {
        // España
        "Europe/Madrid": "España",
        "Atlantic/Canary": "España",
        "Africa/Ceuta": "España",

        // México
        "America/Mexico_City": "México",
        "America/Cancun": "México",
        "America/Merida": "México",
        "America/Monterrey": "México",
        "America/Mazatlan": "México",
        "America/Chihuahua": "México",
        "America/Hermosillo": "México",
        "America/Tijuana": "México",
        "America/Matamoros": "México",

        // Argentina
        "America/Argentina/Buenos_Aires": "Argentina",
        "America/Argentina/Cordoba": "Argentina",
        "America/Argentina/Salta": "Argentina",
        "America/Argentina/Jujuy": "Argentina",
        "America/Argentina/Tucuman": "Argentina",
        "America/Argentina/Catamarca": "Argentina",
        "America/Argentina/La_Rioja": "Argentina",
        "America/Argentina/San_Juan": "Argentina",
        "America/Argentina/Mendoza": "Argentina",
        "America/Argentina/San_Luis": "Argentina",
        "America/Argentina/Rio_Gallegos": "Argentina",
        "America/Argentina/Ushuaia": "Argentina",
        "America/Buenos_Aires": "Argentina",

        // Chile
        "America/Santiago": "Chile",
        "Pacific/Easter": "Chile",
        "America/Punta_Arenas": "Chile",

        // Colombia
        "America/Bogota": "Colombia",

        // Perú
        "America/Lima": "Perú",

        // Uruguay
        "America/Montevideo": "Uruguay",

        // Venezuela
        "America/Caracas": "Venezuela",

        // Estados Unidos
        "America/New_York": "Estados Unidos",
        "America/Chicago": "Estados Unidos",
        "America/Denver": "Estados Unidos",
        "America/Los_Angeles": "Estados Unidos",
        "America/Phoenix": "Estados Unidos",
        "Pacific/Honolulu": "Estados Unidos",
        "America/Anchorage": "Estados Unidos"
    };

    window.detectUserCountry = function () {
        try {
            const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
            if (timeZone) {
                if (TIMEZONE_TO_COUNTRY[timeZone]) {
                    return TIMEZONE_TO_COUNTRY[timeZone];
                }
                // Búsqueda aproximada por prefijo
                if (timeZone.startsWith("America/Argentina/")) return "Argentina";
                if (timeZone.startsWith("America/Mexico")) return "México";
            }
        } catch (e) {
            console.warn("[Location] Error al detectar zona horaria:", e);
        }
        return null;
    };

    window.getStoredUserCountry = function (userId) {
        try {
            if (userId) {
                const userCountry = localStorage.getItem('ludeka-country-' + userId);
                if (userCountry) return userCountry;
            }
            return localStorage.getItem('ludeka-user-country');
        } catch (e) {
            return null;
        }
    };

    window.setStoredUserCountry = function (country, userId) {
        try {
            if (country) {
                localStorage.setItem('ludeka-user-country', country);
                if (userId) {
                    localStorage.setItem('ludeka-country-' + userId, country);
                }
            } else {
                localStorage.removeItem('ludeka-user-country');
                if (userId) {
                    localStorage.removeItem('ludeka-country-' + userId);
                }
            }
        } catch (e) {
            console.warn("[Location] Error al guardar país:", e);
        }
    };
})();
