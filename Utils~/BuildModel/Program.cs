using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Urho3DNet;
using Console = System.Console;

internal class Program
{
    private static void Main(string[] args)
    {
        var cmd = new RootCommand();
        cmd.AddOption(new Option<DirectoryInfo>("--output"));
        cmd.Handler = CommandHandler.Create(GenerateCode);
        cmd.Invoke(args);
    }

    public static void GenerateCode(DirectoryInfo output)
    {
        using (SharedPtr<Context> contextPtr = new Context())
        {
            SharedPtr<Application> application = new MockApplication(contextPtr, output);
            application.Ptr.Run();
        }
    }

}