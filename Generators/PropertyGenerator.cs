using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Generators;

[Generator]
public class PropertyGenerator : IIncrementalGenerator
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
			            && f.Modifiers.Any(SyntaxKind.PrivateKeyword));
		
		var properties = fields.SelectMany(
			f => f.Declaration.Variables.Select(v =>
			{
				
				return PropertyDeclaration(f.Declaration.Type, UnderscoreToPascalCase(v.Identifier.Text))
					.WithModifiers(
                    TokenList(
                        Token(
                            TriviaList(
                                Trivia(
                                    DocumentationCommentTrivia(
                                        SyntaxKind.SingleLineDocumentationCommentTrivia,
                                        List<XmlNodeSyntax>(
                                            new XmlNodeSyntax[]{
                                                XmlText()
                                                .WithTextTokens(
                                                    TokenList(
                                                        XmlTextLiteral(
                                                            TriviaList(
                                                                DocumentationCommentExterior("///")
                                                            ),
                                                            " ",
                                                            " ",
                                                            TriviaList()
                                                        )
                                                    )
                                                ),
                                                XmlExampleElement(
                                                    SingletonList<XmlNodeSyntax>(
                                                        XmlText()
                                                        .WithTextTokens(
                                                            TokenList(
                                                                new []{
                                                                    XmlTextNewLine(
                                                                        TriviaList(),
                                                                        "\n",
                                                                        "\n",
                                                                        TriviaList()
                                                                    ),
                                                                    XmlTextLiteral(
                                                                        TriviaList(
                                                                            DocumentationCommentExterior("    ///")
                                                                        ),
                                                                        $" Gets {v.Identifier.Text}",
                                                                        $" Gets {v.Identifier.Text}",
                                                                        TriviaList()
                                                                    ),
                                                                    XmlTextNewLine(
                                                                        TriviaList(),
                                                                        "\n",
                                                                        "\n",
                                                                        TriviaList()
                                                                    ),
                                                                    XmlTextLiteral(
                                                                        TriviaList(
                                                                            DocumentationCommentExterior("    ///")
                                                                        ),
                                                                        " ",
                                                                        " ",
                                                                        TriviaList()
                                                                    )
                                                                }
                                                            )
                                                        )
                                                    )
                                                )
                                                .WithStartTag(
                                                    XmlElementStartTag(
                                                        XmlName(
                                                            Identifier("summary")
                                                        )
                                                    )
                                                )
                                                .WithEndTag(
                                                    XmlElementEndTag(
                                                        XmlName(
                                                            Identifier("summary")
                                                        )
                                                    )
                                                ),
                                                XmlText()
                                                .WithTextTokens(
                                                    TokenList(
                                                        XmlTextNewLine(
                                                            TriviaList(),
                                                            "\n",
                                                            "\n",
                                                            TriviaList()
                                                        )
                                                    )
                                                )
                                            }
                                        )
                                    )
                                )
                            ),
                            SyntaxKind.InternalKeyword,
                            TriviaList()
                        )
                    )
                ).WithExpressionBody(
						ArrowExpressionClause(
							IdentifierName(v.Identifier)
						)
					).WithSemicolonToken(
						Token(SyntaxKind.SemicolonToken)
					).WithLeadingTrivia();
			}));

		var ns = classDeclaration.GetNamespace();
		if (ns is null)
		{
			var diagnostic = Diagnostic.Create(DiagnosticDescriptors.NoNamespaceFound, classDeclaration.GetLocation());

			return new GeneratorResult(diagnostic);
		}

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
										List<MemberDeclarationSyntax>(properties)
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

		return node is ClassDeclarationSyntax classDeclaration
		       && classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
		       && classDeclaration.Members
			       .OfType<FieldDeclarationSyntax>()
			       .Any(f => !f.Modifiers.Any(SyntaxKind.StaticKeyword)
			                 && f.Modifiers.Any(SyntaxKind.PrivateKeyword));
	}

	private static string UnderscoreToPascalCase(string s)
	{
		s = s.TrimStart('_');
		var array = s.ToCharArray();
		array[0] = char.ToUpper(array[0]);

		return new string(array);
	}
}