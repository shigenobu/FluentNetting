namespace FluentNest
{
    internal static class FnExtension
    {
        internal static byte[] FxConcat(this byte[] self, byte[] additional)
        {
            var output = new byte[self.Length + additional.Length];
            for (var i = 0; i < self.Length; i++)
                output[i] = self[i];
            for (var j = 0; j < additional.Length; j++)
                output[self.Length + j] = additional[j];
            return output;    
        }
    }
}