using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validations.Model;

namespace Validations.Interface
{
    public interface IHandler
    {
        string HandlerName { get; }
        IHandler SetNext(IHandler handler);
        void Handle(Request request);
    }
}
