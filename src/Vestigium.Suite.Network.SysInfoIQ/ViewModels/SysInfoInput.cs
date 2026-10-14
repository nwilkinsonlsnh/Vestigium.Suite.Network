using System.IO;

namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels;

public static class SysInfoInput
{
    public static bool TryOutputFolder(string? path, out string? folder, out string? reject)
    {
        folder = null;
        reject = null;

        var trimmed = path?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            reject = "Output folder is required.";
            return false;
        }

        if (trimmed.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            reject = "Output folder contains invalid characters.";
            return false;
        }

        if (!Path.IsPathRooted(trimmed))
        {
            reject = "Output folder must be an absolute path.";
            return false;
        }

        folder = trimmed;
        return true;
    }
}
