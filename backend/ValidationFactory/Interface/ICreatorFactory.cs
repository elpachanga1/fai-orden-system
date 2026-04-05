using Validations.Interface;

namespace ValidationFactory
{
    public interface ICreatorFactory
    {
        IHandler CreateChain();
    }
}
