using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;

namespace Analyzers.Tests;

using Verifier = CSharpCodeFixVerifier<PreferStringEmptyOverStringLiteralAnalyzer, PreferStringEmptyOverStringLiteralCodeFix, XUnitVerifier>;
using Analyzer = AnalyzerVerifier<PreferStringEmptyOverStringLiteralAnalyzer, AnalyzerWithoutCodeFixTest<PreferStringEmptyOverStringLiteralAnalyzer>, XUnitVerifier>;

public class PreferStringEmptyOverStringLiteralTests
{
	private const string Scaffold = @"
namespace UnitTests;

public class Test {{
	public void M() {{
		{0}
	}}
}}";

	[Fact]
	public Task Test()
	{
		var code = string.Format(Scaffold, "string x  = [|\"\"|];");
		var fixedCode = string.Format(Scaffold, "string x = string.Empty;");

		return Verifier.VerifyCodeFixAsync(code, fixedCode);
	}

	[Fact]
	public Task DoNotRaiseInParameters()
	{
		const string code = @"
namespace UnitTests;

public class Test {
	public void M(string s = """") {
	}
}";

		return Verifier.VerifyAnalyzerAsync(code);
	}

	[Fact]
	public Task DoNotRaiseInAttribute()
	{
		const string code = @"
using System.ComponentModel.DataAnnotations;

namespace UnitTests;

public class Test {
	[RegularExpression("""")]
	public string S {get;set;}
}";

		return Verifier.VerifyAnalyzerAsync(code);
	}

	[Fact]
	public Task DoNotRaiseInSwitch()
	{
		const string code = @"
namespace UnitTests;

public class Test {
	public void M(string s)
	{
		switch (s)
		{
			case """":
				break;
		}	
	}
}";

		return Verifier.VerifyAnalyzerAsync(code);
	}

	[Fact]
	public Task DoNotRaiseInSwitchExpression()
	{
		const string code = @"
namespace UnitTests;

public class Test {
	public int M(string s)
	{
		return s switch
		{
			"""" => 1
		};
	}
}";

		return Verifier.VerifyAnalyzerAsync(code);
	}
}