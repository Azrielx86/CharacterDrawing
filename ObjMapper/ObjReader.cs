using System.Text.RegularExpressions;
using ObjMapper.Exceptions;
using ObjMapper.Models;

namespace ObjMapper;

public partial class ObjReader
{
    private string Path { get; }
    private readonly Dictionary<string, float[]> _materials = new();
    private readonly List<ObjModel> _models = new();

    public ObjReader(string path)
    {
        Path = path;
    }

    public void ParseObj()
    {
        try
        {
            ParseMtl();
            using var fs = new FileStream(Path, FileMode.Open, FileAccess.Read);
            using var sr = new StreamReader(fs);
            ObjModel? model = null;

            while (sr.ReadLine() is { } currentLine)
            {
                if (currentLine.StartsWith("o "))
                {
                    model = new ObjModel
                    {
                        Name = currentLine.Replace("o ", string.Empty).Trim()
                    };
                    Console.WriteLine($">==== New Object found: {model.Name}");
                    _models.Add(model);
                }

                if (model is null) continue;
                if (currentLine.StartsWith("v "))
                {
                    var numbers = VertexRegex().Matches(currentLine).Select(c => c.Value);
                    model.VertexList.AddRange(numbers.Select(float.Parse));
                }
                else if (currentLine.StartsWith("f "))
                {
                    var faces = NumberRegex().Matches(currentLine).Select(c => int.Parse(c.Value) - 1);
                    model.Faces.AddRange(faces);
                }
                else if (currentLine.StartsWith("usemtl"))
                {
                    var materialString = WordRegex().Matches(currentLine).LastOrDefault()!.Value;
                    Console.Write($">==== Material required: {materialString} ");
                    var mat = _materials[materialString];
                    Console.WriteLine($"({mat[0]}, {mat[1]}, {mat[2]})");
                    model.Color = _materials[materialString];
                }
            }

            var modelVertex = _models.Select(m => m.MapVertex());
            var modelFaces = _models.Select(m => m.MapFaces());

            using StreamWriter swVertex = new("vertex.txt");
            modelVertex.ToList().ForEach(v => swVertex.Write(v));
            using StreamWriter swFaces = new("faces.txt");
            modelFaces.ToList().ForEach(f => swFaces.Write(f));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void ParseMtl()
    {
        var mtlPath = Path;
        if (Path.EndsWith(".obj"))
            mtlPath = Path.Replace(".obj", ".mtl");

        if (!File.Exists(mtlPath))
            throw new MtlFileNotFound();

        try
        {
            using var fs = new FileStream(mtlPath, FileMode.Open, FileAccess.Read);
            using var sr = new StreamReader(fs);

            while (sr.ReadLine() is { } currentLine)
            {
                if (!currentLine.StartsWith("newmtl")) continue;
                var mtlName = WordRegex().Matches(currentLine).LastOrDefault()!.Value;
                while (sr.ReadLine() is { } colorLine)
                {
                    if (!colorLine.StartsWith("Kd")) continue;
                    var colors = ColorRegex().Matches(colorLine).Select(c => c.Value).ToList();
                    var cFl = colors.Select(float.Parse).ToArray();
                    _materials[mtlName] = cFl;
                    break;
                }
            }

            foreach (var mat in _materials)
            {
                Console.Write($"Material found: {mat.Key} ");
                Console.WriteLine($"({mat.Value[0]}, {mat.Value[1]}, {mat.Value[2]})");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [GeneratedRegex(@"-?\d+\.\d+")]
    private static partial Regex VertexRegex();

    [GeneratedRegex(@"\w+")]
    private static partial Regex WordRegex();

    [GeneratedRegex(@"\d+\.\d+")]
    private static partial Regex ColorRegex();

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumberRegex();
}