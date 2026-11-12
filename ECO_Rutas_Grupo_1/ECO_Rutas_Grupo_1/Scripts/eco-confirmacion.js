// Persona 4 — Valeria Calvo Chaves
// Rellena el modal de confirmación con los datos del botón que lo abrió
// y muestra un spinner mientras se envía el formulario.
using System.Diagnostics;
using System.Xml.Linq;

(function() {
    'use strict';

    document.addEventListener('DOMContentLoaded', function() {
        var modal = document.getElementById('modalConfirmacion');
        if (!modal)
        {
            return;
        }

        var form = document.getElementById('formConfirmacion');
        var titulo = document.getElementById('modalConfirmacionTitulo');
        var mensaje = document.getElementById('modalConfirmacionMensaje');
        var boton = document.getElementById('modalConfirmacionBoton');
        var textoBoton = boton.querySelector('.texto-boton');
        var spinner = boton.querySelector('.spinner-border');

        modal.addEventListener('show.bs.modal', function(evento) {
            var origen = evento.relatedTarget;
            if (!origen)
            {
                return;
            }

            form.setAttribute('action', origen.getAttribute('data-accion'));
            titulo.textContent = origen.getAttribute('data-titulo') || 'Confirmar acción';
            mensaje.textContent = origen.getAttribute('data-mensaje') || '¿Desea continuar?';
            textoBoton.textContent = origen.getAttribute('data-boton') || 'Confirmar';
            boton.className = 'btn ' + (origen.getAttribute('data-clase-boton') || 'btn-danger');
            boton.disabled = false;
            spinner.classList.add('d-none');
        });

        form.addEventListener('submit', function() {
            boton.disabled = true;
            spinner.classList.remove('d-none');
        });
    });
})();