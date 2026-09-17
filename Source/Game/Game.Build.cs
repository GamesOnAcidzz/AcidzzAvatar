using Flax.Build;
using Flax.Build.NativeCpp;
using System.IO;
public class Game : GameModule
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();

        // C#-only scripting
        BuildNativeCode = false;
    }

    /// <inheritdoc />
    public override void Setup(BuildOptions options)
    {
        base.Setup(options);

        options.ScriptingAPI.IgnoreMissingDocumentationWarnings = true;

        string portAudio = Path.Combine(
            FolderPath,
            "..",
            "..",
            "Content",
            "ThirdParty",
            "PortAudio"
        );

        // C# wrapper
        options.ScriptingAPI.FileReferences.Add(
            Path.Combine(
                portAudio,
                "PortAudioSharp2.1.0.6",
                "lib",
                "net8.0",
                "PortAudioSharp.dll"
            )
        );

        // Windows x64
        if (options.Platform.Target == TargetPlatform.Windows)
        {
            options.DependencyFiles.Add(
                Path.Combine(
                    portAudio,
                    "org.k2fsa.portaudio.runtime.win-x64.1.0.6",
                    "runtimes",
                    "win-x64",
                    "native",
                    "portaudio.dll"
                )
            );
        }

        // Linux x64
        if (options.Platform.Target == TargetPlatform.Linux)
        {
            options.DependencyFiles.Add(
                Path.Combine(
                    portAudio,
                    "org.k2fsa.portaudio.runtime.linux-x64.1.0.6",
                    "runtimes",
                    "linux-x64",
                    "native",
                    "libportaudio.so"
                )
            );
        }
        // Here you can modify the build options for your game module
        // To reference another module use: options.PublicDependencies.Add("Audio");
        // To add C++ define use: options.PublicDefinitions.Add("COMPILE_WITH_FLAX");
        // To learn more see scripting documentation.
    }
}
