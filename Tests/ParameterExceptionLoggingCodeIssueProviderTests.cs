using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AddParametersToException;
using NUnit.Framework;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Services;
using Tests.Utils;

namespace Tests
{
    [TestFixture]
    public class ParameterExceptionLoggingCodeIssueProviderTests
    {
        private ParameterExceptionLoggingCodeIssueProvider codeIssueProvider; 

        [SetUp]
        public void SetUp()
        {
            codeIssueProvider = new ParameterExceptionLoggingCodeIssueProvider();
        }

        [Test]
        public void GetIssues_NotAMethod_ReturnsNull()
        {
            SyntaxTree tree = SyntaxTree.ParseText(@"int i = 0;");
            IDocument doc = TestDocument.Create(tree.GetRoot());

            Assert.That(codeIssueProvider.GetIssues(doc, tree.GetRoot().ChildNodes().First(), new CancellationToken()), Is.Null);
        }

        [Test]
        public void GetIssues_MethodDoesNotContainTry_ReturnsNull()
        {
            SyntaxTree noTry = SyntaxTree.ParseText(@"public class Test
                                                        {
                                                            public void DoesNotContainTry()
                                                            {
            
                                                            }
                                                        }");
            IDocument doc = TestDocument.Create(noTry.GetRoot());

            Assert.That(codeIssueProvider.GetIssues(doc, noTry.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First(), new CancellationToken()), Is.Null);
        }
    }
}
