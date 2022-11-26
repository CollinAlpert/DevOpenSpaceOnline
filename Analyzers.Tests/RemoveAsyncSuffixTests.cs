using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;

namespace Analyzers.Tests;

using Verifier = CSharpCodeFixVerifier<RemoveAsyncSuffixAnalyzer, RemoveAsyncSuffixCodeFix, XUnitVerifier>;

public class RemoveAsyncSuffixTests
{
	[Fact]
	public Task Test()
	{
		var code = @"
using System.Threading.Tasks;

namespace UnitTests;

public class Test {
	public Task [|DoAsync|]() {
		return Task.CompletedTask;
	}

	public void M() {
		DoAsync();
	}
}";
		var fixedCode = @"
using System.Threading.Tasks;

namespace UnitTests;

public class Test {
	public Task Do() {
		return Task.CompletedTask;
	}

	public void M() {
		Do();
	}
}";

		return Verifier.VerifyCodeFixAsync(code, fixedCode);
	}
}