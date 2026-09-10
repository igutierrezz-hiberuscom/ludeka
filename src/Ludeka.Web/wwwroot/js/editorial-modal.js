// EditorialModal (INC-36, DD-07): gestión de foco coherente del shell compartido.
// Guarda el foco previo, enfoca el botón de cierre al abrir, lo restaura al cerrar
// y cierra con Escape. Mejora progresiva: sin JS el modal sigue siendo markup puro.
window.ludekaModal = (function () {
    var focoPrevio = null;
    var dialogo = null;

    function abrir(dialogoRef, cierreRef) {
        focoPrevio = document.activeElement;
        dialogo = dialogoRef;
        if (cierreRef) cierreRef.focus();
    }

    function cerrar() {
        dialogo = null;
        if (focoPrevio && document.contains(focoPrevio)) focoPrevio.focus();
        focoPrevio = null;
    }

    document.addEventListener('keydown', function (evento) {
        if (evento.key === 'Escape' && dialogo) {
            var cierre = dialogo.querySelector('[data-editorial-cierre]');
            if (cierre) cierre.click();
        }
    });

    return { open: abrir, close: cerrar };
})();
