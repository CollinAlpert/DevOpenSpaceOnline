using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class RemoveAsyncSuffixCodeFix : CodeFixProvider
{
	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root?.FindNode(context.Span) is not MethodDeclarationSyntax method)
		{
			return;
		}

		var newName = method.Identifier.Text.Replace("Async", string.Empty);
		var title = $"Rename to '{newName}'";
		var codeAction = CodeAction.Create(
			title, 
			ct => CreateChangedSolutionAsync(context.Document, method, newName, ct),
			title);
		
		context.RegisterCodeFix(codeAction, context.Diagnostics);
	}

	private static async Task<Solution> CreateChangedSolutionAsync(
		Document document,
		MethodDeclarationSyntax method,
		string newName,
		CancellationToken cancellationToken)
	{
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		var methodSymbol = semanticModel?.GetDeclaredSymbol(method) ?? throw new ArgumentException("Can't find method symbol");
		var solution = document.Project.Solution;

		return await Renamer.RenameSymbolAsync(solution, methodSymbol, newName, solution.Options, cancellationToken);
	}

	public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
		DiagnosticDescriptors.RemoveAsyncSuffix.Id
	);
}