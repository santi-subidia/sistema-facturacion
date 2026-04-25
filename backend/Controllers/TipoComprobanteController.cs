using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class TipoComprobanteController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly Backend.Services.Interfaces.ICacheService _cacheService;

        public TipoComprobanteController(AppDbContext db, Backend.Services.Interfaces.ICacheService cacheService) 
        { 
            _db = db;
            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _cacheService.GetOrCreateAsync(
                $"Catalogo:TipoComprobante:Page:{page}:Size:{pageSize}",
                async () =>
                {
                    var query = _db.TiposComprobantes
                        .OrderBy(t => t.Nombre);

                    var totalItems = await query.CountAsync();
                    var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                    var tipos = await query
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    return new
                    {
                        Data = tipos,
                        Pagination = new
                        {
                            CurrentPage = page,
                            PageSize = pageSize,
                            TotalItems = totalItems,
                            TotalPages = totalPages,
                            HasPrevious = page > 1,
                            HasNext = page < totalPages
                        }
                    };
                },
                TimeSpan.FromMinutes(5));

            return Ok(result);
        }

        [HttpGet("habilitados")]
        public async Task<IActionResult> GetHabilitados()
        {
            var config = await _db.AfipConfiguraciones
                .FirstOrDefaultAsync(c => c.Activa);

            if (config == null)
            {
                // Si no hay configuración activa, no mostramos tipos habilitados (o se podría decidir mostrar todos)
                // Para seguir la lógica estricta: devolver vacío.
                return Ok(new List<TipoComprobante>());
            }

            var habilitados = await _cacheService.GetOrCreateAsync(
                "Catalogo:TipoComprobante:Habilitados",
                async () =>
                {
                    return await _db.AfipTiposComprobantesHabilitados
                        .Include(t => t.TipoComprobante)
                        .Where(t => t.IdAfipConfiguracion == config!.Id && t.Habilitado)
                        .Select(t => t.TipoComprobante)
                        .OrderBy(t => t!.Nombre)
                        .ToListAsync();
                },
                TimeSpan.FromMinutes(10));

            return Ok(habilitados);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var tipo = await _db.TiposComprobantes.FindAsync(id);

            if (tipo == null)
            {
                return NotFound();
            }

            return Ok(tipo);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create(TipoComprobante tipo)
        {
            _db.TiposComprobantes.Add(tipo);
            await _db.SaveChangesAsync();

            _cacheService.RemoveByPrefix("Catalogo:TipoComprobante");

            return CreatedAtAction(nameof(GetById), new { id = tipo.Id }, tipo);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Update(int id, TipoComprobante tipo)
        {
            if (id != tipo.Id)
            {
                return BadRequest();
            }

            _db.Entry(tipo).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.TiposComprobantes.AnyAsync(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _cacheService.RemoveByPrefix("Catalogo:TipoComprobante");

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
             var tipo = await _db.TiposComprobantes.FindAsync(id);
            if (tipo == null)
                return NotFound();

            _db.TiposComprobantes.Remove(tipo);
            
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                 // Check if it's used
                 return BadRequest("No se puede eliminar el tipo de comprobante porque está siendo utilizado.");
            }

            _cacheService.RemoveByPrefix("Catalogo:TipoComprobante");

            return NoContent();
        }

        private bool NombreExists(string nombre)
        {
            return _db.TiposComprobantes.Any(t => t.Nombre == nombre);
        }

        private bool NombreExistsExcluding(string nombre, int excludeId)
        {
            return _db.TiposComprobantes.Any(t => t.Nombre == nombre && t.Id != excludeId);
        }

        private bool TipoComprobanteExists(int id)
        {
            return _db.TiposComprobantes.Any(t => t.Id == id);
        }
    }
}
