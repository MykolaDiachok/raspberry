namespace raspberry;

public class RgbColor(int r, int g, int b)
{
    private int R { get; set; } = r;
    private int G { get; set; } = g;
    private int B { get; set; } = b;
    
    public uint ToUint()
    {
        return (uint)(R << 16 | G << 8 | B);
    }
}