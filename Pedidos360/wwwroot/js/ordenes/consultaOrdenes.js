// RF-03: consulta dinamica de ordenes de servicio por AJAX.
// Lee las ordenes desde GET /api/ordenes, llena la tabla y los contadores,
// y filtra por estado sin recargar la pagina.
(function ($) {
    'use strict';

    const URL_API = '/api/ordenes';
    const $tabla = $('#tablaOrdenes tbody');
    const $filtro = $('#filtroEstado');
    const $mensaje = $('#mensajeOrdenes');
    const formatoColones = new Intl.NumberFormat('es-CR', { style: 'currency', currency: 'CRC', minimumFractionDigits: 0 });
    const clasesEstado = { 1: 'bg-secondary', 2: 'bg-warning text-dark', 3: 'bg-info text-dark', 4: 'bg-success' };

    // Evita que un texto de la base de datos se interprete como HTML.
    function escapar(texto) {
        return $('<div>').text(texto ?? '').html();
    }

    function mostrarMensaje(texto, tipo) {
        $mensaje.html('<div class="alert alert-' + tipo + ' alert-dismissible fade show">' + escapar(texto) +
            '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Cerrar"></button></div>');
    }

    // Mensaje claro segun el codigo HTTP que devuelve la API (RF-06: 401/403).
    function mensajeDeError(xhr, porDefecto) {
        if (xhr.status === 401) return 'Su sesión expiró. Vuelva a iniciar sesión.';
        if (xhr.status === 403) return 'No tiene permisos para esta acción.';
        return (xhr.responseJSON && xhr.responseJSON.title) || porDefecto;
    }

    function pintarTabla(ordenes) {
        if (ordenes.length === 0) {
            $tabla.html('<tr><td colspan="5" class="text-center text-muted py-4">No hay órdenes para mostrar.</td></tr>');
            return;
        }

        const filas = ordenes.map(function (orden) {
            const accion = orden.siguienteEstado
                ? '<button type="button" class="btn btn-sm btn-outline-primary btn-avanzar" data-id="' + orden.id +
                  '" data-siguiente="' + orden.siguienteEstado + '">Pasar a ' + escapar(orden.siguienteEstadoTexto) + '</button>'
                : '<span class="text-muted small">Sin acciones</span>';

            return '<tr>' +
                '<td class="fw-semibold">' + escapar(orden.equipo) + '</td>' +
                '<td>' + escapar(orden.cliente) + '</td>' +
                '<td class="text-end">' + formatoColones.format(orden.costoEstimado) + '</td>' +
                '<td><span class="badge ' + (clasesEstado[orden.estado] || 'bg-secondary') + '">' + escapar(orden.estadoTexto) + '</span></td>' +
                '<td class="text-end text-nowrap">' + accion + '</td>' +
                '</tr>';
        });

        $tabla.html(filas.join(''));
    }

    function pintarContadores(ordenes) {
        const conteo = { 1: 0, 2: 0, 3: 0, 4: 0 };
        ordenes.forEach(function (orden) { conteo[orden.estado]++; });
        $('[data-contador]').each(function () {
            $(this).text(conteo[$(this).data('contador')] || 0);
        });
    }

    // Contadores: siempre con todas las ordenes.
    function cargarContadores() {
        return $.getJSON(URL_API).done(pintarContadores);
    }

    // Tabla: con el estado elegido en el filtro (vacio = todas).
    function cargarTabla() {
        const estado = $filtro.val();
        return $.getJSON(URL_API, estado ? { estado: estado } : {})
            .done(pintarTabla)
            .fail(function (xhr) {
                $tabla.html('<tr><td colspan="5" class="text-center text-danger py-4">No se pudieron cargar las órdenes.</td></tr>');
                mostrarMensaje(mensajeDeError(xhr, 'No se pudieron cargar las órdenes.'), 'danger');
            });
    }

    function recargar() {
        cargarContadores();
        cargarTabla();
    }

    // Filtrar sin recargar la pagina.
    $filtro.on('change', cargarTabla);

    // Avanzar el estado: PUT /api/ordenes/{id}/estado. antiforgery.js agrega el token.
    $tabla.on('click', '.btn-avanzar', function () {
        const $boton = $(this).prop('disabled', true);
        $.ajax({
            url: URL_API + '/' + $boton.data('id') + '/estado',
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({ estado: $boton.data('siguiente') })
        })
            .done(function (orden) {
                mostrarMensaje('La orden de ' + orden.equipo + ' pasó a ' + orden.estadoTexto + '.', 'success');
                recargar();
            })
            .fail(function (xhr) {
                mostrarMensaje(mensajeDeError(xhr, 'No se pudo actualizar el estado.'), 'danger');
                $boton.prop('disabled', false);
            });
    });

    recargar();
})(jQuery);
