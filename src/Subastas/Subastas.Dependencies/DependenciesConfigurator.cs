using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Subasta.Managers;
using Subastas.Database;
using Subastas.Interfaces;
using Subastas.Interfaces.Repositories;
using Subastas.Interfaces.Services;
using Subastas.Repositories;
using Subastas.Services;

namespace Subastas.Dependencies
{
    public static class DependenciesConfigurator
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration, bool testingBuilder = false)
        {
            // Configurar el contexto de base de datos con la cadena de conexión real
            if (testingBuilder)
            {
                services.AddDbContext<SubastasContext>(options =>
                options.UseInMemoryDatabase(databaseName: "UnitTestingDB"));
            }
            else
            {
                services.AddDbContext<SubastasContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SubastasConnectionString")));
            }

            // Configurar tus servicios aquí
            services.AddScoped<IUserService, UsersService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRolRepository, RolRepository>();
            services.AddScoped<IRolService, RolService>();
            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddScoped<IPermisoRepository, PermisoRepository>();
            services.AddScoped<IPermisoService, PermisoService>();
            services.AddScoped<IProductoRepository, ProductoRepository>();
            services.AddScoped<IProductoService, ProductoService>();
            services.AddScoped<IPujaRepository, PujaRepository>();
            services.AddScoped<ISubastaRepository, SubastaRepository>();
            services.AddScoped<ISubastaService, SubastaService>();
            services.AddScoped<IPujaService, PujaService>();
            services.AddScoped<IMenuRepository, MenuRepository>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IS3StorageService, S3StorageService>();
            services.AddScoped<ICuentaRepository, CuentaRepository>();
            services.AddScoped<ICuentaService, CuentaService>();
            services.AddScoped<IUsuarioRolRepository, UsuarioRolRepository>();
            services.AddScoped<IUsuarioRolService, UsuarioRolService>();
            services.AddScoped<ILogEntryRepository, LogEntryRepository>();
            return services;
        }
    }
}
