using System;
using System.Linq;
using System.Threading;
using AddParametersToException;
using NUnit.Framework;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Tests.Utils;

namespace Tests
{
    [TestFixture]
    public class AddParameterExceptionLoggingCodeActionTests
    {
        private AddParameterExceptionLoggingCodeAction codeAction;

        [SetUp]
        public void SetUp()
        {
            //codeAction = new AddParameterExceptionLoggingCodeAction();
        }

//        [Test]
//        public void GetEdit_AddsParameterExceptionLogging()
//        {
//            SyntaxTree tree = SyntaxTree.ParseText(@"
//using System;
//
//namespace Tests.Utils
//{
//    public class Test
//    {
//        public void TestMethod()
//        {
//            try
//            {
//                Console.WriteLine(""stuff"");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(""Things"");
//            }
//        }
//    }
//}");
//            IDocument doc = TestDocument.Create(tree.GetRoot());
//            codeAction = new AddParameterExceptionLoggingCodeAction(doc, tree.GetRoot().DescendantNodes().OfType<CatchClauseSyntax>().First(), null);

//            //var full = codeAction.GetEdit().UpdatedSolution.GetDocument(doc.Id).GetSyntaxRoot().ToFullString();
//            var fullstuff = codeAction.UpdateCatchClause(new CancellationToken()).GetText();
 
//            Assert.AreEqual(@"
//using System;
//
//namespace Tests.Utils
//{
//    public class Test
//    {
//        public void TestMethod()
//        {
//            try
//            {
//                Console.WriteLine(""stuff"");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(""Things"");
//                ex.Data.Add(""aaa"", ""bbb"");
//            }
//        }
//    }
//}", fullstuff);
//        }
    }
}