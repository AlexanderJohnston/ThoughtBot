namespace REBL.Utilities
{
    // Generic version of Unpack above
    public static class DynamicBuffer<T>
    {
        public static T Unpack(object buffer)
        {
            var bufferExists = Buffers.Is<T>(buffer);
            T unpack;
            if (!bufferExists)
            {
                unpack = default;
            }
            else
            {
                unpack = (T)buffer;
            }
            return unpack;
        }
    }





}