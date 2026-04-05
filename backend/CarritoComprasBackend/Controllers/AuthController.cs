using CarritoComprasBackend.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Domain.Services;
using ValidationFactory;
using Validations;
using Validations.Interface;

namespace ShoppingCartBackEnd.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<StoreController> _logger;
        private readonly StoreService _storeService;
        private readonly ICreatorFactory _creatorFactory;

        public AuthController(ILogger<StoreController> logger, StoreService storeFactory, ICreatorFactory creatorFactory)
        {
            this._logger = logger;
            this._storeService = storeFactory;
            this._creatorFactory = creatorFactory;
        }

        [HttpPost("/User/AuthenticateUser", Name = "AuthenticateUser")]
        public async Task<IActionResult> AuthenticateUser(string username, string password)
        {
            try
            {
                var result = await _storeService.AuthenticateUser(username, password);
                if (result != null)
                {
                    IHandler handlerChain = _creatorFactory.CreateChain();
                    
                    var request = InMemoryRequestRepository.Instance.GetRequest(username);
                    var validationMapEntry = request.ValidationMaps.FirstOrDefault(x => x.ValidationName == "Authentication");
                    if (validationMapEntry == null) 
                    {
                        validationMapEntry = new Validations.Model.ValidationMap { ValidationName = "Authentication", State = true, CreationDate = DateTime.Now };  
                        request.ValidationMaps.Add(validationMapEntry);
                    }                    
                    handlerChain.Handle(request);
                    return Ok(result);
                }
                else
                {
                    return StatusCode(404, $"An error occurred: User not found");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
