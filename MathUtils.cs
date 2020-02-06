using Godot;
using System;

public static class MathUtils {

    static Random random=new Random();

    public static double randomRange(double min,double max){ 
        return random.NextDouble()*(max-min)+min;
    }    

}
