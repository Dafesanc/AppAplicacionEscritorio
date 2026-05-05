namespace GestionComercial.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using GestionComercial.Domain.Entities;

/// <summary>
/// DbContext principal del sistema - Configura todas las entidades
/// </summary>
public class GestionComercialContext : DbContext
{
    public GestionComercialContext(DbContextOptions<GestionComercialContext> options)
        : base(options)
    {
    }

    // DbSets - Mapped de entidades
    public DbSet<Rol> Roles { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Vehiculo> Vehiculos { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Pesaje> Pesajes { get; set; }
    public DbSet<Venta> Ventas { get; set; }
    public DbSet<DetalleVenta> DetallesVentas { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
    public DbSet<LoteInventario> LotesInventario { get; set; }
    public DbSet<Auditoria> Auditorias { get; set; }

    /// <summary>
    /// Se ejecuta cuando se crea el modelo - Configuración de entidades
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de Rol
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.ID_Rol);
            entity.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500);
            entity.HasIndex(e => e.Nombre).IsUnique();
        });

        // Configuración de Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.ID_Usuario);
            entity.Property(e => e.NombreUsuario)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.NombreCompleto)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Contrasena)
                .IsRequired();
            entity.Property(e => e.Email)
                .HasMaxLength(100);
            entity.Property(e => e.Telefono)
                .HasMaxLength(20);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("ACTIVO");
            entity.HasIndex(e => e.NombreUsuario).IsUnique();
            entity.HasOne(e => e.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(e => e.ID_Rol)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.ID_Cliente);
            entity.Property(e => e.CodigoCliente)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(150);
            entity.Property(e => e.TipoIdentificacion)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.NumeroIdentificacion)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Categoria)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Contacto)
                .HasMaxLength(20);
            entity.Property(e => e.Email)
                .HasMaxLength(100);
            entity.Property(e => e.Direccion)
                .HasMaxLength(250);
            entity.Property(e => e.DescuentoPorDefecto)
                .HasPrecision(5, 2)
                .HasDefaultValue(0);
            entity.Property(e => e.LimiteCredito)
                .HasPrecision(15, 2)
                .HasDefaultValue(0);
            entity.Property(e => e.SaldoCredito)
                .HasPrecision(15, 2)
                .HasDefaultValue(0);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("ACTIVO");
            entity.HasIndex(e => e.CodigoCliente).IsUnique();
            entity.HasIndex(e => e.NumeroIdentificacion).IsUnique();
            entity.HasOne(e => e.UsuarioCreador)
                .WithMany()
                .HasForeignKey(e => e.UsuarioCreacion)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuración de Vehículo
        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.HasKey(e => e.ID_Vehiculo);
            entity.Property(e => e.Placa)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.Tipo)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Marca)
                .HasMaxLength(50);
            entity.Property(e => e.Modelo)
                .HasMaxLength(50);
            entity.Property(e => e.Color)
                .HasMaxLength(30);
            entity.Property(e => e.Capacidad)
                .HasPrecision(10, 2);
            entity.Property(e => e.PesoTara)
                .HasPrecision(10, 2);
            entity.Property(e => e.VIN)
                .HasMaxLength(50);
            entity.Property(e => e.PlacaINEN)
                .HasMaxLength(50);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("ACTIVO");
            entity.HasIndex(e => e.Placa).IsUnique();
            entity.HasIndex(e => e.ID_Cliente);
            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Vehiculos)
                .HasForeignKey(e => e.ID_Cliente)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.UsuarioCreador)
                .WithMany()
                .HasForeignKey(e => e.UsuarioCreacion)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuración de Producto
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.ID_Producto);
            entity.Property(e => e.Codigo)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(150);
            entity.Property(e => e.TipoMaterial)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Unidad)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.PrecioBase)
                .HasPrecision(15, 4);
            entity.Property(e => e.Stock)
                .HasPrecision(15, 2)
                .HasDefaultValue(0);
            entity.Property(e => e.StockMinimo)
                .HasPrecision(15, 2)
                .HasDefaultValue(0);
            entity.Property(e => e.StockMaximo)
                .HasPrecision(15, 2)
                .HasDefaultValue(0);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("ACTIVO");
            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.HasOne(e => e.UsuarioCreador)
                .WithMany()
                .HasForeignKey(e => e.UsuarioCreacion)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuración de Pesaje
        modelBuilder.Entity<Pesaje>(entity =>
        {
            entity.HasKey(e => e.ID_Pesaje);
            entity.Property(e => e.TipoPesaje)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.PesoKg)
                .HasPrecision(10, 2)
                .IsRequired();
            entity.Property(e => e.Temperatura)
                .HasPrecision(5, 2);
            entity.Property(e => e.Humedad)
                .HasPrecision(5, 2);
            entity.Property(e => e.EstadoBascula)
                .HasMaxLength(50);
            entity.Property(e => e.NumeroSerie)
                .HasMaxLength(50);
            entity.HasIndex(e => e.ID_Vehiculo);
            entity.HasIndex(e => e.FechaPesaje);
            entity.HasOne(e => e.Vehiculo)
                .WithMany(v => v.Pesajes)
                .HasForeignKey(e => e.ID_Vehiculo)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioPesaje)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de Venta
        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.ID_Venta);
            entity.Property(e => e.NumeroVenta)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.PesoTaraKg)
                .HasPrecision(10, 2);
            entity.Property(e => e.PesoBrutoKg)
                .HasPrecision(10, 2);
            entity.Property(e => e.PesoNetoKg)
                .HasPrecision(10, 2);
            entity.Property(e => e.Subtotal)
                .HasPrecision(15, 4);
            entity.Property(e => e.DescuentosAplicados)
                .HasPrecision(15, 4)
                .HasDefaultValue(0);
            entity.Property(e => e.IVA)
                .HasPrecision(15, 4)
                .HasDefaultValue(0);
            entity.Property(e => e.TotalVenta)
                .HasPrecision(15, 4);
            entity.Property(e => e.TipoDocumento)
                .HasMaxLength(20)
                .HasDefaultValue("TICKET");
            entity.Property(e => e.EstadoVenta)
                .HasMaxLength(20)
                .HasDefaultValue("COMPLETADA");
            entity.HasIndex(e => e.NumeroVenta).IsUnique();
            entity.HasIndex(e => e.ID_Cliente);
            entity.HasIndex(e => e.FechaVenta);
            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Ventas)
                .HasForeignKey(e => e.ID_Cliente)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Vehiculo)
                .WithMany(v => v.Ventas)
                .HasForeignKey(e => e.ID_Vehiculo)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.PesajeTara)
                .WithMany()
                .HasForeignKey(e => e.ID_PesajeTara)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.PesajeBruto)
                .WithMany()
                .HasForeignKey(e => e.ID_PesajeBruto)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioVenta)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Producto)
                .WithMany()
                .HasForeignKey(e => e.ID_Producto)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuración de DetalleVenta
        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.HasKey(e => e.ID_DetalleVenta);
            entity.Property(e => e.Cantidad)
                .HasPrecision(15, 2);
            entity.Property(e => e.PrecioUnitario)
                .HasPrecision(15, 4);
            entity.Property(e => e.DescuentoLinea)
                .HasPrecision(5, 2)
                .HasDefaultValue(0);
            entity.Property(e => e.ValorDescuento)
                .HasPrecision(15, 4);
            entity.Property(e => e.SubtotalLinea)
                .HasPrecision(15, 4);
            entity.HasIndex(e => e.ID_Venta);
            entity.HasOne(e => e.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(e => e.ID_Venta)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Producto)
                .WithMany(p => p.DetallesVenta)
                .HasForeignKey(e => e.ID_Producto)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de Factura
        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.ID_Factura);
            entity.Property(e => e.NumeroFactura)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.SubtotalFactura)
                .HasPrecision(15, 4);
            entity.Property(e => e.IVAFactura)
                .HasPrecision(15, 4);
            entity.Property(e => e.TotalFactura)
                .HasPrecision(15, 4);
            entity.Property(e => e.EstadoFactura)
                .HasMaxLength(20)
                .HasDefaultValue("EMITIDA");
            entity.HasIndex(e => e.NumeroFactura).IsUnique();
            entity.HasIndex(e => e.ID_Cliente);
            entity.HasOne(e => e.Venta)
                .WithOne(v => v.Factura)
                .HasForeignKey<Factura>(e => e.ID_Venta)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Cliente)
                .WithMany()
                .HasForeignKey(e => e.ID_Cliente)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioEmision)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de MovimientoInventario
        modelBuilder.Entity<MovimientoInventario>(entity =>
        {
            entity.HasKey(e => e.ID_Movimiento);
            entity.Property(e => e.TipoMovimiento)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.Cantidad)
                .HasPrecision(15, 2);
            entity.Property(e => e.StockAnterior)
                .HasPrecision(15, 2);
            entity.Property(e => e.StockPosterior)
                .HasPrecision(15, 2);
            entity.Property(e => e.Referencia)
                .HasMaxLength(100);
            entity.HasIndex(e => e.ID_Producto);
            entity.HasIndex(e => e.FechaMovimiento);
            entity.HasOne(e => e.Producto)
                .WithMany(p => p.Movimientos)
                .HasForeignKey(e => e.ID_Producto)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioMovimiento)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de LoteInventario
        modelBuilder.Entity<LoteInventario>(entity =>
        {
            entity.HasKey(e => e.ID_Lote);
            entity.Property(e => e.NumeroLote)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.QuantidadRecibida)
                .HasPrecision(15, 2);
            entity.Property(e => e.QuantidadDisponible)
                .HasPrecision(15, 2);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("DISPONIBLE");
            entity.Property(e => e.Proveedor)
                .HasMaxLength(100);
            entity.HasIndex(e => e.ID_Producto);
            entity.HasIndex(e => e.NumeroLote).IsUnique();
            entity.HasOne(e => e.Producto)
                .WithMany(p => p.Lotes)
                .HasForeignKey(e => e.ID_Producto)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de Auditoria
        modelBuilder.Entity<Auditoria>(entity =>
        {
            entity.HasKey(e => e.ID_Auditoria);
            entity.Property(e => e.Tabla)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.TipoOperacion)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.RegistroID)
                .HasMaxLength(100);
            entity.Property(e => e.DireccionIP)
                .HasMaxLength(50);
            entity.Property(e => e.Razon)
                .HasMaxLength(250);
            entity.HasIndex(e => e.ID_Usuario);
            entity.HasIndex(e => e.FechaOperacion);
            entity.HasIndex(e => e.Tabla);
            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.ID_Usuario)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
