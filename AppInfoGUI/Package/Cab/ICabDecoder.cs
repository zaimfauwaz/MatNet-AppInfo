namespace AppInfoGUI.Package.Cab;

// Unpacks the blocks of one folder of a cabinet, one after the other. The decoder may keep state between blocks.
internal interface ICabDecoder
{
    // Adds the `size` unpacked bytes of one block to `output`
    void Decode(byte[] block, int size, GrowingBuffer output);
}

// Blocks that are not packed
internal class StoredDecoder : ICabDecoder
{
    public void Decode(byte[] block, int size, GrowingBuffer output) => output.Append(block, 0, size);
}
