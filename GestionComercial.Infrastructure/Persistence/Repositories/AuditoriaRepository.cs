namespace GestionComercial.Infrastructure.Persistence.Repositories;

using GestionComercial.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repositorio para Auditoría (Registro completo de operaciones)
/// </summary>
public class AuditoriaRepository : Repository<Auditoria>
{
    public AuditoriaRepository(GestionComercialContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene registros de auditoría por usuario
    /// </summary>
    public async Task<IEnumerable<Auditoria>> ObtenerPorUsuarioAsync(int idUsuario, int? limitUltimos = null)
    {
        IQueryable<Auditoria> query = _dbSet
            .Where(a => a.ID_Usuario == idUsuario)
            .Include(a => a.Usuario)
            .OrderByDescending(a => a.FechaOperacion);

        if (limitUltimos.HasValue)
            query = query.Take(limitUltimos.Value);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Obtiene registros de auditoría por tabla/entidad
    /// </summary>
    public async Task<IEnumerable<Auditoria>> ObtenerPorEntidadAsync(string tabla, int idEntidad)
    {
        return await _dbSet
            .Where(a => a.Tabla == tabla && a.RegistroID == idEntidad.ToString())
            .Include(a => a.Usuario)
            .OrderByDescending(a => a.FechaOperacion)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene registros de auditoría por rango de fechas
    /// </summary>
    public async Task<IEnumerable<Auditoria>> ObtenerPorFechaAsync(DateTime desde, DateTime hasta, int? idUsuario = null, string? tipoOperacion = null)
    {
        IQueryable<Auditoria> query = _dbSet
            .Where(a => a.FechaOperacion >= desde && a.FechaOperacion <= hasta)
            .Include(a => a.Usuario)
            .OrderByDescending(a => a.FechaOperacion);

        if (idUsuario.HasValue)
            query = query.Where(a => a.ID_Usuario == idUsuario);

        if (!string.IsNullOrEmpty(tipoOperacion))
            query = query.Where(a => a.TipoOperacion == tipoOperacion);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Obtiene registros de auditoría por tipo de operación
    /// </summary>
    public async Task<IEnumerable<Auditoria>> ObtenerPorOperacionAsync(string operacion)
    {
        return await _dbSet
            .Where(a => a.TipoOperacion == operacion)
            .Include(a => a.Usuario)
            .OrderByDescending(a => a.FechaOperacion)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene registros de auditoría de INSERTS
    /// </summary>
    public async Task<IEnumerable<Auditoria>> ObtenerInsertesAsync(int? limitUltimos = null)
    {
        IQueryable<Auditoria> query = _dbSet
            .Where(a => a.TipoOperacion == "INSERT")
            .Include(a => a.Usuario)
            .OrderByDescending(a => a.FechaOperacion);

        if (limitUltimos.HasValue)
            query = query.Take(limitUltimos.Value);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Obtiene registros de auditoría de UPDATES
    /// </summary>
    public async Task<IEnumerable<Auditoria>> ObtenerActualizacionesAsync(int? limitUltimos = null)
    {
        IQueryable<Auditoria> query = _dbSet
            .Where(a => a.TipoOperacion == "UPDATE")
            .Include(a => a.Usuario)
            .OrderByDescending(a => a.FechaOperacion);

        if (limitUltimos.HasValue)
            query = query.Take(limitUltimos.Value);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Obtiene registros de auditoría de DELETES
    /// </summary>
    public async Task<IEnumerable<Auditoria>> ObtenerEliminacionesAsync(int? limitUltimos = null)
    {
        IQueryable<Auditoria> query = _dbSet
            .Where(a => a.TipoOperacion == "DELETE")
            .Include(a => a.Usuario)
            .OrderByDescending(a => a.FechaOperacion);

        if (limitUltimos.HasValue)
            query = query.Take(limitUltimos.Value);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Obtiene el historial de cambios de una entidad específica
    /// </summary>
    public async Task<IEnumerable<Auditoria>> ObtenerHistorialEntidadAsync(string tabla, int idEntidad)
    {
        return await _dbSet
            .Where(a => a.Tabla == tabla && a.RegistroID == idEntidad.ToString())
            .Include(a => a.Usuario)
            .OrderBy(a => a.FechaOperacion)
            .ToListAsync();
    }

    /// <summary>
    /// Cuenta operaciones por usuario en período
    /// </summary>
    public async Task<int> ContarOperacionesPorUsuarioAsync(int idUsuario, DateTime desde, DateTime hasta)
    {
        return await _dbSet
            .Where(a => a.ID_Usuario == idUsuario &&
                        a.FechaOperacion >= desde &&
                        a.FechaOperacion <= hasta)
            .CountAsync();
    }

    /// <summary>
    /// Obtiene usuarios más activos en período
    /// </summary>
    public async Task<IEnumerable<(int IdUsuario, string NombreUsuario, int Operaciones)>> ObtenerUsuariosMasActivosAsync(DateTime desde, DateTime hasta, int top = 10)
    {
        var resultado = await _dbSet
            .Where(a => a.FechaOperacion >= desde && a.FechaOperacion <= hasta && a.ID_Usuario.HasValue)
            .GroupBy(a => a.ID_Usuario)
            .OrderByDescending(g => g.Count())
            .Take(top)
            .Select(g => new { IdUsuario = g.Key!.Value, Operaciones = g.Count() })
            .ToListAsync();

        var idUsuarios = resultado.Select(r => r.IdUsuario).ToList();
        var usuarios = await _context.Set<Usuario>()
            .Where(u => idUsuarios.Contains(u.ID_Usuario))
            .ToListAsync();

        return resultado.Select(r => (
            r.IdUsuario,
            usuarios.FirstOrDefault(u => u.ID_Usuario == r.IdUsuario)?.NombreCompleto ?? "Desconocido",
            r.Operaciones));
    }

    /// <summary>
    /// Obtiene distribución de operaciones por tipo
    /// </summary>
    public async Task<IEnumerable<(string Operacion, int Cantidad)>> ObtenerDistribucionOperacionesAsync(DateTime desde, DateTime hasta)
    {
        var resultado = await _dbSet
            .Where(a => a.FechaOperacion >= desde && a.FechaOperacion <= hasta)
            .GroupBy(a => a.TipoOperacion)
            .Select(g => new { Operacion = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        return resultado.Select(r => (r.Operacion, r.Cantidad));
    }

    /// <summary>
    /// Obtiene registros sospechosos (eliminaciones o accesos en horario nocturno)
    /// </summary>
    public async Task<IEnumerable<Auditoria>> ObtenerOperacionesSuspectasAsync()
    {
        return await _dbSet
            .Where(a =>
                a.TipoOperacion == "DELETE" ||
                a.FechaOperacion.Hour >= 22 ||
                a.FechaOperacion.Hour < 6)
            .Include(a => a.Usuario)
            .OrderByDescending(a => a.FechaOperacion)
            .ToListAsync();
    }
}
