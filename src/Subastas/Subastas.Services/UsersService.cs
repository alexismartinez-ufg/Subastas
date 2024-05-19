﻿using Microsoft.EntityFrameworkCore;
using Subastas.Domain;
using Subastas.Interfaces;

namespace Subastas.Services
{
    public class UsersService(IUserRepository userRepo) : IUserService
    {
        public async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            return await userRepo.GetAllAsync();
        }

        public async Task<Usuario> CreateAsync(Usuario newUsuario)
        {
            if (newUsuario == null)
                return null;

            await userRepo.AddAsync(newUsuario);

            return newUsuario;
        }

        public async Task<Usuario> CreateIfNotExistsAsync(Usuario newUsuario)
        {
            if (newUsuario == null)
                return null;

            if (!string.IsNullOrEmpty(newUsuario.CorreoUsuario) && await ExistsByCorreoAsync(newUsuario.CorreoUsuario))
                return await GetByCorreoAsync(newUsuario.CorreoUsuario);

            if (newUsuario.IdUsuario > 0 && await ExistsByIdAsync(newUsuario.IdUsuario))
                return await GetByIdAsync(newUsuario.IdUsuario);

            return await CreateAsync(newUsuario);
        }

        public async Task<bool> ExistsByCorreoAsync(string correo)
        {
            return await userRepo.ExistsByPredicate(u => EF.Functions.Like(u.CorreoUsuario, correo));
        }

        public async Task<bool> ExistsByIdAsync(int usuario)
        {
            return await userRepo.ExistsByPredicate(u => u.IdUsuario.Equals(usuario));
        }

        public async Task<Usuario> GetByCorreoAsync(string correo)
        {
            return await userRepo.GetByPredicate(u => EF.Functions.Like(u.CorreoUsuario, correo));
        }

        public async Task<Usuario> GetByIdAsync(int usuario)
        {
            return await userRepo.GetByIdAsync(usuario);
        }
    }
}
