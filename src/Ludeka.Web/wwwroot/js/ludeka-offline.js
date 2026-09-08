// ludeka-offline.js — Gestor de persistencia local y conectividad en cliente
(function() {
    window.LudekaOffline = {
        // Almacena una instantánea serializada de la ludoteca en localStorage
        saveLibrarySnapshot: function(userId, dataJson) {
            try {
                if (!userId) userId = 'default';
                var payload = {
                    timestamp: new Date().toISOString(),
                    userId: userId,
                    data: typeof dataJson === 'string' ? JSON.parse(dataJson) : dataJson
                };
                var serialized = JSON.stringify(payload);
                localStorage.setItem('ludeka_offline_lib_' + userId, serialized);
                localStorage.setItem('ludeka_offline_lib_last', serialized);
                return true;
            } catch (e) {
                console.warn('[LudekaOffline] Error al guardar instantánea local:', e);
                return false;
            }
        },

        // Recupera la instantánea local de la ludoteca del usuario
        getLibrarySnapshot: function(userId) {
            try {
                if (!userId) userId = 'default';
                var raw = localStorage.getItem('ludeka_offline_lib_' + userId) || localStorage.getItem('ludeka_offline_lib_last');
                if (!raw) return null;
                var parsed = JSON.parse(raw);
                return JSON.stringify(parsed);
            } catch (e) {
                console.warn('[LudekaOffline] Error al recuperar instantánea local:', e);
                return null;
            }
        },

        // Fecha legible del último respaldo offline guardado
        getLastSnapshotTimestamp: function(userId) {
            try {
                if (!userId) userId = 'default';
                var raw = localStorage.getItem('ludeka_offline_lib_' + userId) || localStorage.getItem('ludeka_offline_lib_last');
                if (!raw) return null;
                var parsed = JSON.parse(raw);
                if (!parsed || !parsed.timestamp) return null;
                return new Date(parsed.timestamp).toLocaleString('es-ES', {
                    day: '2-digit',
                    month: '2-digit',
                    year: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit'
                });
            } catch (e) {
                return null;
            }
        },

        // Comprueba si el navegador tiene conexión de red
        isOnline: function() {
            return typeof navigator !== 'undefined' && typeof navigator.onLine === 'boolean' 
                ? navigator.onLine 
                : true;
        },

        // Suscribe el componente DotNet a los cambios de conexión del dispositivo
        initConnectivityListener: function(dotNetHelper) {
            if (!dotNetHelper) return;

            function notify(status) {
                try {
                    dotNetHelper.invokeMethodAsync('OnConnectivityChanged', status);
                } catch (err) {
                    // El componente puede haberse desmontado
                }
            }

            window.addEventListener('online', function() {
                notify(true);
            });

            window.addEventListener('offline', function() {
                notify(false);
            });

            return navigator.onLine;
        }
    };
})();
