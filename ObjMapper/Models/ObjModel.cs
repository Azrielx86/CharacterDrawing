using System.Text;

namespace ObjMapper.Models;

public class ObjModel
{
    public string? Name { get; set; }
    public List<float> VertexList { get; set; } = new();
    public List<int> Faces { get; set; } = new();
    public float[] Color { get; set; } = { 0.0f, 0.0f, 0.0f };
    public string Separator { get; set; } = " ";

    public string MapVertexWithColor()
    {
        StringBuilder sb = new();
        var color = string.Join(Separator, Color);
        VertexList.Chunk(3)
            .ToList()
            .ForEach(c =>
            {
                sb.Append(string.Join(Separator, c))
                    .Append(Separator)
                    .Append(color)
                    .Append('\n');
            });

        return sb.ToString();
    }
    
    public string MapVertex()
    {
        StringBuilder sb = new();
        VertexList.Chunk(3)
            .ToList()
            .ForEach(c =>
            {
                sb.Append(string.Join(Separator, c))
                    .Append('\n');
            });

        return sb.ToString();
    }

    public string MapFaces()
    {
        StringBuilder sb = new();
        Faces.Chunk(3)
            .ToList()
            .ForEach(f =>
            {
                sb.Append(string.Join(Separator, f))
                    .Append('\n');
            });
        return sb.ToString();
    }
}