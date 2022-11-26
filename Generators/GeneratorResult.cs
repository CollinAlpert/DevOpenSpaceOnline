using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Generators;

public class GeneratorResult
{
	public string? TypeName { get; set; }

	public SourceText? Source { get; set; }

	public Diagnostic? Diagnostic { get; set; }

	public bool IsValid => TypeName is not null && Source is not null && Diagnostic is null;

	public static GeneratorResult Empty { get; } = new();

	public GeneratorResult(string typeName, SourceText source)
	{
		TypeName = typeName;
		Source = source;
	}

	public GeneratorResult(Diagnostic diagnostic)
	{
		Diagnostic = diagnostic;
	}

	private GeneratorResult()
	{
	}
}