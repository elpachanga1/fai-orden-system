using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validations.ConcretImplementation;
using Validations.Interface;

namespace ValidationFactory
{
    public class ConcreteChainCreator : ICreatorFactory
    {
        public IHandler CreateChain()
        {
            IHandler authenticationHandler = new AuthenticationHandler();
            IHandler dataSanitizationHandler = new DataSanitizationHandler();
            IHandler bruteForceHandler = new BruteForceHandler();
            IHandler responseSpeedHandler = new ResponseSpeedHandler();
            IHandler finishHandler = new FinishValidationHandler();

            authenticationHandler.SetNext(dataSanitizationHandler)
                .SetNext(bruteForceHandler)
                .SetNext(responseSpeedHandler)
                .SetNext(finishHandler);

            return authenticationHandler;
        }
    }
}
