namespace ObjMapper.Exceptions;

public class MtlFileNotFound : Exception
{
    public override string Message { get; } = "Mtl File not found!";
}