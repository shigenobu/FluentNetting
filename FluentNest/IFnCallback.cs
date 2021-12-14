using System.Collections.Generic;

namespace FluentNest
{
    public interface IFnCallback
    {
        public void Receive(string tag, List<FnEntry> entries);
    }
}