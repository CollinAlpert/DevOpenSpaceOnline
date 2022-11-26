using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class PreferStringEmptyOverStringLiteralCodeFix : CodeFixProvider
{
	private const string Title = "Use 'string.Empty'";

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root?.FindNode(context.Span) is not LiteralExpressionSyntax stringLiteral)
		{
			return;
		}

		var stringEmpty = MemberAccessExpression(
			SyntaxKind.SimpleMemberAccessExpression,
			PredefinedType(
				Token(SyntaxKind.StringKeyword)
			),
			IdentifierName("Empty")
		);

		var newRoot = root.ReplaceNode(stringLiteral, stringEmpty.WithTriviaFrom(stringLiteral));
		var codeAction = CodeAction.Create(Title, _ =>
		{
			var newDocument = context.Document.WithSyntaxRoot(newRoot);

			return Task.FromResult(newDocument);
		}, Title);
		
		context.RegisterCodeFix(codeAction, context.Diagnostics);
	}

	public override FixAllProvider GetFixAllProvider()
	{
		return WellKnownFixAllProviders.BatchFixer;
	}

	public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
		DiagnosticDescriptors.PreferStringEmptyOverStringLiteral.Id
	);
}