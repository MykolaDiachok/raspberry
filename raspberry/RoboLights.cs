using System.Device.Gpio;

namespace raspberry;

public class RobotLight
{
    private const int LedCount = 16;    
    private const int LedPin = 12;      
    private const int LedBrightness = 255;
    private const int BreathSteps = 10;
    private string _lightMode = "none";
    private int _colorBreathR = 0, _colorBreathG = 0, _colorBreathB = 0;

    private readonly GpioController _gpioController;
    private readonly Ws281XController _ledStrip;
    private Thread _lightThread;
    private AutoResetEvent _flag;

    public RobotLight()
    {
        _gpioController = new GpioController();
        _flag = new AutoResetEvent(false);

        
        _gpioController.OpenPin(5, PinMode.Output);
        _gpioController.OpenPin(6, PinMode.Output);
        _gpioController.OpenPin(13, PinMode.Output);

        
        _ledStrip = new Ws281XController(LedPin, LedCount);
        _ledStrip.Brightness = LedBrightness;
    }

    
    public void SetColor(int r, int g, int b)
    {
        for (int i = 0; i < LedCount; i++)
        {
            _ledStrip.SetPixelColor(i, new RgbColor(r, g, b));
        }
        _ledStrip.Show();
    }

    
    public void SetSomeColor(int r, int g, int b, int[] ids)
    {
        foreach (var id in ids)
        {
            _ledStrip.SetPixelColor(id, new RgbColor(r, g, b));
        }
        _ledStrip.Show();
    }

    
    public void Pause()
    {
        _lightMode = "none";
        SetColor(0, 0, 0);
        _flag.Reset();
    }

    
    public void Resume()
    {
        _flag.Set();
    }

    
    public void Police()
    {
        _lightMode = "police";
        Resume();
    }

    
    private void PoliceProcessing()
    {
        while (_lightMode == "police")
        {
            SetSomeColor(0, 0, 255, new[] { 0, 1, 2 });
            Thread.Sleep(50);
            SetSomeColor(0, 0, 0, new[] { 0, 1, 2 });
            Thread.Sleep(50);
            SetSomeColor(255, 0, 0, new[] { 0, 1, 2 });
            Thread.Sleep(50);
            SetSomeColor(0, 0, 0, new[] { 0, 1, 2 });
            Thread.Sleep(50);
        }
    }

    
    public void Breath(int r, int g, int b)
    {
        _lightMode = "breath";
        _colorBreathR = r;
        _colorBreathG = g;
        _colorBreathB = b;
        Resume();
    }

    
    private void BreathProcessing()
    {
        while (_lightMode == "breath")
        {
            for (int i = 0; i < BreathSteps; i++)
            {
                SetColor((_colorBreathR * i) / BreathSteps, (_colorBreathG * i) / BreathSteps, (_colorBreathB * i) / BreathSteps);
                Thread.Sleep(30);
            }

            for (int i = 0; i < BreathSteps; i++)
            {
                SetColor(_colorBreathR - (_colorBreathR * i) / BreathSteps, _colorBreathG - (_colorBreathG * i) / BreathSteps, _colorBreathB - (_colorBreathB * i) / BreathSteps);
                Thread.Sleep(30);
            }
        }
    }

    
    public void FrontLight(string switchState)
    {
        if (switchState == "on")
        {
            _gpioController.Write(6, PinValue.High);
            _gpioController.Write(13, PinValue.High);
        }
        else if (switchState == "off")
        {
            _gpioController.Write(5, PinValue.Low);
            _gpioController.Write(13, PinValue.Low);
        }
    }

    
    private void LightChange()
    {
        if (_lightMode == "none")
        {
            Pause();
        }
        else if (_lightMode == "police")
        {
            PoliceProcessing();
        }
        else if (_lightMode == "breath")
        {
            BreathProcessing();
        }
    }

    
    public void Run()
    {
        while (true)
        {
            _flag.WaitOne();
            LightChange();
        }
    }
}