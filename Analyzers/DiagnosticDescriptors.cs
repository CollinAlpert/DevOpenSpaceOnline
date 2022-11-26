using Microsoft.CodeAnalysis;

namespace Analyzers;

public static class DiagnosticDescriptors
{
	public static readonly DiagnosticDescriptor PreferStringEmptyOverStringLiteral = new(
		"DEV001",
		"Prefer 'string.Empty' over \"\"",
		"Prefer 'string.Empty' over \"\"",
		"Style",
		DiagnosticSeverity.Warning,
		true
	);
	public static readonly DiagnosticDescriptor RemoveAsyncSuffix = new(
		"DEV002",
		"Remove 'Async' suffix",
		"Remove 'Async' suffix",
		"Style",
		DiagnosticSeverity.Warning,
		true
	);
}