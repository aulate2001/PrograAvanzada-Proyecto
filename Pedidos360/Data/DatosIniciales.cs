using Microsoft.EntityFrameworkCore;
using Pedidos360.Models;

namespace Pedidos360.Data
{
    // Datos de prueba para la demo. Cada tabla se llena solo si esta vacia,
    // asi que se puede ejecutar en cada arranque sin duplicar nada.
    public static class DatosIniciales
    {
        private const string TipoImagen = "image/png";
        private const string ArchivoPlaceholder = "placeholder.png";

        // PNG de 1x1 pixel: ultimo recurso si falta placeholder.png.
        private static readonly byte[] PngMinimo = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR4nGP4z8DwHwAFAAH/iZk9HQAAAABJRU5ErkJggg==");

        public static async Task InicializarAsync(IServiceProvider servicios)
        {
            using var scope = servicios.CreateScope();
            var contexto = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var entorno = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var log = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatosIniciales");

            try
            {
                if (!await contexto.Database.CanConnectAsync())
                {
                    log.LogWarning("Datos iniciales omitidos: no hay conexion a la base de datos.");
                    return;
                }

                if ((await contexto.Database.GetPendingMigrationsAsync()).Any())
                {
                    log.LogWarning("Datos iniciales omitidos: hay migraciones pendientes. Ejecute Update-Database.");
                    return;
                }

                await CargarCategoriasAsync(contexto);
                await CargarProductosAsync(contexto, entorno);
                await CargarClientesAsync(contexto);
                await CargarOrdenesServicioAsync(contexto);
            }
            catch (Exception excepcion)
            {
                // Que falle el seeder no debe impedir que la aplicacion arranque.
                log.LogError(excepcion, "No se pudieron cargar los datos iniciales.");
            }
        }

        private static async Task CargarCategoriasAsync(ApplicationDbContext contexto)
        {
            if (await contexto.Categorias.AnyAsync())
            {
                return;
            }

            contexto.Categorias.AddRange(
                new Categoria { Nombre = "Electrónica" },
                new Categoria { Nombre = "Oficina" });

            await contexto.SaveChangesAsync();
        }

        private static async Task CargarProductosAsync(ApplicationDbContext contexto, IWebHostEnvironment entorno)
        {
            if (await contexto.Productos.AnyAsync())
            {
                return;
            }

            var categorias = await contexto.Categorias.AsNoTracking().ToDictionaryAsync(c => c.Nombre, c => c.Id);
            if (!categorias.TryGetValue("Electrónica", out var electronica) ||
                !categorias.TryGetValue("Oficina", out var oficina))
            {
                // Las categorias ya existian con otros nombres: no se adivina a cual asignar los productos.
                return;
            }

            var carpeta = Path.Combine(entorno.WebRootPath ?? string.Empty, "imagenes-iniciales");

            // (codigo, nombre, categoria, precio, impuesto %, stock).
            var datos = new (string Codigo, string Nombre, int CategoriaId, decimal Precio, decimal Impuesto, int Stock)[]
            {
                ("ELEC-001", "Mouse inalámbrico",        electronica,  8500m, 13m,  45),
                ("ELEC-002", "Teclado mecánico",         electronica, 32500m, 13m,  18),
                ("ELEC-003", "Audífonos Bluetooth",      electronica, 27900m, 13m,  25),
                ("ELEC-004", "Cable HDMI 2 m",           electronica,  4900m, 13m, 120),
                ("ELEC-005", "Memoria USB 64 GB",        electronica,  6500m, 13m,  60),
                ("ELEC-006", "Cargador USB-C 20 W",      electronica,  9900m, 13m,   3),
                ("OFI-001",  "Resma de papel carta",     oficina,      4200m, 13m, 200),
                ("OFI-002",  "Cuaderno universitario",   oficina,      2100m,  1m,  80),
                ("OFI-003",  "Bolígrafos azules (12 u)", oficina,      3500m, 13m,  90),
                ("OFI-004",  "Engrapadora metálica",     oficina,      5800m, 13m,  30),
                ("OFI-005",  "Archivador A-Z",           oficina,      4700m,  2m,  40),
                ("OFI-006",  "Calculadora científica",   oficina,     14500m, 13m,  12),
            };

            var imagen = LeerImagen(carpeta);

            foreach (var d in datos)
            {
                contexto.Productos.Add(new Producto
                {
                    Codigo = d.Codigo,
                    Nombre = d.Nombre,
                    CategoriaId = d.CategoriaId,
                    Precio = d.Precio,
                    ImpuestoPorc = d.Impuesto,
                    Stock = d.Stock,
                    Imagen = imagen,
                    ImagenTipo = TipoImagen,
                    Activo = true
                });
            }

            await contexto.SaveChangesAsync();
        }

        private static async Task CargarClientesAsync(ApplicationDbContext contexto)
        {
            if (await contexto.Clientes.AnyAsync())
            {
                return;
            }

            // Cedula fisica = 9 digitos, juridica = 10. Telefono = 8 digitos (reglas de Models/Cliente.cs).
            contexto.Clientes.AddRange(
                Crear(TipoIdentificacion.Fisica, "102340567", "María Fernanda Solís Vargas", "maria.solis@correo.com", "88112233", "San José, Escazú, 200 m norte de la iglesia"),
                Crear(TipoIdentificacion.Fisica, "203450678", "Carlos Andrés Rojas Mora", "carlos.rojas@correo.com", "88223344", "Alajuela, Grecia, Barrio San Roque"),
                Crear(TipoIdentificacion.Fisica, "304560789", "Daniela Jiménez Araya", "daniela.jimenez@correo.com", "88334455", "Cartago, Paraíso, frente al parque"),
                Crear(TipoIdentificacion.Fisica, "105670890", "José Pablo Castro Chaves", "jose.castro@correo.com", "88445566", "Heredia, Barva, 100 m sur de la escuela"),
                Crear(TipoIdentificacion.Fisica, "206780901", "Valeria Montero Quesada", "valeria.montero@correo.com", "88556677", "San José, Desamparados, Urbanización Los Pinos"),
                Crear(TipoIdentificacion.Fisica, "307890012", "Luis Diego Ramírez Salas", "luis.ramirez@correo.com", "88667788", "Puntarenas, Esparza, Calle Central"),
                Crear(TipoIdentificacion.Juridica, "3101123456", "Distribuidora La Esperanza S.A.", "compras@laesperanza.co.cr", "22001122", "San José, Zapote, 300 m este del Registro Civil"),
                Crear(TipoIdentificacion.Juridica, "3102234567", "Oficentro Costa Rica S.R.L.", "ventas@oficentrocr.co.cr", "22113344", "Heredia, Ulloa, Zona Franca"));

            await contexto.SaveChangesAsync();
        }

        // Ordenes de ejemplo del enunciado (RF-03). Los clientes se crean si no existen.
        private static async Task CargarOrdenesServicioAsync(ApplicationDbContext contexto)
        {
            if (await contexto.OrdenesServicio.AnyAsync())
            {
                return;
            }

            var carlos = await ObtenerOCrearClienteAsync(contexto, "111110001", "Carlos Mora", "carlos.mora@correo.com", "87001001", "San José, Curridabat");
            var ana = await ObtenerOCrearClienteAsync(contexto, "111110002", "Ana Vargas", "ana.vargas@correo.com", "87001002", "Heredia, San Pablo");
            var luis = await ObtenerOCrearClienteAsync(contexto, "111110003", "Luis Castro", "luis.castro@correo.com", "87001003", "Alajuela, Centro");
            var maria = await ObtenerOCrearClienteAsync(contexto, "111110004", "María Solís", "maria.solis2@correo.com", "87001004", "Cartago, Tres Ríos");

            var hoy = DateTime.Now;
            contexto.OrdenesServicio.AddRange(
                CrearOrden(carlos, 2, "Laptop HP", "No enciende, posible falla en la tarjeta madre.", 25000m, EstadoOrden.Pendiente, hoy.AddDays(-1)),
                CrearOrden(ana, 2, "iPhone 13", "Pantalla quebrada.", 45000m, EstadoOrden.EnReparacion, hoy.AddDays(-3)),
                CrearOrden(luis, 3, "PC Gamer", "Limpieza interna y cambio de pasta térmica.", 15000m, EstadoOrden.Listo, hoy.AddDays(-5)),
                CrearOrden(maria, 4, "MacBook Air", "Reinstalar sistema operativo.", 35000m, EstadoOrden.Entregado, hoy.AddDays(-7), hoy.AddDays(-2)));

            await contexto.SaveChangesAsync();
        }

        private static async Task<Cliente> ObtenerOCrearClienteAsync(ApplicationDbContext contexto, string cedula, string nombre, string correo, string telefono, string direccion)
        {
            var cliente = await contexto.Clientes.FirstOrDefaultAsync(c => c.Cedula == cedula);
            if (cliente is null)
            {
                cliente = Crear(TipoIdentificacion.Fisica, cedula, nombre, correo, telefono, direccion);
                contexto.Clientes.Add(cliente);
                await contexto.SaveChangesAsync();
            }

            return cliente;
        }

        private static OrdenServicio CrearOrden(Cliente cliente, int tipoServicioId, string equipo, string descripcion, decimal costo, EstadoOrden estado, DateTime ingreso, DateTime? entrega = null)
        {
            return new OrdenServicio
            {
                ClienteId = cliente.Id,
                TipoServicioId = tipoServicioId,
                Equipo = equipo,
                DescripcionProblema = descripcion,
                CostoEstimado = costo,
                Estado = estado,
                FechaIngreso = ingreso,
                FechaEntrega = entrega
            };
        }

        private static Cliente Crear(TipoIdentificacion tipo, string cedula, string nombre, string correo, string telefono, string direccion)
        {
            return new Cliente
            {
                TipoIdentificacion = tipo,
                Cedula = cedula,
                Nombre = nombre,
                Correo = correo,
                Telefono = telefono,
                Direccion = direccion
            };
        }

        // Por ahora todos los productos usan la misma imagen: wwwroot/imagenes-iniciales/placeholder.png.
        // Si el archivo no existe se usa un PNG minimo para que el seeder no falle.
        private static byte[] LeerImagen(string carpeta)
        {
            var ruta = Path.Combine(carpeta, ArchivoPlaceholder);
            return File.Exists(ruta) ? File.ReadAllBytes(ruta) : PngMinimo;
        }
    }
}
