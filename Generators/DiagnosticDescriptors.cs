using Microsoft.CodeAnalysis;

namespace Generators;

internal static class DiagnosticDescriptors
{
	public static readonly DiagnosticDescriptor NoNamespaceFound = new(
		"GEN001",
		"No namespace found.",
		"No namespace found.",
		"Generation",
		DiagnosticSeverity.Warning,
		true);
}