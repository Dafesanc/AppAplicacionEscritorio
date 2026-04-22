namespace GestionComercial.Infrastructure.Persistence.Migrations;

using GestionComercial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

[DbContext(typeof(GestionComercialContext))]
partial class GestionComercialContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        // Este archivo es generado automáticamente por EF Core
        // No editar manualmente
        
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        // Las configuraciones de todas las entidades están en DbContext.OnModelCreating()
        // Este snapshot se regenera automáticamente con cada migración
    }
}
