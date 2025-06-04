using Microsoft.Extensions.DependencyInjection;
using Subastas.Domain;
using Subastas.Interfaces;
using Subastas.TextFixture;
using System.Collections.ObjectModel;

namespace Subastas.Seed
{
    public class DataSeed(TestFixture fixture) : IClassFixture<TestFixture>
    {
        private readonly IMenuService menuService = fixture.ServiceProvider.GetRequiredService<IMenuService>();
        private readonly IPermisoService permisoService = fixture.ServiceProvider.GetRequiredService<IPermisoService>();
        private readonly IRolService rolService = fixture.ServiceProvider.GetRequiredService<IRolService>();
        private readonly IUserService userService = fixture.ServiceProvider.GetRequiredService<IUserService>();
        private readonly IEncryptionService encrypManager = fixture.ServiceProvider.GetRequiredService<IEncryptionService>();

        [Fact(DisplayName = "Init Data Seeder")]
        public async Task Init()
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
