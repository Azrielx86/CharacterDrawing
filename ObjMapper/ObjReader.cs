using System.Text.RegularExpressions;
using ObjMapper.Exceptions;
using ObjMapper.Models;

namespace ObjMapper;

public partial class ObjReader
{
    private string Path { get; }
    private readonly Dictionary<string, float[]> _materials = new();
    private readonly List<ObjModel> _models = new();
    private int _vtxCount;
    private int _fcCount;

    public bool ExportMaterials { get; init; } = true;
    public string OutputDir { get; set; } = ".";
    public string Separator { get; set; } = " ";

    public ObjReader(string path)
    {
        Path = path;
    }

    public void ParseObj()
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (ExportMaterials)
                ParseMtl();
            using var fs = new FileStream(Path, FileMode.Open, FileAccess.Read);
            using var sr = new StreamReader(fs);
            ObjModel? model = null;

            Console.WriteLine("==========[ Searching for objects ]==========");
            while (sr.ReadLine() is { } currentLine)
            {
                if (currentLine.StartsWith("o "))
                {
                    model = new ObjModel
                    {
                        Name = currentLine.Replace("o ", string.Empty).Trim(),
                        Separator = Separator
                    };
                    Console.WriteLine($">====== Object found: {model.Name}");
                    _models.Add(model);
                }

                if (model is null) continue;
                if (currentLine.StartsWith("v "))
                {
                    var numbers = VertexRegex().Matches(currentLine).Select(c => c.Value);
                    model.VertexList.AddRange(numbers.Select(float.Parse));
                    _vtxCount++;
                }
                else if (currentLine.StartsWith("f "))
                {
                    var faces = NumberRegex().Matches(currentLine).Select(c => int.Parse(c.Value) - 1);
                    model.Faces.AddRange(faces);
                    _fcCount++;
                }
                else if (currentLine.StartsWith("usemtl"))
                {
                    if (!ExportMaterials) continue;
                    var materialString = WordRegex().Matches(currentLine).LastOrDefault()!.Value;
                    Console.Write($"\t==> Material required: {materialString} ");
                    var mat = _materials[materialString];
                    Console.WriteLine($"({mat[0]}, {mat[1]}, {mat[2]})");
                    model.Color = _materials[materialString];
                }
            }

            var modelVertex = _models.Select(m => ExportMaterials ? m.MapVertexWithColor() : m.MapVertex());
            var modelFaces = _models.Select(m => m.MapFaces());

            // using StreamWriter swVertex = new("vertex.txt");
            using StreamWriter swVertex = new(System.IO.Path.Combine(OutputDir, "vertex.txt"));
            modelVertex.ToList().ForEach(v => swVertex.Write(v));
            using StreamWriter swFaces = new(System.IO.Path.Combine(OutputDir, "faces.txt"));
            modelFaces.ToList().ForEach(f => swFaces.Write(f));

            watch.Stop();
            Console.WriteLine("=======[ Export info ]=======");
            Console.WriteLine($"\t{_vtxCount} Vertex.");
            Console.WriteLine($"\t{_fcCount} Faces.");
            Console.WriteLine($"\t{_models.Count} Objects.");
            Console.WriteLine($"\t{_materials.Count} Materials.");
            Console.WriteLine($"Time taken: {watch.Elapsed}.");
        }
        catch (Exception e)
        {
            watch.Stop();
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
            Console.WriteLine("==========[ Searching for materials ]==========");
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
                Console.Write($">====== Material found: {mat.Key} ");
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