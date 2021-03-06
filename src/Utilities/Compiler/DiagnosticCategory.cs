﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Analyzer.Utilities
{
    internal static class DiagnosticCategory
    {
        public const string Design = nameof(Design);
        public const string Globalization = nameof(Globalization);
        public const string Interoperability = nameof(Interoperability);
        public const string Mobility = nameof(Mobility);
        public const string Performance = nameof(Performance);
        public const string Reliability = nameof(Reliability);
        public const string Security = nameof(Security);
        public const string Usage = nameof(Usage);
        public const string Naming = nameof(Naming);
        public const string Library = nameof(Library);
        public const string Documentation = nameof(Documentation);
        public const string Maintainability = nameof(Maintainability);

        public const string RoslyDiagnosticsDesign = nameof(RoslyDiagnosticsDesign);
        public const string RoslyDiagnosticsMaintainability = nameof(RoslyDiagnosticsMaintainability);
        public const string RoslyDiagnosticsPerformance = nameof(RoslyDiagnosticsPerformance);
        public const string RoslyDiagnosticsReliability = nameof(RoslyDiagnosticsReliability);
        public const string RoslyDiagnosticsUsage = nameof(RoslyDiagnosticsUsage);

        public const string MicrosoftCodeAnalysisCorrectness = nameof(MicrosoftCodeAnalysisCorrectness);
        public const string MicrosoftCodeAnalysisDesign = nameof(MicrosoftCodeAnalysisDesign);
        public const string MicrosoftCodeAnalysisDocumentation = nameof(MicrosoftCodeAnalysisDocumentation);
        public const string MicrosoftCodeAnalysisLocalization = nameof(MicrosoftCodeAnalysisLocalization);
        public const string MicrosoftCodeAnalysisPerformance = nameof(MicrosoftCodeAnalysisPerformance);
        public const string MicrosoftCodeAnalysisCompatibility = nameof(MicrosoftCodeAnalysisCompatibility);
    }
}