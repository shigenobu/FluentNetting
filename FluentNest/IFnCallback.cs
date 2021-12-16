using System.Collections.Generic;

namespace FluentNest
{
    public interface IFnCallback
    {
        public void Receive(FnMessageForward msg);
    }
}