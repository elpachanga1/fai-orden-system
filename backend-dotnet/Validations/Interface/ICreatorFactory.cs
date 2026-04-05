using Validations.Interface;

namespace Validations
{
    public interface ICreatorFactory
    {
        IHandler CreateChain();
    }
}
