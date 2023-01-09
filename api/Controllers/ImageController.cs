using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using SkiaSharp;

namespace api.Controllers;

[ApiController]
[Route("[controller]")]
public class ImageController : ControllerBase
{
    private readonly ILogger<ImageController> _logger;
    private readonly List<Color> randomColors = new List<Color>();

    public ImageController(ILogger<ImageController> logger)
    {
        _logger = logger;
        Random rnd = new Random();
        for (int i = 0; i < 50; i++)
        {
            randomColors.Add(new Color(rnd.Next(255), rnd.Next(255), rnd.Next(255)));
        }
    }

    [EnableCors("AllowAnyOrigin")]
    [HttpPost]
    public async Task<ActionResult> Post(
        [FromForm(Name = "image")]IFormFile file, 
        [FromForm(Name = "chunkSize")]int chunkSize, 
        [FromForm(Name = "colorsAmount")]int? colorsAmount
    ) {
        if (file == null) return BadRequest(new { status= false, massage = "File is null"});
        if (file.Length < 0) return BadRequest(new { status= false, massage = "File length < 0"});
        if (chunkSize <= 0) return BadRequest(new { status= false, massage = "Invalid chunk size"});
        if (colorsAmount != null && colorsAmount <= 0) return BadRequest(new { status= false, massage = "Invalid colors amount"});

        using var fileStream = file.OpenReadStream();
        byte[] imageBytes = new byte[file.Length];
        fileStream.Read(imageBytes, 0, (int)file.Length);

        using SKBitmap sourceBitmap = SKBitmap.Decode(imageBytes);

        var initialHeight = sourceBitmap.Height;
        var initialWidth = sourceBitmap.Width;

        using SKBitmap bitmap = new SKBitmap(initialWidth, initialHeight);

        // var chunkSize = 50;
        var chunkSquare = chunkSize * chunkSize;
        List<Color> colorsInImage = new List<Color>();
        
        var red = 0;
        var green = 0;
        var blue = 0;
        
        // we calculate wtih if image can't be exactly divided by initial chunkSquare (on sides)
        var calculatedChunkSquare = 0;
        for (int i = 0; i < initialHeight; i += chunkSize)
        {
            for (int j = 0; j < initialWidth; j += chunkSize)
            {
                for (int y = i; y < Math.Min(i + chunkSize, initialHeight); y++)
                {
                    for (int x = j; x < Math.Min(j + chunkSize, initialWidth); x++)
                    {
                        var pixel = sourceBitmap.GetPixel(x, y);
                        red += pixel.Red;
                        green += pixel.Green;
                        blue += pixel.Blue;
                        calculatedChunkSquare += 1;
                    }
                }
                
                colorsInImage.Add(new Color(red / calculatedChunkSquare, green / calculatedChunkSquare, blue / calculatedChunkSquare));
                calculatedChunkSquare = 0;
                red = 0;
                green = 0;
                blue = 0;
            }
        }

        IEnumerable<Color>? reducedColors = null;
        if (colorsAmount.HasValue) {
            Dictionary<string, int> colorsDic = new Dictionary<string, int>();
            foreach (var colorInImage in colorsInImage)
            {
                var clrsWoCurrentColor = colorsInImage.ToList();
                var i = colorsInImage.FindIndex((color) => color.Equals(colorInImage));
                clrsWoCurrentColor.RemoveAt(i);
                var closestCrl = colorInImage.GetClosesColor(clrsWoCurrentColor);
                int count;
                if (colorsDic.TryGetValue(closestCrl.Name, out count))
                {
                    colorsDic[closestCrl.Name] = count + 1;
                }
                else
                {
                    colorsDic.Add(closestCrl.Name, 0);
                }
            }

            var sortedColorDic = colorsDic.ToList();

            sortedColorDic.Sort((pair1,pair2) => pair1.Value.CompareTo(pair2.Value) * -1);

            reducedColors = sortedColorDic.Take(colorsAmount.Value).Select(el => new Color(el.Key));
        }

        var index = 0;
        for (int i = 0; i < initialHeight; i += chunkSize)
        {
            for (int j = 0; j < initialWidth; j += chunkSize)
            {
                SKColor color = reducedColors != null ? colorsInImage[index].GetClosesColor(reducedColors).ToSKColor() : colorsInImage[index].ToSKColor();
                for (int y = i; y < Math.Min(i + chunkSize, initialHeight); y++)
                {
                    for (int x = j; x < Math.Min(j + chunkSize, initialWidth); x++)
                    {
                        // todo set pixels istead of loop
                        bitmap.SetPixel(x, y, color);
                    }
                }
                index++;
            }
        }

        using SKImage Image = SKImage.FromBitmap(bitmap);
        using SKData data = Image.Encode();

        return Ok(new { status = true, message = "Good", data = data.ToArray() });
    }
}
