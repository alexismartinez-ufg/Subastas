﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Subastas.Domain;
using Subastas.Dto;
using Subastas.Interfaces;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace Subastas.Controllers
{
    public class UsuarioController(IUserService userService, IEncryptionService encrypManager, ICuentaService cuentaService, IConfiguration configuration) : Controller
    {

        // GET: UsuarioController
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var listaUsuario = await userService.GetAllAsync();
            return View(listaUsuario);
        }

        // GET: UsuarioController/Details/5
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult> Details(int id)
        {
            var usuario = await userService.GetByIdAsync(id);
            if (usuario == null)
            {
                return View(nameof(Index));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != usuario.IdUsuario.ToString() && !User.IsInRole("Admin"))
            {
                return View(nameof(Index));
            }

            var cuenta = await cuentaService.GetByUserIdAsync(id);
            if (cuenta == null)
            {
                return View(nameof(Index));
            }

            var usuarioCuentaViewModel = new UsuarioCuentaViewModel
            {
                Usuario = usuario,
                Cuenta = cuenta
            };

            return View(usuarioCuentaViewModel);
        }


        // GET: UsuarioController/Create
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public ActionResult Create()
        {
            return View(new UsuarioCuentaViewModel());
        }

        // POST: UsuarioController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult> Create(UsuarioCuentaViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    viewModel.Usuario.Password = encrypManager.Encrypt(viewModel?.Usuario?.Password);
                    await userService.CreateIfNotExistsAsync(viewModel.Usuario);
                    viewModel.Cuenta.IdUsuario = viewModel.Usuario.IdUsuario;
                    await cuentaService.CreateAsync(viewModel.Cuenta);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ModelState.AddModelError("", "Error al crear el usuario y la cuenta");
                }
            }
            return View(viewModel);
        }

        // GET: UsuarioController/Edit/5
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult> Edit(int id)
        {
            var usuario = await userService.GetByIdAsync(id);
            if (usuario == null)
            {
                return View(nameof(Index));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != usuario.IdUsuario.ToString() && !User.IsInRole("Admin"))
            {
                return View(nameof(Index));
            }

            var cuenta = await cuentaService.GetByUserIdAsync(id);
            if (cuenta == null)
            {
                return View(nameof(Index));
            }

            var usuarioCuentaViewModel = new UsuarioCuentaViewModel
            {
                Usuario = usuario,
                Cuenta = cuenta
            };

            return View("Create", usuarioCuentaViewModel);
        }

        // POST: UsuarioController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult> Edit(int id, UsuarioCuentaViewModel viewModel)
        {
            if (id != viewModel.Usuario.IdUsuario)
            {
                return View("Create", viewModel);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioDb = await userService.GetByIdAsync(id);
                    if (usuarioDb == null)
                    {
                        return View("Create", viewModel);
                    }

                    // Validar y asignar solo si el valor ha cambiado
                    if (!string.IsNullOrWhiteSpace(viewModel.Usuario.NombreUsuario) &&
                        usuarioDb.NombreUsuario != viewModel.Usuario.NombreUsuario)
                    {
                        usuarioDb.NombreUsuario = viewModel.Usuario.NombreUsuario;
                    }

                    if (!string.IsNullOrWhiteSpace(viewModel.Usuario.ApellidoUsuario) &&
                        usuarioDb.ApellidoUsuario != viewModel.Usuario.ApellidoUsuario)
                    {
                        usuarioDb.ApellidoUsuario = viewModel.Usuario.ApellidoUsuario;
                    }

                    if (!string.IsNullOrWhiteSpace(viewModel.Usuario.CorreoUsuario) &&
                        usuarioDb.CorreoUsuario != viewModel.Usuario.CorreoUsuario)
                    {
                        usuarioDb.CorreoUsuario = viewModel.Usuario.CorreoUsuario;
                    }

                    usuarioDb.EstaActivo = viewModel.Usuario.EstaActivo;

                    // Validar contraseña
                    if (!string.IsNullOrWhiteSpace(viewModel.Usuario.Password))
                    {
                        var decryptedPassword = encrypManager.Decrypt(usuarioDb.Password);
                        if (decryptedPassword != viewModel.Usuario.Password)
                        {
                            usuarioDb.Password = encrypManager.Encrypt(viewModel.Usuario.Password);
                        }
                    }

                    await userService.UpdateAsync(usuarioDb);

                    var cuentaDb = await cuentaService.GetByUserIdAsync(id);
                    if (cuentaDb == null)
                    {
                        return View("Create", viewModel);
                    }

                    cuentaDb.Saldo = viewModel.Cuenta.Saldo;
                    cuentaDb.EstaActivo = viewModel.Cuenta.EstaActivo;

                    await cuentaService.UpdateCuenta(cuentaDb);

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el usuario y la cuenta: " + ex.Message);
                }
            }

            return View("Create", viewModel);
        }

        // GET: UsuarioController/Delete/5
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            var usuario = await userService.GetByIdAsync(id);

            return View(usuario);
        }

        // POST: UsuarioController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                var borrado = await userService.DeleteById(id);

                if (borrado)
                {
                    return RedirectToAction(nameof(Index));
                }
                ViewData["Error"] = "Error al eliminar el usuario";
                return View();
            }
            catch
            {
                ViewData["Error"] = "Error al eliminar el usuario";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CargarMonto(int userId, decimal monto)
        {
            try
            {
                var usuario = await userService.GetUserWithCuentum(userId);

                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                usuario.Cuentum.Saldo += monto;

                await userService.UpdateAsync(usuario);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}
