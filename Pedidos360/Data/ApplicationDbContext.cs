using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Models;

namespace Pedidos360.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Pedido> Pedidos => Set<Pedido>();
        public DbSet<PedidoDetalle> PedidoDetalles => Set<PedidoDetalle>();
        public DbSet<TipoServicio> TiposServicio => Set<TipoServicio>();
        public DbSet<OrdenServicio> OrdenesServicio => Set<OrdenServicio>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Identity primero: sus tablas se renombran despues.
            base.OnModelCreating(modelBuilder);

            ConfigurarIdentity(modelBuilder);
            ConfigurarCategorias(modelBuilder);
            ConfigurarProductos(modelBuilder);
            ConfigurarClientes(modelBuilder);
            ConfigurarPedidos(modelBuilder);
            ConfigurarPedidoDetalles(modelBuilder);
            ConfigurarTiposServicio(modelBuilder);
            ConfigurarOrdenesServicio(modelBuilder);
        }

        // Tipos de servicio cargados como datos iniciales de EF (no tienen CRUD).
        private static void ConfigurarTiposServicio(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TipoServicio>(entity =>
            {
                entity.ToTable("TiposServicio");
                entity.HasIndex(t => t.Nombre).IsUnique();

                entity.HasData(
                    new TipoServicio { Id = 1, Nombre = "Diagnóstico" },
                    new TipoServicio { Id = 2, Nombre = "Reparación de hardware" },
                    new TipoServicio { Id = 3, Nombre = "Mantenimiento preventivo" },
                    new TipoServicio { Id = 4, Nombre = "Instalación de software" });
            });
        }

        private static void ConfigurarOrdenesServicio(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrdenServicio>(entity =>
            {
                entity.ToTable("OrdenesServicio", t =>
                {
                    t.HasCheckConstraint("CK_OrdenesServicio_Estado", "[Estado] IN (1, 2, 3, 4)");
                    t.HasCheckConstraint("CK_OrdenesServicio_CostoEstimado", "[CostoEstimado] > 0");
                });

                entity.Property(o => o.CostoEstimado).HasPrecision(18, 2);
                entity.HasIndex(o => o.Estado);

                // No se puede borrar un cliente ni un tipo de servicio que tengan ordenes.
                entity.HasOne(o => o.Cliente)
                      .WithMany()
                      .HasForeignKey(o => o.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.TipoServicio)
                      .WithMany()
                      .HasForeignKey(o => o.TipoServicioId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // Nombres en espanol para las tablas de Identity, igual que en Database/01_Crear_Pedidos360.sql.
        private static void ConfigurarIdentity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityUser>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.Property(u => u.UserName).HasColumnName("NombreUsuario");
                entity.Property(u => u.Email).HasColumnName("Correo");
                entity.Property(u => u.PasswordHash).HasColumnName("HashContrasenna");
            });

            modelBuilder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable("Roles");
                entity.Property(r => r.Name).HasColumnName("Nombre");
            });

            modelBuilder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("UsuarioRoles");
                entity.Property(ur => ur.UserId).HasColumnName("IdUsuario");
                entity.Property(ur => ur.RoleId).HasColumnName("IdRol");
            });

            modelBuilder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("RolClaims");
                entity.Property(rc => rc.RoleId).HasColumnName("IdRol");
            });

            modelBuilder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("UsuarioClaims");
                entity.Property(uc => uc.UserId).HasColumnName("IdUsuario");
            });

            modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("UsuarioLogins");
                entity.Property(ul => ul.UserId).HasColumnName("IdUsuario");
            });

            modelBuilder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("UsuarioTokens");
                entity.Property(ut => ut.UserId).HasColumnName("IdUsuario");
            });
        }

        private static void ConfigurarCategorias(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable("Categorias");
                entity.HasIndex(c => c.Nombre).IsUnique();
            });
        }

        private static void ConfigurarProductos(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("Productos", t =>
                {
                    t.HasCheckConstraint("CK_Productos_Precio", "[Precio] > 0");
                    t.HasCheckConstraint("CK_Productos_ImpuestoPorc", "[ImpuestoPorc] BETWEEN 0 AND 100");
                    t.HasCheckConstraint("CK_Productos_Stock", "[Stock] >= 0");
                });

                entity.Property(p => p.Precio).HasPrecision(18, 2);
                entity.Property(p => p.ImpuestoPorc).HasPrecision(5, 2);

                // La imagen se guarda en la base, no como enlace.
                entity.Property(p => p.Imagen).HasColumnType("varbinary(max)");

                entity.HasIndex(p => p.Codigo).IsUnique();
                entity.HasIndex(p => p.Nombre);

                // No se puede borrar una categoria que tenga productos.
                entity.HasOne(p => p.Categoria)
                      .WithMany(c => c.Productos)
                      .HasForeignKey(p => p.CategoriaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigurarClientes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Clientes", t =>
                    t.HasCheckConstraint("CK_Clientes_TipoIdentificacion", "[TipoIdentificacion] IN (1, 2)"));

                // Cedula: 9 digitos (fisica) o 10 (juridica). Telefono: 8 digitos.
                entity.Property(c => c.Cedula).HasMaxLength(10);
                entity.Property(c => c.Telefono).HasMaxLength(8);

                entity.HasIndex(c => c.Cedula).IsUnique();
                entity.HasIndex(c => c.Nombre);
            });
        }

        private static void ConfigurarPedidos(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.ToTable("Pedidos", t =>
                {
                    t.HasCheckConstraint("CK_Pedidos_Estado", "[Estado] IN (1, 2)");
                    t.HasCheckConstraint("CK_Pedidos_Subtotal", "[Subtotal] >= 0");
                    t.HasCheckConstraint("CK_Pedidos_Impuestos", "[Impuestos] >= 0");
                    t.HasCheckConstraint("CK_Pedidos_Total", "[Total] = [Subtotal] + [Impuestos]");
                });

                entity.Property(p => p.Subtotal).HasPrecision(18, 2);
                entity.Property(p => p.Impuestos).HasPrecision(18, 2);
                entity.Property(p => p.Total).HasPrecision(18, 2);

                // No se puede borrar un cliente con pedidos.
                entity.HasOne(p => p.Cliente)
                      .WithMany(c => c.Pedidos)
                      .HasForeignKey(p => p.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Usuario de Identity que confirmo el pedido.
                entity.HasOne(p => p.Usuario)
                      .WithMany()
                      .HasForeignKey(p => p.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigurarPedidoDetalles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PedidoDetalle>(entity =>
            {
                entity.ToTable("PedidoDetalles", t =>
                {
                    t.HasCheckConstraint("CK_PedidoDetalles_Cantidad", "[Cantidad] > 0");
                    t.HasCheckConstraint("CK_PedidoDetalles_PrecioUnit", "[PrecioUnit] > 0");
                    t.HasCheckConstraint("CK_PedidoDetalles_TipoDescuento", "[TipoDescuento] IN (1, 2)");
                    t.HasCheckConstraint("CK_PedidoDetalles_Descuento",
                        "[Descuento] >= 0 AND (([TipoDescuento] = 1 AND [Descuento] <= 100) OR ([TipoDescuento] = 2 AND [Descuento] <= [Cantidad] * [PrecioUnit]))");
                    t.HasCheckConstraint("CK_PedidoDetalles_ImpuestoPorc", "[ImpuestoPorc] BETWEEN 0 AND 100");
                    t.HasCheckConstraint("CK_PedidoDetalles_TotalLinea", "[TotalLinea] >= 0");
                });

                entity.Property(d => d.PrecioUnit).HasPrecision(18, 2);
                entity.Property(d => d.Descuento).HasPrecision(18, 2);
                entity.Property(d => d.ImpuestoPorc).HasPrecision(5, 2);
                entity.Property(d => d.TotalLinea).HasPrecision(18, 2);

                // Un producto aparece una sola vez por pedido.
                entity.HasIndex(d => new { d.PedidoId, d.ProductoId }).IsUnique();

                // Si se borra el pedido se borran sus lineas.
                entity.HasOne(d => d.Pedido)
                      .WithMany(p => p.Detalles)
                      .HasForeignKey(d => d.PedidoId)
                      .OnDelete(DeleteBehavior.Cascade);

                // No se puede borrar un producto que ya se vendio.
                entity.HasOne(d => d.Producto)
                      .WithMany(p => p.Detalles)
                      .HasForeignKey(d => d.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
