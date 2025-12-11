using Godot;
using System;

public static class MathUtils 
{

    private static Random random=new Random();

    public static double RandomRange(double min,double max)
    { 
        return random.NextDouble()*(max-min)+min;
    }

    public static int RandomRangeInt(int min,int max)
    {
        return random.Next(min,max);
    }

    public static float MinMax(float min, float max, float delta)
    {
        return Math.Max(Math.Min(delta, max), min);
    }

    public static int RandSign()
    {
        return random.Next() % 2 == 0 ? 1 : -1;
    }
    
    public static bool RandBool()
    {
        return random.Next() % 2 == 0;
    }

}
