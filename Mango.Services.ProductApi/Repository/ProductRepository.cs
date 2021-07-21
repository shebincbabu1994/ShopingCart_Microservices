using AutoMapper;
using Mango.Services.ProductApi.DbContexts;
using Mango.Services.ProductApi.Models;
using Mango.Services.ProductApi.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mango.Services.ProductApi.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _db;
        private IMapper _mapper;
        public ProductRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<ProductDto> CreateUpdateProduct(ProductDto model)
        {
            var createOrUpdate = _mapper.Map<ProductDto, Product>(model);
            if (createOrUpdate.ProductId > 0)
            {
                _db.Products.Update(createOrUpdate);
            }
            else
            {
                _db.Products.Add(createOrUpdate);
            }
            await _db.SaveChangesAsync();

            return _mapper.Map<Product, ProductDto>(createOrUpdate);


        }

        public async Task<bool> DeleteProduct(int productId)
        {
            try
            {
                var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
                if (product == null)
                {
                    return false;
                }
                _db.Products.Remove(product);
                return _db.SaveChanges() > 0;

            }
            catch
            {
                return false;
            }

        }


        public async Task<ProductDto> GetProductById(int id)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == id);

            return _mapper.Map<ProductDto>(product);

        }

        public async Task<IEnumerable<ProductDto>> GetProducts()
        {
            var products = await _db.Products.ToListAsync();

            return _mapper.Map<List<ProductDto>>(products);

        }
    }
}
