using Microsoft.Extensions.DependencyInjection;
using Subastas.Domain;
using Subastas.Interfaces;
using Subastas.TextFixture;
using System.Collections.ObjectModel;

namespace Subastas.Seed.Roles
{
    public class CreateRoles : IClassFixture<TestFixture>
    {
        private readonly IRolService rolService;
        private readonly IPermisoService permisoService;

        public CreateRoles(TestFixture fixture)
        {
            rolService = fixture.ServiceProvider.GetService<IRolService>();
            permisoService = fixture.ServiceProvider.GetService<IPermisoService>();
        }

        [Fact]
        public async Task CreateRolesSeed()
        {
            var admin = await rolService.CreateIfNotExistsAsync(new Role
            {
                EstaActivo = true,
                NombreRol = "Admin",
                RolPermisos = (await permisoService.GetAllAsync()).Select(p => new RolPermiso
                {
                    IdPermiso = p.IdPermiso,
                    EstaActivo = true
                }).ToList()
            });

            var user = await rolService.CreateIfNotExistsAsync(new Role
            {
                EstaActivo = true,
                NombreRol = "User",
                RolPermisos = new Collection<RolPermiso>
                {
                    new RolPermiso
                    {
                        IdPermiso = (await permisoService.GetByNameAsync("Login")).IdPermiso
                    },
                    new RolPermiso
                    {
                        IdPermiso = (await permisoService.GetByNameAsync("Logout")).IdPermiso
                    }
                }
            });

            var roles = new Role[] { admin, user }; 

            Assert.True(roles.Any() && roles.Length == 2);
        }
    }
}
