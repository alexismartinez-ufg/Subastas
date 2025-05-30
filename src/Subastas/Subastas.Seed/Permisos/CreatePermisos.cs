using Microsoft.Extensions.DependencyInjection;
using Subastas.Domain;
using Subastas.Interfaces;
using Subastas.TextFixture;

namespace Subastas.Seed.Permisos
{
    public class CreatePermisos : IClassFixture<TestFixture>
    {
        private readonly IPermisoService permisoService;
        private readonly IMenuService menuService;

        public CreatePermisos(TestFixture fixture)
        {
            permisoService = fixture.ServiceProvider.GetService<IPermisoService>();
            menuService = fixture.ServiceProvider.GetService<IMenuService>();
        }

        [Fact]
        public async Task CreatePermisosSeed()
        {
            var permisos = new List<Permiso>()
            {
                await permisoService.CreateIfNotExistsAsync(new Permiso
                {
                    EstaActivo = true,
                    NombrePermiso = "Login",
                    IdMenuNavigation = await menuService.GetByNameAsync("Authentication")
                }),
                await permisoService.CreateIfNotExistsAsync(new Permiso
                {
                    EstaActivo = true,
                    NombrePermiso = "Logout",
                    IdMenuNavigation = await menuService.GetByNameAsync("Authentication")

                }),
                await permisoService.CreateIfNotExistsAsync(new Permiso
                {
                    EstaActivo = true,
                    NombrePermiso = "Create_Subastas",
                    IdMenuNavigation = await menuService.GetByNameAsync("Subastas")
                }),
                await permisoService.CreateIfNotExistsAsync(new Permiso
                {
                    EstaActivo = true,
                    NombrePermiso = "Add_Subastas",
                    IdMenuNavigation = await menuService.GetByNameAsync("Subastas")
                }),
                await permisoService.CreateIfNotExistsAsync(new Permiso
                {
                    EstaActivo = true,
                    NombrePermiso = "Edit_Subastas",
                    IdMenuNavigation = await menuService.GetByNameAsync("Subastas")
                }),
                await permisoService.CreateIfNotExistsAsync(new Permiso
                {
                    EstaActivo = true,
                    NombrePermiso = "Delete_Subastas",
                    IdMenuNavigation = await menuService.GetByNameAsync("Subastas")
                })
            };

            Assert.True(permisos.Any() && permisos.Count == 6);
        }
    }
}
