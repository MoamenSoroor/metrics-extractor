﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Diagnostics;

#if true // CODEANALYSIS_V3_OR_BETTER
using System.Linq;
#endif

namespace Analyzer.Utilities
{
    /// <summary>
    /// MSBuild property names that are required to be threaded as analyzer config options.
    /// </summary>
    /// <remarks>const fields in this type are automatically discovered and used to generate build_properties entries in the generated .globalconfig</remarks>
    internal static class MSBuildPropertyOptionNames
    {
        public const string TargetFramework = nameof(TargetFramework);
        public const string TargetPlatformMinVersion = nameof(TargetPlatformMinVersion);
        public const string UsingMicrosoftNETSdkWeb = nameof(UsingMicrosoftNETSdkWeb);
        public const string ProjectTypeGuids = nameof(ProjectTypeGuids);
        public const string InvariantGlobalization = nameof(InvariantGlobalization);
        public const string PlatformNeutralAssembly = nameof(PlatformNeutralAssembly);
        public const string EnforceExtendedAnalyzerRules = nameof(EnforceExtendedAnalyzerRules);
    }

    internal static class MSBuildPropertyOptionNamesHelpers
    {
        [Conditional("DEBUG")]
        public static void VerifySupportedPropertyOptionName(string propertyOptionName)
        {
#if true // CODEANALYSIS_V3_OR_BETTER
            Debug.Assert(typeof(MSBuildPropertyOptionNames).GetFields().Single(f => f.Name == propertyOptionName) != null);
#endif
        }
    }
}
