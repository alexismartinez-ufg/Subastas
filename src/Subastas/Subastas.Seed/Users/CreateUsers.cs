﻿using Microsoft.Extensions.DependencyInjection;
using Subastas.Domain;
using Subastas.Interfaces;
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
            
            var emerson = await userService.CreateIfNotExistsAsync(new Usuario
            {
                CorreoUsuario = "ia.leonardorivas@ufg.edu.sv",
                NombreUsuario = "Emerson",
                ApellidoUsuario = "Rivas",
                Cuentum = new Cuenta
                {
                    Saldo = 1000.00M,
                    EstaActivo = true
                },
                EstaActivo = true,
                Password = encrypManager.Encrypt("contraEmerson"),
                UsuarioRols = new Collection<UsuarioRol>
                {
                    new UsuarioRol
                    {
                        EstaActivo = true,
                        IdRol = adminRole.IdRol
                    }
                }
            });

            var guillermo = await userService.CreateIfNotExistsAsync(new Usuario
            {
                CorreoUsuario = "ia.minero@ufg.edu.sv",
                NombreUsuario = "Guillermo",
                ApellidoUsuario = "Minero",
                Cuentum = new Cuenta
                {
                    Saldo = 1000.00M,
                    EstaActivo = true
                },
                EstaActivo = true,
                Password = encrypManager.Encrypt("contra123"),
                UsuarioRols = new Collection<UsuarioRol>
                {
                    new UsuarioRol
                    {
                        EstaActivo = true,
                        IdRol = adminRole.IdRol
                    }
                }
            });

            var Lisbeth = await userService.CreateIfNotExistsAsync(new Usuario
            {
                CorreoUsuario = "ia.lisbethargueta@ufg.edu.sv",
                NombreUsuario = "Lisbeth",
                ApellidoUsuario = "Argueta",
                Cuentum = new Cuenta
                {
                    Saldo = 5000.00M,
                    EstaActivo = true
                },
                EstaActivo = true,
                Password = encrypManager.Encrypt("contraLisbeth"),
                UsuarioRols = new Collection<UsuarioRol>
                {
                    new UsuarioRol
                    {
                        EstaActivo = true,
                        IdRol = adminRole.IdRol
                    }
                }
            });

            Assert.True(emerson != null && Alexis != null && Alfredo != null && guillermo != null && Lisbeth != null);
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
                CorreoUsuario = "csamayoa@ufg.edu.sv",
                NombreUsuario = "Carmen",
                ApellidoUsuario = "Sammayoa",
                Cuentum = new Cuenta
                {
                    Saldo = 1000.00M,
                    EstaActivo = true
                },
                EstaActivo = true,
                Password = encrypManager.Encrypt("Pass12345"),
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
