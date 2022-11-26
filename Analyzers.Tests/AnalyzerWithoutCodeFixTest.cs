using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Analyzers.Tests;

public class AnalyzerWithoutCodeFixTest<TAnalyzer> : AnalyzerTest<XUnitVerifier> where TAnalyzer : DiagnosticAnalyzer, new()
{
	public AnalyzerWithoutCodeFixTest()
	{
		TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net60;
	}
    
	protected override CompilationOptions CreateCompilationOptions()
	{
		return new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
	}

	protected override ParseOptions CreateParseOptions()
	{
		return new CSharpParseOptions(LanguageVersion.CSharp10, DocumentationMode.Diagnose);
	}

	protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
	{
		return new[] { new TAnalyzer() };
	}

	protected override string DefaultFileExt { get; } = "cs";
	public override string Language { get; } = LanguageNames.CSharp;
}