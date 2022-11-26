using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generators;

public static class SyntaxNodeExtensions
{
	public static NameSyntax? GetNamespace(this SyntaxNode node)
	{
		var parent = node.Parent;
		while (parent != null)
		{
			if(parent is BaseNamespaceDeclarationSyntax ns)
			{
				return ns.Name;
			}

			parent = parent.Parent;
		}

		return null;
	}
}