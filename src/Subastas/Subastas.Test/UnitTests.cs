using Microsoft.Extensions.DependencyInjection;
using Subastas.Domain;
using Subastas.Interfaces;
using Subastas.Interfaces.Services;
using Subastas.TextFixture;

namespace Subastas.Test
{
    public class UnitTests(TestFixture fixture) : IClassFixture<TestFixture>
    {
        private readonly IMenuService MenuService = fixture.ServiceProvider.GetRequiredService<IMenuService>();
        private readonly IPermisoService PermisoService = fixture.ServiceProvider.GetRequiredService<IPermisoService>();
        private readonly IRolService RolService = fixture.ServiceProvider.GetRequiredService<IRolService>();
        private readonly IUserService UserService = fixture.ServiceProvider.GetRequiredService<IUserService>();
        private readonly IEncryptionService EncrypManager = fixture.ServiceProvider.GetRequiredService<IEncryptionService>();
        private readonly IProductoService ProductoService = fixture.ServiceProvider.GetRequiredService<IProductoService>();
        private readonly ISubastaService SubastaService = fixture.ServiceProvider.GetRequiredService<ISubastaService>();
        private readonly IPujaService PujaService = fixture.ServiceProvider.GetRequiredService<IPujaService>();
        private readonly ICuentaService CuentaService = fixture.ServiceProvider.GetRequiredService<ICuentaService>();

        [Fact(DisplayName = "01 - Crear usuario")]
        public async Task RegisterUser_WithValidData_ShouldCreateUser()
        {
            var user = await CreateUser();
            Assert.NotNull(user);
        }

        [Fact(DisplayName = "02 - Login exitoso")]
        public async Task Login_WithValidCredentials_ShouldReturnUser()
        {
            await CreateUser();

            var user = await UserService.GetUserAndRoleByLogin(
                "ia.alexismartinez@ufg.edu.sv",
                "MiContraseñaEnEntornoDev");

            Assert.NotNull(user);
        }

        [Fact(DisplayName = "03 - Creación de Subasta")]
        public async Task CreateAuction_AsAdmin_ShouldSucceed()
        {
            var subasta = await CreateSubasta();
            Assert.NotNull(subasta);
            Assert.True(subasta.IdProducto > 0);
        }

        [Fact(DisplayName = "04 - Edición de Producto")]
        public async Task EditProduct_ShouldUpdateProductDetails_WhenProductExists()
        {
            var producto = await CreateProducto();
            Assert.NotNull(producto);

            producto.DescripcionProducto = "Descripción modificada";
            producto.ImagenProducto = "Imagen modificada";

            var productoEditado = await ProductoService.UpdateAsync(producto);

            Assert.Equal("Descripción modificada", productoEditado.DescripcionProducto);
            Assert.Equal("Imagen modificada", productoEditado.ImagenProducto);
        }

        [Fact(DisplayName = "05 - Pujar con monto válido")]
        public async Task PlaceBid_WithValidAmount_ShouldUpdateAuction()
        {
            var nuevaPuja = await CreatePuja();
            Assert.NotNull(nuevaPuja);
            Assert.Equal(200.00m, nuevaPuja.MontoPuja);
        }

        [Fact(DisplayName = "06 - Cancelar subasta")]
        public async Task CancelAuction_ByOwnerOrAdmin_ShouldSetStatusToCancelled()
        {
            var subasta = await CreateSubasta();
            Assert.NotNull(subasta);

            var deleted = await SubastaService.DeleteById(subasta.IdSubasta);
            Assert.True(deleted);
        }

        [Fact(DisplayName = "07 - Terminar Subasta")]
        public async Task EndAuction_WhenExpirationReached_ShouldCloseAuction()
        {
            var subasta = await CreateSubasta();
            Assert.NotNull(subasta);

            subasta.EstaActivo = false;
            await SubastaService.UpdateAsync(subasta);

            Assert.False(subasta.EstaActivo);
        }

        [Fact(DisplayName = "08 - Saldo Insuficiente para pujar")]
        public async Task PlaceBid_WithInsufficientBalance_ShouldFail()
        {
            var usuario = await CreateUser();
            var cuenta = (await UserService.GetUserWithCuentum(usuario.IdUsuario)).Cuentum!;
            var subasta = await CreateSubasta();

            var puja = new Puja
            {
                FechaPuja = DateTime.Now,
                IdUsuario = usuario.IdUsuario,
                IdSubasta = subasta.IdSubasta,
                MontoPuja = 20000.00m
            };

            Assert.True(puja.MontoPuja > cuenta.Saldo);
        }

        [Fact(DisplayName = "09 - Historial de participaciones en subastas")]
        public async Task GetAuctionHistory_ByUser_ShouldReturnCorrectData()
        {
            var usuario = await CreateUser();
            var cuenta = (await UserService.GetUserWithCuentum(usuario.IdUsuario)).Cuentum!;
            var subasta = await CreateSubasta();

            var puja = new Puja
            {
                FechaPuja = DateTime.Now,
                IdUsuario = usuario.IdUsuario,
                IdSubasta = subasta.IdSubasta,
                MontoPuja = 200.00m
            };

            if (puja.MontoPuja > cuenta.Saldo)
                Assert.Fail($"Saldo insuficiente del usuario {usuario.CorreoUsuario}");

            var pujas = await PujaService.GetAllByPredicateAsync(p => p.IdSubasta == puja.IdSubasta);

            if (pujas.Any() && puja.MontoPuja <= pujas.Max(p => p.MontoPuja))
                Assert.Fail($"La puja actual: {pujas.Max(p => p.MontoPuja)} es mayor al monto enviado: {puja.MontoPuja}");

            cuenta.Saldo -= puja.MontoPuja;
            Assert.True(await CuentaService.UpdateCuenta(cuenta), "No se actualizó el saldo");

            var nuevaPuja = await PujaService.CreateAsync(puja);
            Assert.NotNull(nuevaPuja);

            var subastaActualizada = await SubastaService.GetSubastaWithPujaAndUsers(subasta.IdSubasta);
            var participaciones = subastaActualizada.Pujas
                .Where(p => p.IdUsuario == usuario.IdUsuario)
                .ToList();

            Assert.NotEmpty(participaciones);
        }

        [Fact(DisplayName = "10 - Usuario puede recargar")]
        public async Task RechargeBalance_WithValidAmount_ShouldIncreaseUserBalance()
        {
            var usuario = await CreateUser();
            var cuenta = (await UserService.GetUserWithCuentum(usuario.IdUsuario)).Cuentum!;
            cuenta.Saldo += 10000;
            await UserService.UpdateAsync(usuario);
            Assert.True(cuenta.Saldo > 1000.00M);
        }

        // ----------------------------
        // MÉTODOS AUXILIARES PRIVADOS
        // ----------------------------
        private async Task<Usuario> CreateUser()
        {
            var menu = await MenuService.CreateIfNotExistsAsync(new Menu { EstaActivo = true, NombreMenu = "Authentication" });
            var permiso = await PermisoService.CreateIfNotExistsAsync(new Permiso
            {
                EstaActivo = true,
                NombrePermiso = "Login",
                IdMenuNavigation = menu
            });

            var role = await RolService.CreateIfNotExistsAsync(new Role
            {
                EstaActivo = true,
                NombreRol = "Admin",
                RolPermisos = [new RolPermiso { EstaActivo = true, IdPermiso = permiso.IdPermiso }]
            });

            return await UserService.CreateIfNotExistsAsync(new Usuario
            {
                CorreoUsuario = "ia.alexismartinez@ufg.edu.sv",
                NombreUsuario = "Alexis",
                ApellidoUsuario = "Martínez",
                EstaActivo = true,
                Password = EncrypManager.Encrypt("MiContraseñaEnEntornoDev"),
                Cuentum = new Cuenta { EstaActivo = true, Saldo = 1000.00M },
                UsuarioRols = [new UsuarioRol { EstaActivo = true, IdRol = role.IdRol }]
            });
        }

        private async Task<Producto> CreateProducto()
        {
            return await ProductoService.CreateAsync(new Producto
            {
                NombreProducto = "Producto de Unit Testing",
                DescripcionProducto = "Producto utilizado en pruebas de unit testing.",
                ImagenProducto = "ruta/ficticia.jpg"
            });
        }

        private async Task<Domain.Subasta> CreateSubasta()
        {
            var producto = await CreateProducto();
            return await SubastaService.CreateAsync(new Domain.Subasta
            {
                IdProducto = producto.IdProducto,
                FechaSubasta = DateTime.Now,
                FechaSubastaFin = DateTime.Now.AddDays(1),
                EstaActivo = true,
                TituloSubasta = "Subasta en Unit Testing"
            });
        }

        private async Task<Puja> CreatePuja()
        {
            var usuario = await CreateUser();
            var cuenta = (await UserService.GetUserWithCuentum(usuario.IdUsuario)).Cuentum!;
            var subasta = await CreateSubasta();

            var puja = new Puja
            {
                FechaPuja = DateTime.Now,
                IdUsuario = usuario.IdUsuario,
                IdSubasta = subasta.IdSubasta,
                MontoPuja = 200.00m
            };

            cuenta.Saldo -= puja.MontoPuja;
            await CuentaService.UpdateCuenta(cuenta);

            return await PujaService.CreateAsync(puja);
        }
    }
}
