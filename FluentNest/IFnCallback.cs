using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FluentNest
{
    public interface IFnCallback
    {
        public void Receive(string tag, List<FnMessageEntry> entries);
    }
}