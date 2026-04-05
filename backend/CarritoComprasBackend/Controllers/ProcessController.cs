using CarritoComprasBackend.Controllers;
using Microsoft.AspNetCore.Mvc;
using Services.Domain.Services;
using ValidationFactory;
using Validations;
using Validations.Interface;

namespace ShoppingCartBackEnd.Controllers
{
    public class ProcessController : Controller
    {
        private readonly ILogger<StoreController> _logger;
        private readonly StoreService _storeService;
        private readonly ICreatorFactory _creatorFactory;

        public ProcessController(ILogger<StoreController> logger, ICreatorFactory creatorFactory)
        {
            this._logger = logger;
            this._creatorFactory = creatorFactory;
        }

        [HttpGet("/Process/GetProcess", Name = "GetProcess")]
        public async Task<IActionResult> GetProcess(string username)
        {
            var request = InMemoryRequestRepository.Instance.GetRequest(username);

            return Ok(request);
        }

        [HttpPost("/Process/RunValidation", Name = "RunValidation")]
        public async Task<IActionResult> RunValidation(string username)
        {
            
            IHandler handlerChain = _creatorFactory.CreateChain();

            var request = InMemoryRequestRepository.Instance.GetRequest(username);
            var validationMapEntry = request.ValidationMaps.FirstOrDefault(x => x.ValidationName == request.RecoveryNextHandlerName);
            if (validationMapEntry != null)
            {
                validationMapEntry.CreationDate = DateTime.Now;
                validationMapEntry.State = true;                                
            }
            handlerChain.Handle(request);           

            return Ok();
        }
             

        [HttpDelete("/Process/DeleteProcess", Name = "DeleteProcess")]
        public async Task<IActionResult> DeleteProcess(string username)
        {
            var request = InMemoryRequestRepository.Instance.DeleteRequest(username);

            return Ok(request);
        }
    }
}
