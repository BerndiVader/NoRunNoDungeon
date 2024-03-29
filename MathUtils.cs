using Godot;
using System;

public static class MathUtils 
{

    private static Random random=new Random();

    public static double randomRange(double min,double max)
    { 
        return random.NextDouble()*(max-min)+min;
    }

    public static int randomRangeInt(int min,int max)
    {
        return random.Next(min,max);
    }    

    public static float minMax(float min, float max, float delta)
    {
        return Math.Max(Math.Min(delta,max),min);
    }

}
