namespace raspberry;

public class Ws281XController
{
    private readonly ws2811_t _ws2811;
    private ws2811_channel_t _channel;
    private readonly int _ledCount;
    private int _pin;  
    public int Brightness { get; set; }

    public Ws281XController(int pin, int ledCount)
    {
        _pin = pin;
        _ledCount = ledCount;
        _ws2811 = new ws2811_t();
        Brightness = 255;  
    }

    
    public bool Initialize()
    {
        _channel = rpi_ws281x.ws2811_channel_get(_ws2811, 0); 
        rpi_ws281x.ws2811_led_set(_channel, 0, 0x000000);  
        
        return rpi_ws281x.ws2811_init(_ws2811) == ws2811_return_t.WS2811_SUCCESS;
    }

    
    public void SetColor(uint color)
    {
        for (int i = 0; i < _ledCount; i++)
        {
            rpi_ws281x.ws2811_led_set(_channel, i, color);
        }
    }

    public void SetSomeColor(int r, int g, int b, int[] ids)
    {
        uint color = (uint)(r << 16 | g << 8 | b); 

        foreach (var id in ids)
        {
            rpi_ws281x.ws2811_led_set(_channel, id, color);  
        }
    }
    
    public void SetPixelColor(int id, int r, int g, int b)
    {
        uint color = (uint)(r << 16 | g << 8 | b); 
        rpi_ws281x.ws2811_led_set(_channel, id, color);  
    }
    
    public void SetPixelColor(int id, RgbColor color)
    {
        uint colorUint = color.ToUint(); 
        rpi_ws281x.ws2811_led_set(_channel, id, colorUint);  
    }
    
    public void SetBrightness(int brightness)
    {
        Brightness = brightness;
        uint color = (uint)(brightness << 16 | brightness << 8 | brightness);
        SetColor(color);
    }
    
    public void Show()
    {
        rpi_ws281x.ws2811_render(_ws2811);  
    }
    
    public void Shutdown()
    {
        rpi_ws281x.ws2811_fini(_ws2811);
    }
}