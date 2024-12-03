using Reloaded.Memory.Sigscan;

namespace PegglePatch;

internal class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("ERROR: No arguments provided! Path to Peggle.exe must be provided. (You can drag Peggle.exe onto the program)");
            Console.ReadLine();
            return;
        }

        string pegglePath = args[0];
        if (!File.Exists(pegglePath) ||
            !Path.GetFileName(pegglePath).StartsWith("Peggle", StringComparison.InvariantCultureIgnoreCase) ||
            !Path.GetExtension(pegglePath).Equals(".exe", StringComparison.InvariantCultureIgnoreCase))
        {
            Console.WriteLine($"ERROR: {pegglePath} is not the Peggle executable.");
            Console.ReadLine();
            return;
        }

        var exeCopy = File.ReadAllBytes(pegglePath);
        var scanner = new Scanner(exeCopy);

        // Patch 1 - Change level music count from 9 to 10
        var offset1Result = scanner.FindPattern("C7 81 84 07 00 00 09 00 00 00");
        if(offset1Result.Found)
        {
            exeCopy[offset1Result.Offset + 6] = 10;
        }

        // Patch 2 - Remove check for music id 4 in level music initialization function
        var offset2Result = scanner.FindPattern("83 FE 04 74 0E");
        if(offset2Result.Found)
        {
            exeCopy[offset2Result.Offset + 2] = 0xFF;
        }

        // Patch 3 - remove partner.xml sig check
        var offset3Result = scanner.FindPattern("38 99 F3 00 00 00");
        if(offset3Result.Found)
        {
            exeCopy[offset3Result.Offset + 6] = 0x90;
            exeCopy[offset3Result.Offset + 7] = 0x90;
        }

        // Patch 4 - remove video memory check
        var offset4Result = scanner.FindPattern("73 47 68 ?? ?? ?? ?? 8D 8D 10 FA FF FF");
        if (offset4Result.Found)
        {
            exeCopy[offset4Result.Offset] = 0xEB;
        }

        // Patch 5 - remove video card check
        var offset5Result = scanner.FindPattern("E8 ?? ?? ?? ?? 84 C0 75 49");
        if(offset5Result.Found)
        {
            exeCopy[offset5Result.Offset + 7] = 0xEB;
        }

        string newExeFilename = Path.GetDirectoryName(pegglePath) + "\\Peggle-Patched.exe";
        File.WriteAllBytes(newExeFilename, exeCopy);

        Console.WriteLine("Restored unused level music: " + ((offset1Result.Found && offset2Result.Found) ? "Yes" : "No - sig not found"));
        Console.WriteLine("Removed sig check: " + (offset3Result.Found ? "Yes" : "No - sig not found"));
        Console.WriteLine("Removed video card checks: " + ((offset4Result.Found && offset5Result.Found) ? "Yes" : "No - sig not found"));
        Console.WriteLine(string.Empty);

        Console.WriteLine("Finished patching executable.");
        Console.WriteLine($"Patched executable location: {newExeFilename}");
        Console.WriteLine("Press enter to exit.");

        Console.ReadLine();
    }
}
