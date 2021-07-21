using Mango.Services.ProductApi.Models.Dto;
using Mango.Services.ProductApi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mango.Services.ProductApi.Controllers
{
    [Route("api/products")]
    public class ProductAPIController : Controller
    {

        private readonly IProductRepository _productRepository;
        protected ResponseDto _response;
        public ProductAPIController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
            _response = new ResponseDto();
        }

        [HttpGet]
        public async Task<object> Get()
        {
            try
            {
                var productDtos = await _productRepository.GetProducts();
                _response.Result = productDtos;

            }
            catch (Exception eX)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> { eX.ToString() };
            }

            return _response;
        }


        [HttpGet]
        [Route("{id}")]
        public async Task<object> Get(int id)
        {
            try
            {
                var productDto = await _productRepository.GetProductById(id);
                _response.Result = productDto;

            }
            catch (Exception eX)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> { eX.ToString() };
            }

            return _response;
        }


        [HttpPost]
        [Authorize]
        public async Task<object> Post([FromBody] ProductDto productDto)
        {
            try
            {
                var model = await _productRepository.CreateUpdateProduct(productDto);
                _response.Result = productDto;

            }
            catch (Exception eX)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> { eX.ToString() };
            }

            return _response;
        }

        [HttpPut]
        [Authorize]
        public async Task<object> Put([FromBody] ProductDto productDto)
        {
            try
            {
                var model = await _productRepository.CreateUpdateProduct(productDto);
                _response.Result = productDto;

            }
            catch (Exception eX)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> { eX.ToString() };
            }

            return _response;
        }

        [HttpDelete]
        [Authorize(Roles ="Admin")]
        [Route("{id}")]
        public async Task<object> Delete(int id)
        {
            try
            {
                var isSuccess = await _productRepository.DeleteProduct(id);
                _response.Result = isSuccess;

            }
            catch (Exception eX)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string> { eX.ToString() };
            }

            return _response;
        }
    }
}
