(function () {
    'use strict';

    // Función para mostrar mensajes de error o información al usuario
    function mensaje(texto) {
        var elemento = document.getElementById('mensaje');
        if (elemento) elemento.textContent = texto || '';
    }

    // Función para cargar el carrito desde la API
    function cargarCarrito() {
        return fetch('/api/carrito', { headers: { Accept: 'application/json' } })
            .then(function (response) { return response.json(); });
    }

    // Función para pintar el carrito en la tabla HTML
    function pintarCarrito(items) {
        var tabla = document.getElementById('carrito-items');
        var vacio = document.getElementById('carrito-vacio');
        if (!tabla) return;

        tabla.innerHTML = '';
        if (!items.length) {
            if (vacio) vacio.hidden = false;
            return;
        }
        if (vacio) vacio.hidden = true;

        items.forEach(function (item) {
            var fila = document.createElement('tr');
            fila.innerHTML = '<td>' + item.SalidaId + '</td>' +
                '<td><input type="number" min="1" max="6" value="' + item.CantidadPasajeros + '" data-salida="' + item.SalidaId + '" /></td>' +
                '<td><button type="button" data-eliminar="' + item.SalidaId + '">Eliminar</button></td>';
            tabla.appendChild(fila);
        });
    }

    // Función para cargar el formulario de pasajeros según las salidas en el carrito
    function cargarFormularioPasajeros(items) {
        var contenedor = document.getElementById('salidas-container');
        if (!contenedor) return;

        contenedor.innerHTML = '';
        items.forEach(function (item, salidaIndex) {
            var bloque = document.createElement('fieldset');
            bloque.innerHTML = '<legend>Salida ' + item.SalidaId + '</legend>' +
                '<input type="hidden" name="Salidas[' + salidaIndex + '].SalidaId" value="' + item.SalidaId + '" />';

            for (var pasajeroIndex = 0; pasajeroIndex < item.CantidadPasajeros; pasajeroIndex++) {
                bloque.innerHTML += '<label>Nombre completo<input required maxlength="120" name="Salidas[' + salidaIndex + '].Pasajeros[' + pasajeroIndex + '].NombreCompleto" /></label>' +
                    '<label>Identificación<input required maxlength="30" name="Salidas[' + salidaIndex + '].Pasajeros[' + pasajeroIndex + '].Identificacion" /></label>';
            }
            contenedor.appendChild(bloque);
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        cargarCarrito().then(function (items) {
            if (window.ecoRutasConfirmar) cargarFormularioPasajeros(items);
            else pintarCarrito(items);
        });

        document.addEventListener('click', function (event) {
            var salidaId = event.target.getAttribute('data-eliminar');
            if (!salidaId) return;
            fetch('/api/carrito/items/' + encodeURIComponent(salidaId), { method: 'DELETE' })
                .then(function (response) {
                    if (!response.ok) throw new Error('No se pudo eliminar la salida.');
                    return response.json();
                })
                .then(function (data) { pintarCarrito(data.items); })
                .catch(function (error) { mensaje(error.message); });
        });

        document.addEventListener('change', function (event) {
            var salidaId = event.target.getAttribute('data-salida');
            if (!salidaId) return;
            var cantidad = event.target.value;
            fetch('/api/carrito/items/' + encodeURIComponent(salidaId) + '?cantidadPasajeros=' + encodeURIComponent(cantidad), { method: 'PUT' })
                .then(function (response) {
                    if (!response.ok) throw new Error('La cantidad debe estar entre 1 y 6.');
                    return response.json();
                })
                .then(function (data) { pintarCarrito(data.items); })
                .catch(function (error) { mensaje(error.message); });
        });
    });
}());
