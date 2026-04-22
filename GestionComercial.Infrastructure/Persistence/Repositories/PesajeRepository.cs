namespace GestionComercial.Infrastructure.Persistence.Repositories;

using GestionComercial.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repositorio para Pesaje
/// </summary>
public class PesajeRepository : Repository<Pesaje>
{
    public PesajeRepository(GestionComercialContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene pesaje por número de serie
    /// </summary>
    public async Task<Pesaje?> ObtenerPorNumeroAsync(string numeroPesaje)
    {
        return await _dbSet
            .Include(p => p.Vehiculo)
            .ThenInclude(v => v.Cliente)
            .FirstOrDefaultAsync(p => p.NumeroSerie == numeroPesaje);
    }

    /// <summary>
    /// Obtiene pesajes de un vehículo
    /// </summary>
    public async Task<IEnumerable<Pesaje>> ObtenerPorVehiculoAsync(int idVehiculo, int? limitUltimos = null)
    {
        IQueryable<Pesaje> query = _dbSet
            .Where(p => p.ID_Vehiculo == idVehiculo)
            .Include(p => p.Vehiculo)
            .OrderByDescending(p => p.FechaPesaje);

        if (limitUltimos.HasValue)
            query = query.Take(limitUltimos.Value);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Obtiene último pesaje de un vehículo (para calibración)
    /// </summary>
    public async Task<Pesaje?> ObtenerUltimoPesajeAsync(int idVehiculo)
    {
        return await _dbSet
            .Where(p => p.ID_Vehiculo == idVehiculo)
            .OrderByDescending(p => p.FechaPesaje)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Obtiene pesajes por rango de fechas
    /// </summary>
    public async Task<IEnumerable<Pesaje>> ObtenerPorFechaAsync(DateTime desde, DateTime hasta, int? idVehiculo = null)
    {
        IQueryable<Pesaje> query = _dbSet
            .Where(p => p.FechaPesaje >= desde && p.FechaPesaje <= hasta)
            .Include(p => p.Vehiculo)
            .ThenInclude(v => v.Cliente)
            .OrderByDescending(p => p.FechaPesaje);

        if (idVehiculo.HasValue)
            query = query.Where(p => p.ID_Vehiculo == idVehiculo);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Obtiene pesajes de tipo TARA
    /// </summary>
    public async Task<IEnumerable<Pesaje>> ObtenerTarasAsync()
    {
        return await _dbSet
            .Where(p => p.TipoPesaje == "TARA")
            .Include(p => p.Vehiculo)
            .OrderByDescending(p => p.FechaPesaje)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene pesajes de tipo BRUTO
    /// </summary>
    public async Task<IEnumerable<Pesaje>> ObtenerBrutosAsync()
    {
        return await _dbSet
            .Where(p => p.TipoPesaje == "BRUTO")
            .Include(p => p.Vehiculo)
            .OrderByDescending(p => p.FechaPesaje)
            .ToListAsync();
    }

    /// <summary>
    /// Verifica si existe el número de serie de pesaje
    /// </summary>
    public async Task<bool> ExisteNumeroAsync(string numeroPesaje)
    {
        return await _dbSet.AnyAsync(p => p.NumeroSerie == numeroPesaje);
    }

    /// <summary>
    /// Obtiene pares TARA/BRUTO para cálculo de peso neto
    /// </summary>
    public async Task<IEnumerable<(Pesaje Tara, Pesaje Bruto, decimal PesoNeto)>> ObtenerParesCalibradosAsync(int idVehiculo, int minutosMaximoEntrePesajes = 30)
    {
        var taras = await _dbSet
            .Where(p => p.ID_Vehiculo == idVehiculo && p.TipoPesaje == "TARA")
            .OrderByDescending(p => p.FechaPesaje)
            .ToListAsync();

        var brutos = await _dbSet
            .Where(p => p.ID_Vehiculo == idVehiculo && p.TipoPesaje == "BRUTO")
            .OrderByDescending(p => p.FechaPesaje)
            .ToListAsync();

        var pares = new List<(Pesaje Tara, Pesaje Bruto, decimal PesoNeto)>();
        foreach (var bruto in brutos)
        {
            var taraAntecedente = taras
                .Where(t => t.FechaPesaje <= bruto.FechaPesaje)
                .OrderByDescending(t => t.FechaPesaje)
                .FirstOrDefault();

            if (taraAntecedente != null &&
                (bruto.FechaPesaje - taraAntecedente.FechaPesaje).TotalMinutes <= minutosMaximoEntrePesajes)
            {
                decimal pesoNeto = bruto.PesoKg - taraAntecedente.PesoKg;
                pares.Add((taraAntecedente, bruto, pesoNeto));
            }
        }

        return pares;
    }

    /// <summary>
    /// Obtiene promedio de peso neto vendido
    /// </summary>
    public async Task<decimal> ObtenerPromedioNeto(int idVehiculo)
    {
        var pares = await ObtenerParesCalibradosAsync(idVehiculo);
        return pares.Any() ? pares.Average(p => p.PesoNeto) : 0m;
    }
}
