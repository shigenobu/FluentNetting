using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FluentNest
{
    public interface IFnCallback
    {
        public void Receive(FnMessage msg);

        public virtual void Error(Exception e, FnMessage msg)
        {
        }
    }
}