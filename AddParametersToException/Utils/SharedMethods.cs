using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using System.Linq;

namespace AddParametersToException.Utils
{
    public static class SharedMethods
    {
        /// <summary>
        /// Returns DAL_ or empty string dependant on class name
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string GetExceptionNamePrefix(CommonSyntaxNode node)
        {
            var classDeclaration = node.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();

            if (classDeclaration != null && classDeclaration.Identifier.Value.ToString().Contains("Repository"))
                return "DAL_";
             
            return string.Empty;
        } 
    }
}