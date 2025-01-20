namespace raspberry;

public class RgbColor(int r, int g, int b)
{
    public int R { get; set; } = r;
    public int G { get; set; } = g;
    public int B { get; set; } = b;
    
    public uint ToUint()
    {
        return (uint)(R << 16 | G << 8 | B);
    }
}