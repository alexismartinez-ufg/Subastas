using Microsoft.Extensions.DependencyInjection;
using Subastas.Domain;
using Subastas.Interfaces;
using Subastas.TextFixture;
using System.Collections.ObjectModel;

namespace Subastas.Test.Auth
{
    public class _01_UserRegisterTest(TestFixture fixture) : IClassFixture<TestFixture>
    {
        private readonly IMenuService MenuService = fixture.ServiceProvider.GetService<IMenuService>()!;
        private readonly IPermisoService PermisoService = fixture.ServiceProvider.GetService<IPermisoService>()!;
        private readonly IRolService RolService = fixture.ServiceProvider.GetService<IRolService>()!;
        private readonly IUserService UserService = fixture.ServiceProvider.GetService<IUserService>()!;
        private readonly IEncryptionService EncrypManager = fixture.ServiceProvider.GetService<IEncryptionService>()!;

        [Fact]
        public async Task RegisterUser_WithValidData_ShouldCreateUser()
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
    }
}
