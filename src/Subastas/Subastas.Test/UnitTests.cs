using Microsoft.Extensions.DependencyInjection;
using Subastas.Domain;
using Subastas.Interfaces;
using Subastas.Interfaces.Services;
using Subastas.TextFixture;
using System.Collections.ObjectModel;

namespace Subastas.Test
{
    public class UnitTests(TestFixture fixture) : IClassFixture<TestFixture>
    {
        private readonly IMenuService MenuService = fixture.ServiceProvider.GetService<IMenuService>()!;
        private readonly IPermisoService PermisoService = fixture.ServiceProvider.GetService<IPermisoService>()!;
        private readonly IRolService RolService = fixture.ServiceProvider.GetService<IRolService>()!;
        private readonly IUserService UserService = fixture.ServiceProvider.GetService<IUserService>()!;
        private readonly IEncryptionService EncrypManager = fixture.ServiceProvider.GetService<IEncryptionService>()!;
        private readonly IProductoService ProductoService = fixture.ServiceProvider.GetService<IProductoService>()!;
        private readonly ISubastaService SubastaService = fixture.ServiceProvider.GetService<ISubastaService>()!;

        [Fact(DisplayName = "01 - Crear usuario")]
        public async Task Test_01_RegisterUser_WithValidData_ShouldCreateUser()
        {
            var menu = await MenuService.CreateIfNotExistsAsync(new Menu
            {
                EstaActivo = true,
                NombreMenu = "Authentication",
            });

            var permiso = await PermisoService.CreateIfNotExistsAsync(new Permiso
            {
                EstaActivo = true,
                NombrePermiso = "Login",
                IdMenuNavigation = menu
            });

            var adminRole = await RolService.CreateIfNotExistsAsync(new Role
            {
                EstaActivo = true,
                NombreRol = "Admin",
                RolPermisos = [new RolPermiso
                {
                    IdPermiso = permiso.IdPermiso,
                    EstaActivo = true
                }]
            });

            var Alexis = await UserService.CreateIfNotExistsAsync(new Usuario
            {
                CorreoUsuario = "ia.alexismartinez@ufg.edu.sv",
                NombreUsuario = "Alexis",
                ApellidoUsuario = "Martínez",
                Cuentum = new Cuenta
                {
                    Saldo = 1000.00M,
                    EstaActivo = true
                },
                EstaActivo = true,
                Password = EncrypManager.Encrypt("MiContraseñaEnEntornoDev"),
                UsuarioRols = new Collection<UsuarioRol>
                {
                    new UsuarioRol
                    {
                        EstaActivo = true,
                        IdRol = adminRole.IdRol
                    }
                }
            });

            Assert.NotNull(Alexis);
        }

        [Fact(DisplayName = "02 - Login exitoso")]
        public async Task Test_02_Login_WithValidCredentials_ShouldReturnUser()
        {
            string email = "ia.alexismartinez@ufg.edu.sv";
            string password = "MiContraseñaEnEntornoDev";

            var user = await UserService.GetUserAndRoleByLogin(email, password);

            Assert.NotNull(user);
        }

        [Fact(DisplayName = "03 - Creación de Subasta")]
        public async Task Test_03_CreateAuction_AsAdmin_ShouldSucceed()
        {
            var producto = new Producto
            {
                NombreProducto = "Producto de Unit Testing",
                DescripcionProducto = "Producto utilizado en pruebas de unit testing para asociarlo a una subasta.",
                ImagenProducto = "dirección de una imagen ficticia"
            };

            producto = await ProductoService.CreateAsync(producto);

            var subasta = new Domain.Subasta
            {
                TituloSubasta = "Subasta en Unit Testing",
                MontoInicial = 100.00m,
                FechaSubasta = DateTime.Now.AddHours(1),
                FechaSubastaFin = DateTime.Now.AddDays(1),
                IdProducto = producto.IdProducto
            };

            subasta = await SubastaService.CreateAsync(subasta);

            Assert.NotNull(subasta);
            Assert.True(subasta.IdProducto > 0);
        }
    }
}
