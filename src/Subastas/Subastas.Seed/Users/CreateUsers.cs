using Microsoft.Extensions.DependencyInjection;
using Subastas.Domain;
using Subastas.Interfaces;
using Subastas.TextFixture;
using System.Collections.ObjectModel;

namespace Subastas.Seed.Users
{
    public class CreateUsers : IClassFixture<TestFixture>
    {
        private readonly IUserService userService;
        private readonly IRolService rolService;
        private readonly IEncryptionService encrypManager;

        public CreateUsers(TestFixture fixture)
        {
            rolService = fixture.ServiceProvider.GetService<IRolService>();
            userService = fixture.ServiceProvider.GetService<IUserService>();
            encrypManager = fixture.ServiceProvider.GetService<IEncryptionService>();
        }

        [Fact]
        public async Task CreateAdminUsersSeed()
        {
            var adminRole = await rolService.GetByNameAsync("Admin");

            var Alexis = await userService.CreateIfNotExistsAsync(new Usuario
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
                Password = encrypManager.Encrypt("MiContraseñaEnEntornoDev"),
                UsuarioRols = new Collection<UsuarioRol>
                {
                    new UsuarioRol
                    {
                        EstaActivo = true,
                        IdRol = adminRole.IdRol
                    }
                }
            });
            var Alfredo = await userService.CreateIfNotExistsAsync(new Usuario
            {
                CorreoUsuario = "ia.alfredoam@ufg.edu.sv",
                NombreUsuario = "Alfredo",
                ApellidoUsuario = "Alas",
                Cuentum = new Cuenta
                {
                    Saldo = 1000.00M,
                    EstaActivo = true
                },
                EstaActivo = true,
                Password = encrypManager.Encrypt("mipass"),
                UsuarioRols = new Collection<UsuarioRol>
                {
                    new UsuarioRol
                    {
                        EstaActivo = true,
                        IdRol = adminRole.IdRol
                    }
                }
            });
            var Chiristian = await userService.CreateIfNotExistsAsync(new Usuario
            {
                CorreoUsuario = "ia.christianp@ufg.edu.sv",
                NombreUsuario = "Christian",
                ApellidoUsuario = "Peña",
                Cuentum = new Cuenta
                {
                    Saldo = 1000.00M,
                    EstaActivo = true
                },
                EstaActivo = true,
                Password = encrypManager.Encrypt("1234"),
                UsuarioRols = new Collection<UsuarioRol>
                {
                    new UsuarioRol
                    {
                        EstaActivo = true,
                        IdRol = adminRole.IdRol
                    }
                }
            });
            var Caleb = await userService.CreateIfNotExistsAsync(new Usuario
            {
                CorreoUsuario = "ia.hrcaleb15@ufg.edu.sv",
                NombreUsuario = "Caleb",
                ApellidoUsuario = "Hernandez",
                Cuentum = new Cuenta
                {
                    Saldo = 1000.00M,
                    EstaActivo = true
                },
                EstaActivo = true,
                Password = encrypManager.Encrypt("1234"),
                UsuarioRols = new Collection<UsuarioRol>
                {
                    new UsuarioRol
                    {
                        EstaActivo = true,
                        IdRol = adminRole.IdRol
                    }
                }
            });
            var Oscar = await userService.CreateIfNotExistsAsync(new Usuario
            {
                CorreoUsuario = "ia.oscar90@ufg.edu.sv",
                NombreUsuario = "Oscar",
                ApellidoUsuario = "Minegro",
                Cuentum = new Cuenta
                {
                    Saldo = 1000.00M,
                    EstaActivo = true
                },
                EstaActivo = true,
                Password = encrypManager.Encrypt("1234"),
                UsuarioRols = new Collection<UsuarioRol>
                {
                    new UsuarioRol
                    {
                        EstaActivo = true,
                        IdRol = adminRole.IdRol
                    }
                }
            });

            Assert.True(Alexis != null && Alfredo != null && Chiristian != null && Caleb != null && Oscar != null);
        }

        [Fact]
        public async Task CreateUsersSeed()
        {
            var usuario = await rolService.GetByNameAsync("User");

            var usuario1 = await userService.CreateIfNotExistsAsync(new Usuario
            {
                CorreoUsuario = "usuario1@mail.com",
                NombreUsuario = "usuario",
                ApellidoUsuario = "1",
                Cuentum = new Cuenta
                {
                    Saldo = 1000.00M,
                    EstaActivo = true
                },
                EstaActivo = true,
                Password = encrypManager.Encrypt("1234"),
                UsuarioRols = new Collection<UsuarioRol>
                {
                    new UsuarioRol
                    {
                        EstaActivo = true,
                        IdRol = usuario.IdRol
                    }
                }
            });

            var usuario2 = await userService.CreateIfNotExistsAsync(new Usuario
            {
                CorreoUsuario = "usuario2@mail.com",
                NombreUsuario = "usuario",
                ApellidoUsuario = "2",
                Cuentum = new Cuenta
                {
                    Saldo = 1000.00M,
                    EstaActivo = true
                },
                EstaActivo = true,
                Password = encrypManager.Encrypt("1234"),
                UsuarioRols = new Collection<UsuarioRol>
                {
                    new UsuarioRol
                    {
                        EstaActivo = true,
                        IdRol = usuario.IdRol
                    }
                }
            });

            var usuario3 = await userService.CreateIfNotExistsAsync(new Usuario
            {
                CorreoUsuario = "jaimecortez@ufg.edu.sv",
                NombreUsuario = "Jaime",
                ApellidoUsuario = "Cortez",
                Cuentum = new Cuenta
                {
                    Saldo = 1000.00M,
                    EstaActivo = true
                },
                EstaActivo = true,
                Password = encrypManager.Encrypt("123456789"),
                UsuarioRols = new Collection<UsuarioRol>
                {
                    new UsuarioRol
                    {
                        EstaActivo = true,
                        IdRol = usuario.IdRol
                    }
                }
            });

            Assert.True(usuario1 != null && usuario2 != null && usuario3 != null);
        }
    }
}
