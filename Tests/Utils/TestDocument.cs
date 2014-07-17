using Roslyn.Compilers.CSharp;
using Roslyn.Services;

namespace Tests.Utils
{
    public static class TestDocument
    {
        public static IDocument Create(CompilationUnitSyntax root)
        {
            var solutionId = SolutionId.CreateNewId();
            var projectId = ProjectId.CreateNewId(solutionId);
            var documentId = DocumentId.CreateNewId(projectId, "test");

            var solution = Solution.Create(solutionId).AddProject(projectId, "whatevs", "stuff", "C#").AddDocument(documentId, "My file name");
            var doc = solution.GetDocument(documentId);

            return doc.UpdateSyntaxRoot(root);
        }
    }
}