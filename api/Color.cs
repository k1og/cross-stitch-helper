using SkiaSharp;

namespace api;

public class Color
{
    public byte Red { get; }
    public byte Green { get; }
    public byte Blue { get; }

    public string Name { get { return $"{this.Red.ToString()},{this.Green.ToString()},{this.Blue.ToString()}"; }}

    public Color(byte Red, byte Green, byte Blue) {
        this.Red = Red;
        this.Green = Green;
        this.Blue = Blue;
    }

    public Color(int Red, int Green, int Blue) {
        this.Red = Convert.ToByte(Red);
        this.Green = Convert.ToByte(Green);
        this.Blue = Convert.ToByte(Blue);
    }

    public Color(SKColor skColor) {
        this.Red = skColor.Red;
        this.Green = skColor.Green;
        this.Blue = skColor.Blue;
    }

    public Color(string name) {
        var rbg = name.Split(',');
        
        this.Red = Convert.ToByte(Int32.Parse(rbg[0]));
        this.Green = Convert.ToByte(Int32.Parse(rbg[1]));
        this.Blue = Convert.ToByte(Int32.Parse(rbg[2]));
    }

    public bool Equals(Color color) {
        return color.Red == this.Red && color.Green == this.Green && color.Blue == this.Blue;
    }

    public SKColor ToSKColor() {
        return new SKColor(Red, Green, Blue);
    }

    public Color GetClosesColor(IEnumerable<Color> colors) {
        var closestColor = colors.First();
        var closestVal = int.MaxValue; 

        foreach (var color in colors)
        {
            var val = Math.Abs(color.Red - this.Red) + Math.Abs(color.Green - this.Green) + Math.Abs(color.Blue - this.Blue);
            if (val < closestVal) {
                closestVal = val;
                closestColor = color;
            }
        }
        return closestColor;
    }
}
