using System.Collections.Generic;

namespace FluentNest
{
    public interface IFnCallback
    {
        public void Receive(FnMessage msg);
    }
}