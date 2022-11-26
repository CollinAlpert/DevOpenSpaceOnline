using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PreferStringEmptyOverStringLiteralAnalyzer : DiagnosticAnalyzer
{
	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.RegisterSyntaxNodeAction(OnStringLiteralAnalysis, SyntaxKind.StringLiteralExpression);
	}

	private static void OnStringLiteralAnalysis(SyntaxNodeAnalysisContext context)
	{
		context.CancellationToken.ThrowIfCancellationRequested();
		var stringLiteral = (LiteralExpressionSyntax)context.Node;
		// exclude attribute arguments
		if (stringLiteral.Parent is AttributeArgumentSyntax
		    // exclude default parameter values
		    or EqualsValueClauseSyntax { Parent: ParameterSyntax }
		    // exclude switch statements
		    or CaseSwitchLabelSyntax
		    // exclude switch expressions
		    or ConstantPatternSyntax { Parent: SwitchExpressionArmSyntax })
		{
			return;
		}

		var text = stringLiteral.Token.Text;
		if(text == "\"\"")
		{
			var diagnostic = Diagnostic.Create(DiagnosticDescriptors.PreferStringEmptyOverStringLiteral, stringLiteral.GetLocation());
			context.ReportDiagnostic(diagnostic);
		}
	}

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
		DiagnosticDescriptors.PreferStringEmptyOverStringLiteral
	);
}