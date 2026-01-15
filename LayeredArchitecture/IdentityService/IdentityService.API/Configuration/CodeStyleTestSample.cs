// =============================================================================
// CODE STYLE TEST SAMPLE - DELETE THIS FILE AFTER TESTING
// This file intentionally violates .editorconfig rules to demonstrate warnings
// =============================================================================

// VIOLATION 1: System usings should come first (dotnet_sort_system_directives_first)
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

// VIOLATION 2: Block-scoped namespace instead of file-scoped (csharp_style_namespace_declarations)
namespace IdentityService.API.Configuration
{
    // VIOLATION 3: Missing accessibility modifier (dotnet_style_require_accessibility_modifiers)
    class CodeStyleTestSample
    {
        // VIOLATION 4: Missing braces (csharp_prefer_braces)
        public void TestBraces(bool condition)
        {
            if (condition)
                Console.WriteLine("No braces!");
        }
    }
}

// =============================================================================
// CORRECT VERSION - How the code should look:
// =============================================================================
//
// using System;
// using System.Collections.Generic;
//
// using Microsoft.Extensions.Options;
//
// namespace IdentityService.API.Configuration;
//
// public class CodeStyleTestSampleCorrect
// {
//     public void TestBraces(bool condition)
//     {
//         if (condition)
//         {
//             Console.WriteLine("With braces!");
//         }
//     }
// }
