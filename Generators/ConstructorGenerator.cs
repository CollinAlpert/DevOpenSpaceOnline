using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Generators;

[Generator]
public class ConstructorGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var syntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(IsCandidate, Transform);
		context.RegisterSourceOutput(syntaxProvider, (ctx, result) =>
		{
			if (result.IsValid)
			{
				ctx.AddSource($"{result.TypeName}.g.cs", result.Source!);
			}
			else if (result.Diagnostic is not null)
			{
				ctx.ReportDiagnostic(result.Diagnostic);
			}
		});
	}

	private static GeneratorResult Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
	{
		var classDeclaration = (ClassDeclarationSyntax)context.Node;
		var fields = classDeclaration.Members
			.OfType<FieldDeclarationSyntax>()
			.Where(f => !f.Modifiers.Any(SyntaxKind.StaticKeyword)
			            && f.Modifiers.Any(SyntaxKind.PrivateKeyword)
			            && f.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))
			.ToList();

		var parameters = fields.SelectMany(
			f => f.Declaration.Variables.Select(v =>
			{
				return Parameter(
					Identifier(
						v.Identifier.Text.TrimStart('_')
					)
				).WithType(f.Declaration.Type);
			}));
		var parameterList = ParameterList(
			SeparatedList(
				parameters
			)
		);

		var expressions = fields.SelectMany(
			f => f.Declaration.Variables.Select(v =>
			{
				return ExpressionStatement(
					AssignmentExpression(
						SyntaxKind.SimpleAssignmentExpression,
						IdentifierName(v.Identifier),
						IdentifierName(v.Identifier.Text.TrimStart('_'))
					)
				);
			}));
		var body = Block(expressions);

		var constructor = ConstructorDeclaration(classDeclaration.Identifier)
			.WithParameterList(parameterList)
			.WithBody(body)
			.WithModifiers(
				TokenList(
					Token(SyntaxKind.InternalKeyword)
				)
			);

		var ns = classDeclaration.GetNamespace();
		if (ns is null)
		{
			var diagnostic = Diagnostic.Create(DiagnosticDescriptors.NoNamespaceFound, classDeclaration.GetLocation());

			return new GeneratorResult(diagnostic);
		}
		
		cancellationToken.ThrowIfCancellationRequested();

		var sourceText = CompilationUnit()
			.WithMembers(
				SingletonList<MemberDeclarationSyntax>(
					FileScopedNamespaceDeclaration(ns)
						.WithMembers(
						SingletonList<MemberDeclarationSyntax>(
							ClassDeclaration(classDeclaration.Identifier)
								.WithModifiers(
									TokenList(
										Token(SyntaxKind.PublicKeyword),
										Token(SyntaxKind.PartialKeyword)
									)
								).WithMembers(
									SingletonList<MemberDeclarationSyntax>(constructor)
								)
						)
					)
				)
			).NormalizeWhitespace()
			.GetText(Encoding.UTF8);

		return new GeneratorResult(classDeclaration.Identifier.Text, sourceText);
	}

	private static bool IsCandidate(SyntaxNode node, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		return node is ClassDeclarationSyntax classDeclarationSyntax
		       && classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword)
		       && classDeclarationSyntax.Members.All(m => m is not ConstructorDeclarationSyntax)
		       && classDeclarationSyntax.Members
			       .OfType<FieldDeclarationSyntax>()
			       .Any(f => !f.Modifiers.Any(SyntaxKind.StaticKeyword)
			                 && f.Modifiers.Any(SyntaxKind.PrivateKeyword)
			                 && f.Modifiers.Any(SyntaxKind.ReadOnlyKeyword));
		
	}
}