// See https://aka.ms/new-console-template for more information

using Mono.Options;
using ObjMapper;

string? fileName = null;
var outputDir = ".";
var exportMaterials = false;

var options = new OptionSet()
{
    { "i|input=", "File to parse.", i => { fileName = i; } },
    { "m|materials", "Export materials.", _ => exportMaterials = true },
    { "o|output=", "Output directory (Default is the same directory).", o => { outputDir = o; } },
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

var or = new ObjReader(fileName)
{
    ExportMaterials = exportMaterials
};

or.ParseObj();