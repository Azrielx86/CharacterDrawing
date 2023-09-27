// See https://aka.ms/new-console-template for more information

using ObjMapper;

Console.WriteLine("Hello, World!");

var or = new ObjReader("./Files/2B_Model.obj");
or.ParseObj();