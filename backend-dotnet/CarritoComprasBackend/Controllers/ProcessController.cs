using CarritoComprasBackend.Controllers;
using Microsoft.AspNetCore.Mvc;
using ValidationFactory;
using Validations;
using Validations.Interface;

namespace ShoppingCartBackEnd.Controllers
{
    public class ProcessController : Controller
    {
        private readonly ICreatorFactory _creatorFactory;

        public ProcessController(ICreatorFactory creatorFactory)
        {
            this._creatorFactory = creatorFactory;
        }

        [HttpGet("/Process/GetProcess", Name = "GetProcess")]
        public IActionResult GetProcess(string username)
        {
            var request = InMemoryRequestRepository.Instance.GetRequest(username);

            return Ok(request);
        }

        [HttpPost("/Process/RunValidation", Name = "RunValidation")]
        public IActionResult RunValidation(string username)
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
        public IActionResult DeleteProcess(string username)
        {
            var request = InMemoryRequestRepository.Instance.DeleteRequest(username);

            return Ok(request);
        }
    }
}
