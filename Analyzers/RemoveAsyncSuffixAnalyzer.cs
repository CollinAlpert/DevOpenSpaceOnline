using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RemoveAsyncSuffixAnalyzer : DiagnosticAnalyzer
{
	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.RegisterCompilationStartAction(OnCompilationStart);
	}

	private static void OnCompilationStart(CompilationStartAnalysisContext context)
	{
		var task = context.Compilation.GetTypeByMetadataName(typeof(Task).FullName);
		var taskOfT = context.Compilation.GetTypeByMetadataName(typeof(Task<>).FullName);
		var valueTask = context.Compilation.GetTypeByMetadataName(typeof(ValueTask).FullName);
		var valueTaskOfT = context.Compilation.GetTypeByMetadataName(typeof(ValueTask<>).FullName);
		
		context.RegisterSymbolAction(ctx => OnMethodDeclarationAnalysis(ctx, task, taskOfT, valueTask, valueTaskOfT), SymbolKind.Method);
	}

	private static void OnMethodDeclarationAnalysis(SymbolAnalysisContext context, INamedTypeSymbol? task, INamedTypeSymbol? taskOfT, INamedTypeSymbol? valueTask, INamedTypeSymbol? valueTaskOfT)
	{
		var method = (IMethodSymbol)context.Symbol;
		var returnType = method.ReturnType;
		if ((method.IsAsync
		     || returnType.Equals(task, SymbolEqualityComparer.Default)
		     || returnType.Equals(taskOfT, SymbolEqualityComparer.Default)
		     || returnType.Equals(valueTask, SymbolEqualityComparer.Default)
		     || returnType.Equals(valueTaskOfT, SymbolEqualityComparer.Default))
		    && method.Name.EndsWith("Async"))
		{
			var diagnostic = Diagnostic.Create(DiagnosticDescriptors.RemoveAsyncSuffix, method.Locations[0]);
			context.ReportDiagnostic(diagnostic);
		}
	}

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
		DiagnosticDescriptors.RemoveAsyncSuffix
	);
}