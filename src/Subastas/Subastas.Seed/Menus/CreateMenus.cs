using Microsoft.Extensions.DependencyInjection;
using Subastas.Domain;
using Subastas.Interfaces;
using Subastas.TextFixture;

namespace Subastas.Seed.Menus
{
    public class CreateMenus : IClassFixture<TestFixture>
    {
        private readonly IMenuService menuService;

        public CreateMenus(TestFixture fixture)
        {
            menuService = fixture.ServiceProvider.GetService<IMenuService>();
        }

        [Fact]
        public async Task CreateMenusSeed()
        {
            var Menus = new List<Menu>()
            {
                await menuService.CreateIfNotExistsAsync(new Menu
                {
                    EstaActivo = true,
                    NombreMenu = "Authentication",                 
                }),
                await menuService.CreateIfNotExistsAsync(new Menu
                {
                    EstaActivo = true,
                    NombreMenu = "Subastas"
                })
            };

            Assert.True(Menus.Any() && Menus.Count == 2);
        }
    }
}
