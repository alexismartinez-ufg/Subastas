using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Subastas.Domain;
using Subastas.Dto.Producto;

namespace Subastas.Interfaces.Services
{
    public interface IProductoService
    {
        Task<IEnumerable<Producto>> GetAllWithImageUrlsAsync();
        Task<IEnumerable<Producto>> GetAllAsync();
        Task<IEnumerable<Producto>> GetAllByPredicateAsync(Expression<Func<Producto, bool>> predicate);
        Task<Producto> CreateWithImageAsync(ProductoCreateRequest request, IFormFile imageFile);
        Task<Producto> UpdateWithImageAsync(int id, ProductoCreateRequest productoToUpdate, IFormFile imagen);
        Task<Producto> UpdateAsync(Producto productoToUpdate);
        Task<Producto> CreateAsync(Producto newProducto);
        Task<Producto> CreateIfNotExistsAsync(Producto newProducto);
        Task<bool> ExistsByIdAsync(int idProducto);
        Task<bool> ExistsByNameAsync(string ProductoName);
        Task<Producto> GetByIdAsync(int idProducto);
        Task<Producto> GetByNameAsync(string ProductoName);
        Task<bool> DeleteById(int idProducto);
        Task<bool> SoftDeleteAsync(int id);
        Task<Producto> GetByIdWithImageUrlAsync(int idProducto);
    }
}
