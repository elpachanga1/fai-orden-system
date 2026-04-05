using Validations.ConcretImplementation;
using Validations.Interface;

namespace Validations
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
