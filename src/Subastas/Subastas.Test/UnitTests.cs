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
        private readonly IPujaService PujaService = fixture.ServiceProvider.GetService<IPujaService>()!;
        private readonly ICuentaService CuentaService = fixture.ServiceProvider.GetService<ICuentaService>()!;

        [Fact(DisplayName = "01 - Crear usuario")]
        public async Task RegisterUser_WithValidData_ShouldCreateUser()
        {
            var alexis = await CreatUser();

            Assert.NotNull(alexis);
        }

        [Fact(DisplayName = "02 - Login exitoso")]
        public async Task Login_WithValidCredentials_ShouldReturnUser()
        {
            await CreatUser();

            string email = "ia.alexismartinez@ufg.edu.sv";
            string password = "MiContraseñaEnEntornoDev";

            var user = await UserService.GetUserAndRoleByLogin(email, password);

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

            if (producto == null)
                Assert.Fail("El producto con nombre \"Producto de Unit Testing\" no se encontro.");

            producto.DescripcionProducto = "Descripción modificada";
            producto.ImagenProducto = "Imagen modificada";

            var productoEditado = await ProductoService.UpdateAsync(producto);

            Assert.NotNull(producto);
            Assert.Equal("Descripción modificada", productoEditado.DescripcionProducto);
            Assert.Equal("Imagen modificada", productoEditado.ImagenProducto);
        }

        [Fact(DisplayName = "05 - Edición de Producto")]
        public async Task PlaceBid_WithValidAmount_ShouldUpdateAuction()
        {
            var usuario = await CreatUser();

            if (usuario == null)
                Assert.Fail("El usuario con correo \"ia.alexismartinez@ufg.edu.sv\" no se encontro.");

            var usuarioWithCuentum = await UserService.GetUserWithCuentum(usuario.IdUsuario);

            if (usuarioWithCuentum == null || usuarioWithCuentum.Cuentum == null)
                Assert.Fail($"El usuario con id {usuario.IdUsuario} no se encontro o no tiene cuenta.");

            var subasta = await CreateSubasta();

            if (subasta == null)
                Assert.Fail("La subasta con nombre \"Subasta en Unit Testing\" no se encontro.");

            var puja = new Puja()
            {
                FechaPuja = DateTime.Now,
                IdUsuario = usuario.IdUsuario,
                IdSubasta = subasta.IdSubasta,
                MontoPuja = 200.00m
            };

            if ((puja.MontoPuja - usuarioWithCuentum.Cuentum.Saldo) > 0)
            {
                Assert.Fail($"Saldo insuficiente del usuario {usuario.CorreoUsuario}");
            }

            var pujasSubasta = await PujaService.GetAllByPredicateAsync(x => x.IdSubasta == puja.IdSubasta);

            if (pujasSubasta.Any() && puja.MontoPuja <= pujasSubasta.Max(p => p.MontoPuja))
                Assert.Fail($"La puja actual: {pujasSubasta.Max(p => p.MontoPuja)} es mayor al monto enviado: {puja.MontoPuja}");

            decimal saldo = usuarioWithCuentum.Cuentum.Saldo - puja.MontoPuja;

            var cuenta = await CuentaService.GetByUserIdAsync(puja.IdUsuario);
            cuenta.Saldo = saldo;
            bool update = await CuentaService.UpdateCuenta(cuenta);

            if (!update)
                Assert.Fail($"No se actualizó el saldo del usuario {usuario.CorreoUsuario}");

            var nuevaPuja = await PujaService.CreateAsync(puja);

            Assert.NotNull(nuevaPuja);
            Assert.Equal(200.00m, nuevaPuja.MontoPuja);
        }

        // METODOS AUXILIARES
        public async Task<Usuario> CreatUser()
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

            return await UserService.CreateIfNotExistsAsync(new Usuario
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
        }

        public async Task<Producto> CreateProducto()
        {
            var producto = new Producto
            {
                NombreProducto = "Producto de Unit Testing",
                DescripcionProducto = "Producto utilizado en pruebas de unit testing para asociarlo a una subasta.",
                ImagenProducto = "dirección de una imagen ficticia"
            };

            return await ProductoService.CreateAsync(producto);
        }

        public async Task<Domain.Subasta> CreateSubasta()
        {
            var producto = await CreateProducto();

            var subasta = new Domain.Subasta
            {
                TituloSubasta = "Subasta en Unit Testing",
                MontoInicial = 100.00m,
                FechaSubasta = DateTime.Now,
                FechaSubastaFin = DateTime.Now.AddDays(1),
                IdProducto = producto.IdProducto
            };

            return await SubastaService.CreateAsync(subasta);
        }
    }
}
