// See https://aka.ms/new-console-template for more information

using Mono.Options;
using ObjMapper;

string? fileName = null;
var outputDir = ".";
var separator = " ";
var exportMaterials = false;

var options = new OptionSet()
{
    { "i|input=", "File to parse.", i => { fileName = i; } },
    { "m|materials", "Export materials.", _ => exportMaterials = true },
    { "o|output=", "Output directory (Default is the same directory).", o => { outputDir = o; } },
    { "s|separator=", "Separator of the data in the output file", s => { separator = s; } }
};

options.Add("h|help", "Show help", _ =>
{
    options.WriteOptionDescriptions(Console.Out);
    Environment.Exit(0);
});

options.Parse(args);

if (fileName is null)
{
    Console.WriteLine("Missing argument: i|input");
    options.WriteOptionDescriptions(Console.Out);
    Environment.Exit(1);
}

if (!Directory.Exists(outputDir))
    Directory.CreateDirectory(outputDir);

var or = new ObjReader(fileName)
{
    ExportMaterials = exportMaterials,
    OutputDir = outputDir,
    Separator = separator
};

or.ParseObj();