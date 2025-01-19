namespace raspberry;

static class Program
{
    static void Main(string[] args)
    {
        var robotLight = new RobotLight();
        robotLight.Run();

        
        robotLight.Breath(70, 70, 255);
        Thread.Sleep(15000);  
        robotLight.Pause();
        robotLight.FrontLight("off");
        
        Thread.Sleep(10000);
        robotLight.Police();
        Thread.Sleep(20000);
        robotLight.FrontLight("off");
    }
}