using Microsoft.EntityFrameworkCore;
using Subastas.Domain;
using Subastas.Interfaces;
using Subastas.Repositories;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Subastas.Dto.Producto;
using Subastas.Dto.S3;
using Subastas.Interfaces.Repositories;
using Subastas.Interfaces.Services;

namespace Subastas.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IS3StorageService _s3StorageService;
        private readonly AwsCredentials _awsCredentials;
        private readonly string _bucketName;
        public ProductoService(IProductoRepository productoRepository, IS3StorageService s3StorageService, IConfiguration config)
        {
            _productoRepository = productoRepository;
            _s3StorageService = s3StorageService;
            _awsCredentials = new AwsCredentials
            {
                AwsKey = config["AwsConfiguration:AwsAccessKey"],
                AwsSecretKey = config["AwsConfiguration:AwsSecretKey"]
            };
            _bucketName = config["AwsConfiguration:SubastasBucket"];
        }
        
        public async Task<IEnumerable<Producto>> GetAllAsync()
        {
            return await _productoRepository.GetAllAsync();
        }
        
        public async Task<IEnumerable<Producto>> GetAllActiveAsync()
        {
            return await _productoRepository.GetAllActiveAsync();
        }
        
        public async Task<IEnumerable<Producto>> GetAllWithImageUrlsAsync()
        {
            var productos = await GetAllActiveAsync();
            var allWithImageUrlsAsync = productos.ToList();
            foreach (var producto in allWithImageUrlsAsync)
            {
                if (!string.IsNullOrEmpty(producto.ImagenProducto))
                {
                    var s3File = new S3File
                    {
                        BucketName = _bucketName,
                        KeyName = producto.ImagenProducto
                    };
                    producto.ImagenProducto = _s3StorageService.GetUrlForObject(s3File, _awsCredentials);
                }
                else
                {
                    producto.ImagenProducto = null;
                }
            }
            return allWithImageUrlsAsync;
        }
        
        
        public async Task<Producto> GetByIdWithImageUrlAsync(int idProducto)
        {
            var producto = await _productoRepository.GetByIdAsync(idProducto);
            if (producto == null)
                return null;

            if (!string.IsNullOrEmpty(producto.ImagenProducto))
            {
                var s3File = new S3File
                {
                    BucketName = _bucketName,
                    KeyName = producto.ImagenProducto
                };
                producto.ImagenProducto = _s3StorageService.GetUrlForObject(s3File, _awsCredentials);
            }
            else
            {
                producto.ImagenProducto = null;
            }

            return producto;
        }

        public async Task<Producto> UpdateWithImageAsync(int id, ProductoCreateRequest productoToUpdate, IFormFile imagen)
        {
            if (productoToUpdate == null)
                return null;
            
            if(imagen is null || imagen.Length == 0)
            {
                throw new Exception("No valid image provided.");
            }
            
            try
            {
                using var stream = new MemoryStream();
                await imagen.CopyToAsync(stream);
                var newS3Object = new S3Object
                {
                    InputStream = stream,
                    Name = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName),
                    BucketName = _bucketName
                };

                var response = await _s3StorageService.UploadObjectAsync(newS3Object, _awsCredentials);
                if (response.StatusCode != 200)
                {
                    throw new Exception("Error uploading image to S3: " + response.Message);
                }
                
                var productoExistente = await _productoRepository.GetByIdAsync(id);
                
                var oldS3File = new S3File
                {
                    BucketName = _bucketName,
                    KeyName = productoExistente.ImagenProducto
                };
                
                await _s3StorageService.DeleteObjectAsync(_awsCredentials, oldS3File);

                productoExistente.NombreProducto = productoToUpdate.NombreProducto;
                productoExistente.DescripcionProducto = productoToUpdate.DescripcionProducto;
                productoExistente.EstaActivo = productoToUpdate.EstaActivo;
                
                productoExistente.ImagenProducto = newS3Object.Name;

                await _productoRepository.UpdateAsync(productoExistente);
                
                return productoExistente;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<Producto> UpdateAsync(Producto productoToUpdate)
        {
            if (productoToUpdate == null)
                return null;

            var existing = await _productoRepository.GetByIdAsync(productoToUpdate.IdProducto);
            if (existing == null)
                return null;

            await _productoRepository.UpdateAsync(productoToUpdate);
            return productoToUpdate;
        }

        public async Task<Producto> CreateAsync(Producto newProducto)
        {
            if (newProducto == null)
                return null;

            await _productoRepository.AddAsync(newProducto);

            return newProducto;
        }
        
        public async Task<Producto> CreateWithImageAsync(ProductoCreateRequest request, IFormFile imageFile)
        {
            if (request == null)
                return null;
            
            if(imageFile is null || imageFile.Length == 0)
            {
                throw new Exception("No valid image provided.");
            }
            
            try
            {
                using var stream = new MemoryStream();
                await imageFile.CopyToAsync(stream);
                var s3Object = new S3Object
                {
                    InputStream = stream,
                    Name = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName),
                    BucketName = _bucketName
                };

                var response = await _s3StorageService.UploadObjectAsync(s3Object, _awsCredentials);
                if (response.StatusCode != 200)
                {
                    throw new Exception("Error uploading image to S3: " + response.Message);
                }
                
                var nuevoProducto = new Producto();
                nuevoProducto.NombreProducto = request.NombreProducto;
                nuevoProducto.DescripcionProducto = request.DescripcionProducto;
                nuevoProducto.EstaActivo = request.EstaActivo;
                
                nuevoProducto.ImagenProducto = s3Object.Name;

                await _productoRepository.AddAsync(nuevoProducto);
                
                return nuevoProducto;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public async Task<Producto> CreateIfNotExistsAsync(Producto newProducto)
        {
            if (newProducto == null)
                return null;

            if (newProducto.IdProducto > 0 && await ExistsByIdAsync(newProducto.IdProducto))
                return await GetByIdAsync(newProducto.IdProducto);

            if (!string.IsNullOrEmpty(newProducto.NombreProducto) && await ExistsByNameAsync(newProducto.NombreProducto))
                return await GetByNameAsync(newProducto.NombreProducto);

            return await CreateAsync(newProducto);
        }

        public async Task<bool> ExistsByIdAsync(int idProducto)
        {
            return await _productoRepository.ExistsByPredicate(p => p.IdProducto.Equals(idProducto));
        }

        public async Task<bool> ExistsByNameAsync(string ProductoName)
        {
            return await _productoRepository.ExistsByPredicate(p => EF.Functions.Like(p.NombreProducto, ProductoName));
        }

        public async Task<Producto> GetByIdAsync(int idProducto)
        {
            return await _productoRepository.GetByIdAsync(idProducto);
        }

        public async Task<Producto> GetByNameAsync(string ProductoName)
        {
            return await _productoRepository.GetByPredicate(p => EF.Functions.Like(p.NombreProducto, ProductoName));
        }

        public async Task<bool> DeleteById(int idProducto)
        {
            try
            {
                await _productoRepository.DeleteAsync(idProducto);
                return true;
            }
            catch (Exception)
            {
                // TODO: SAVELOG
                return false;
            }
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            try
            {
                await _productoRepository.SoftDeleteAsync(id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<Producto>> GetAllByPredicateAsync(Expression<Func<Producto, bool>> predicate)
        {
            return (IEnumerable<Producto>)await _productoRepository.GetCollectionByPredicate(predicate);
        }
    }
}
