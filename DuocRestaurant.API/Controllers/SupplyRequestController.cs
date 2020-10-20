﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.Services;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DuocRestaurant.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplyRequestController : ControllerBase
    {
        private ISupplyRequestService supplyRequestService { get; set; }
        private IProductService productService { get; set; }
        private RestaurantDatabaseSettings dbSettings { get; set; }

        public SupplyRequestController(ISupplyRequestService supplyRequestService,
            IProductService productService,
            IOptions<RestaurantDatabaseSettings> databaseContext)
        {
            this.supplyRequestService = supplyRequestService;
            this.productService = productService;
            this.dbSettings = databaseContext.Value;
        }

        [HttpGet]
        [ActionName("GetAll")]
        [Route("[action]")]
        public IActionResult Get()
        {
            IActionResult result;

            try
            {
                var supplyRequests = this.supplyRequestService.Get(this.dbSettings);
                var products = this.productService.Get(this.dbSettings).Where(x => x.Active).ToList();
                foreach (var supplyRequest in supplyRequests)
                {
                    if (supplyRequest.SupplyRequestDetails != null)
                    {
                        foreach (var supplyRequestDetail in supplyRequest.SupplyRequestDetails)
                        {
                            supplyRequestDetail.Product = products.FirstOrDefault(x => x.Id == supplyRequestDetail.ProductId);
                        }
                    }
                }

                result = Ok(supplyRequests.MapAll(this.dbSettings, true));
            }
            catch (Exception ex)
            {
                result = BadRequest(ex.Message);
            }

            return result;
        }

        [HttpGet]
        [ActionName("GetById")]
        [Route("[action]/{id:int}")]
        public IActionResult Get([FromRoute(Name = "id")] int recipeId)
        {
            IActionResult result;

            try
            {
                var supplyRequest = this.supplyRequestService.Get(this.dbSettings, recipeId);

                if (supplyRequest.SupplyRequestDetails != null)
                {
                    var products = this.productService.Get(this.dbSettings).Where(x => x.Active).ToList();
                    foreach (var supplyRequestDetail in supplyRequest.SupplyRequestDetails)
                    {
                        supplyRequestDetail.Product = products.FirstOrDefault(x => x.Id == supplyRequestDetail.ProductId);
                    }
                }

                result = Ok(supplyRequest.Map(this.dbSettings, true));
            }
            catch (Exception ex)
            {
                result = BadRequest(ex.Message);
            }

            return result;
        }

        [HttpPost, DisableRequestSizeLimit]
        public IActionResult Post([FromBody] SupplyRequest supplyRequest)
        {
            IActionResult result;

            try
            {
                var supplyRequests = this.supplyRequestService.Get(this.dbSettings);
                if (supplyRequests.Any(x => x.CreationDate.Date.Equals(supplyRequest.CreationDate.Date))) // check if this is necessary
                    throw new Exception($"Ya existe una receta para la fecha: {supplyRequest.CreationDate:dd-MM-YYYY}");

                var created = this.supplyRequestService.Add(this.dbSettings, supplyRequest);

                if (created.SupplyRequestDetails != null && created.SupplyRequestDetails.Any())
                {
                    var products = this.productService.Get(this.dbSettings).Where(x => x.Active).ToList();
                    foreach (var supplyRequestDetail in created.SupplyRequestDetails)
                    {
                        supplyRequestDetail.Product = products.FirstOrDefault(x => x.Id == supplyRequestDetail.ProductId);
                    }
                }

                result = Ok(created.Map(this.dbSettings, true));
            }
            catch (Exception ex)
            {
                result = BadRequest(ex.Message);
            }

            return result;
        }

        [HttpPut("{id}")]
        public IActionResult Put([FromRoute(Name = "id")] int recipeId, [FromBody] SupplyRequest supplyRequest)
        {
            IActionResult result;

            try
            {
                var supplyRequests = this.supplyRequestService.Get(this.dbSettings);
                if (supplyRequests.Any(x => x.CreationDate.Date.Equals(supplyRequest.CreationDate.Date))) // check if this is necessary
                    throw new Exception($"Ya existe una receta para la fecha: {supplyRequest.CreationDate:dd-MM-YYYY}");

                var edited = this.supplyRequestService.Edit(this.dbSettings, recipeId, supplyRequest);

                edited.SupplyRequestDetails = this.supplyRequestService.Get(this.dbSettings, edited).ToList();

                if (edited.SupplyRequestDetails != null && edited.SupplyRequestDetails.Any())
                {
                    var products = this.productService.Get(this.dbSettings).Where(x => x.Active).ToList();
                    foreach (var supplyRequestDetail in edited.SupplyRequestDetails)
                    {
                        supplyRequestDetail.Product = products.FirstOrDefault(x => x.Id == supplyRequestDetail.ProductId);
                    }
                }

                result = Ok(edited.Map(this.dbSettings, true));
            }
            catch (Exception ex)
            {
                result = BadRequest(ex.Message);
            }

            return result;
        }

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute(Name = "id")] int recipeId)
        {
            IActionResult result;

            try
            {
                result = Ok(this.supplyRequestService.Delete(this.dbSettings, recipeId));
            }
            catch (Exception ex)
            {
                result = BadRequest(ex.Message);
            }

            return result;
        }
    }
}