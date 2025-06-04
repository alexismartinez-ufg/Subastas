using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Subasta.Managers;
using Subastas.Interfaces;
using Subastas.Interfaces.Services;
using System.Data.Common;
using System.Text;

namespace Subastas.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class SubastaController : Controller
    {
        private readonly IProductoService _productoService;
        private readonly ISubastaService _subastaService;
        private readonly IUserService _userService;
        private readonly IHubContext<SubastaHub> _hubContext;

        public SubastaController(
            IProductoService productoService,
            ISubastaService subastaService,
            IUserService userService,
            IHubContext<SubastaHub> hubContext)
        {
            _productoService = productoService;
            _subastaService = subastaService;
            _userService = userService;
            _hubContext = hubContext as IHubContext<SubastaHub>; // Conversión explícita
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                IEnumerable<Domain.Subasta> listaSubasta;

                if (User.IsInRole("Admin"))
                {
                    listaSubasta = await _subastaService.GetCollectionByPredicateWithIncludesAsync(s => s.EstaActivo || !s.EstaActivo, x => x.Pujas);
                }
                else
                {
                    listaSubasta = await _subastaService.GetCollectionByPredicateWithIncludesAsync(s => s.EstaActivo, x => x.Pujas);
                }

                // Para cada subasta en la lista
                foreach (var subasta in listaSubasta)
                {
                    if (subasta.Pujas.Any())
                    {
                        var ultimaPuja = subasta.Pujas.OrderByDescending(p => p.FechaPuja).First();

                        subasta.MontoInicial = ultimaPuja.MontoPuja;
                    }
                    else
                    {
                        subasta.MontoInicial = 0;
                    }
                }

                listaSubasta = await _subastaService.SetToListProductoWithImgPreloaded(listaSubasta.ToList());
                return View(listaSubasta);
            }
            catch (DbException ex)
            {
                // TODO EVENTLOGEOLOGEOSO PERRASSSSSS
                ModelState.AddModelError(string.Empty, "Error de base de datos: " + ex.Message);
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error: " + ex.Message);
                return View();
            }
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles ="Admin")]
        public async Task<IActionResult> Create()
        {
            ViewData["IdProducto"] = new SelectList(await _productoService.GetAllByPredicateAsync(p => !p.EstaSubastado && p.EstaActivo && p.Subasta.IdProducto != p.IdProducto), "IdProducto", "NombreProducto");
            return View(new Domain.Subasta());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("TituloSubasta,MontoInicial,FechaSubasta,FechaSubastaFin,IdProducto")] Domain.Subasta subasta)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveSubastaUpdate");
                    await _subastaService.CreateAsync(subasta);
                    return RedirectToAction(nameof(Index));
                }

                ViewData["IdProducto"] = new SelectList(await _productoService.GetAllByPredicateAsync(p => !p.EstaSubastado && p.EstaActivo && p.Subasta.IdProducto != p.IdProducto), "IdProducto", "NombreProducto");
            }
            catch (Exception ex)
            {
                // LogError
            }

            return View(subasta);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> Details(int id)
        {
            var subasta = new Domain.Subasta();
            try
            {
                subasta = await _subastaService.GetSubastaWithPujaAndUsers(id) ?? subasta;
                subasta = await _subastaService.SetProductoWithImgPreloaded(subasta);

                if (subasta != null)
                {
                    decimal? maxMontoPuja = subasta.Pujas.Any() ? (decimal?)subasta.Pujas.Max(s => s.MontoPuja) : null;
                    ViewBag.MaxMontoPuja = maxMontoPuja;

                    if (maxMontoPuja.HasValue)
                    {
                        // Obtener el IdUsuario correspondiente al monto de puja más alto
                        int idUsuarioMaxPuja = subasta.Pujas.First(p => p.MontoPuja == maxMontoPuja).IdUsuario;

                        // Obtener el nombre del usuario con el IdUsuario encontrado
                        var usuarioConMaxPuja = await _userService.GetByIdAsync(idUsuarioMaxPuja);
                        ViewBag.NombreUsuarioMaxPuja = usuarioConMaxPuja.NombreUsuario;
                    }
                }
                return View(subasta);
            }
            catch (Exception)
            {
                return View(subasta);
            }
        }


        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var subasta = await _subastaService.GetByIdAsync(id);
            if (subasta == null)
            {
                 return RedirectToAction(nameof(Index));
            }

            ViewData["IdProducto"] = new SelectList(await _productoService.GetAllByPredicateAsync(p => !p.EstaSubastado && p.EstaActivo && (p.Subasta.IdProducto != p.IdProducto || p.Subasta.IdSubasta == id)), "IdProducto", "NombreProducto", subasta.IdProducto);
            return View("Create", subasta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("IdSubasta,TituloSubasta,MontoInicial,FechaSubasta,FechaSubastaFin,IdProducto,EstaActivo,Finalizada")] Domain.Subasta subasta)
        {
            if (id != subasta.IdSubasta)
            {
                 return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveSubastaUpdate");
                    await _subastaService.UpdateAsync(subasta);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // LogError
                }
            }

            ViewData["IdProducto"] = new SelectList(await _productoService.GetAllByPredicateAsync(p => !p.EstaSubastado && p.EstaActivo && (p.Subasta.IdProducto != p.IdProducto || p.Subasta.IdSubasta == id)), "IdProducto", "NombreProducto", subasta.IdProducto);
            return View("Create", subasta);
        }


        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = new { success = false, errorMessage = "" };

            try
            {
                var subasta = await _subastaService.GetSubastaWithPujaAndUsers(id);

                var usersPujas = subasta.Pujas
                    .GroupBy(p => p.IdUsuario)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        SumaDePujas = g.Sum(p => p.MontoPuja)
                    })
                    .ToList();

                var deleted = await _subastaService.DeleteById(id);

                if (deleted)
                {
                    foreach (var user in usersPujas)
                    {
                        var usuario = await _userService.GetUserWithCuentum(user.UserId);

                        usuario.Cuentum.Saldo += user.SumaDePujas;

                        await _userService.UpdateAsync(usuario);
                    }

                    await _hubContext.Clients.All.SendAsync("ReceiveSubastaUpdate");
                    result = new { success = true, errorMessage = "" };
                }
                else
                {
                    result = new { success = false, errorMessage = "Parece que hubo un error" };
                }
            }
            catch (Exception ex)
            {
                result = new { success = false, errorMessage = ex.Message };
            }

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> TerminarSubasta(int idSubasta)
        {
            try
            {
                var subasta = await _subastaService.GetSubastaWithPujaAndUsers(idSubasta);

                if (subasta == null)
                {
                    return NotFound();
                }

                subasta.EstaActivo = false;
                var ganador = subasta.Pujas.OrderByDescending(p => p.MontoPuja).FirstOrDefault()?.IdUsuarioNavigation;

                if (ganador == null)
                {
                    return NotFound();
                }

                subasta.IdUsuario = ganador.IdUsuario;

                await _subastaService.UpdateAsync(subasta);

                string administrador = User.Identity.Name;

                await _hubContext.Clients.Group(subasta.IdSubasta.ToString()).SendAsync("ReceiveSubastaTerminada", administrador, ganador.NombreUsuario);

                return Ok(new {success = true});
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> Export(string format)
        {
            List<SubastaExportModel> subastas;

            try
            {
                subastas = (await _subastaService.GetAllAsync())
                                .Where(s => s.EstaActivo)
                                .Select(s => new SubastaExportModel
                                {
                                    TituloSubasta = s.TituloSubasta,
                                    MontoInicial = s.MontoInicial.ToString(),
                                    FechaInicio = s.FechaSubasta.ToString(),
                                    FechaFin = s.FechaSubastaFin.ToString(),
                                    Activo = s.EstaActivo ? "Sí" : "No",
                                    Producto = s.IdProducto.ToString(),
                                })
                                .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener las subastas: {ex.Message}");
                return StatusCode(500, "Error al obtener las subastas");
            }

            if (subastas == null || !subastas.Any())
            {
                return NotFound("No hay subastas para exportar.");
            }

            if (format == "xlsx")
            {
                byte[] fileContents;
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Subastas");
                        worksheet.Cell(1, 1).Value = "TituloSubasta";
                        worksheet.Cell(1, 2).Value = "MontoInicial";
                        worksheet.Cell(1, 3).Value = "FechaInicio";
                        worksheet.Cell(1, 4).Value = "FechaFin";
                        worksheet.Cell(1, 5).Value = "Activo";
                        worksheet.Cell(1, 6).Value = "Producto";

                        int row = 2;
                        foreach (var subasta in subastas)
                        {
                            worksheet.Cell(row, 1).Value = subasta.TituloSubasta;
                            worksheet.Cell(row, 2).Value = subasta.MontoInicial;
                            worksheet.Cell(row, 3).Value = subasta.FechaInicio;
                            worksheet.Cell(row, 4).Value = subasta.FechaFin;
                            worksheet.Cell(row, 5).Value = subasta.Activo;
                            worksheet.Cell(row, 6).Value = subasta.Producto;
                            row++;
                        }

                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            fileContents = stream.ToArray();
                        }
                    }

                    return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "subastas.xlsx");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al generar el archivo Excel: {ex.Message}");
                    return StatusCode(500, "Error al generar el archivo Excel");
                }
            }
            else if (format == "json")
            {
                try
                {
                    var json = JsonConvert.SerializeObject(subastas, Newtonsoft.Json.Formatting.Indented);
                    var jsonBytes = Encoding.UTF8.GetBytes(json);

                    return File(jsonBytes, "application/json", "subastas.json");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al generar el archivo JSON: {ex.Message}");
                    return StatusCode(500, "Error al generar el archivo JSON");
                }
            }

            return NotFound();
        }

        public class SubastaExportModel
        {
            public string TituloSubasta { get; set; }
            public string MontoInicial { get; set; }
            public string FechaInicio { get; set; }
            public string FechaFin { get; set; }
            public string Activo { get; set; }
            public string Producto { get; set; }
        }
    }
}