using System;
using System.Collections.Generic;

namespace UserService.Domain.Constants;

public static class FileFolders
{
    public static readonly HashSet<string> AllowedFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "app-logos"
    };
}
