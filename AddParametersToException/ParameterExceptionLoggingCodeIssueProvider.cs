using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AddParametersToException.Utils;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Editor;

namespace AddParametersToException
{
    [ExportCodeIssueProvider("AddParametersToException", LanguageNames.CSharp)]
    public class ParameterExceptionLoggingCodeIssueProvider : ICodeIssueProvider
    {
        // MethodDeclarationSyntax - MethodDeclaration - has body
        // TryStatementSytanx - TryStatement - Catches
        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxNode node, CancellationToken cancellationToken)
        {
            if (node.GetType() != typeof(MethodDeclarationSyntax)) return null;
            var methodDeclaration = (MethodDeclarationSyntax)node;

            // No try statements? Not interested
            if (!methodDeclaration.DescendantNodes().OfType<TryStatementSyntax>().Any())
                return null;

            // No method parameters? Let's get out of here
            IEnumerable<ParameterSyntax> methodParameters = methodDeclaration.ParameterList.DescendantNodes().OfType<ParameterSyntax>().ToList();

            if (!methodParameters.Any()) return null;

            IEnumerable<TryStatementSyntax> tryBlocks = methodDeclaration.DescendantNodes().OfType<TryStatementSyntax>();

            var codeIssues = new List<CodeIssue>();
            foreach (var tryBlock in tryBlocks)
            {
                foreach (var catchClause in tryBlock.Catches)
                {
                    // Check for another declared exception and pass to action (with block index)
                    var catchIdentifier = catchClause.Declaration.Identifier;

                    if (catchIdentifier.Kind == SyntaxKind.None)
                        continue;

                    var loggingStatementsInsertionLineNumber = 0;

                    try
                    {
                        // Find usages of exception variable
                        var innerExceptionDeclaration = FindExceptionVariableDeclarations(catchClause.Block, document.GetSemanticModel(cancellationToken));
                        if (innerExceptionDeclaration != null)
                        {
                            catchIdentifier = innerExceptionDeclaration.Identifier;
                            loggingStatementsInsertionLineNumber = innerExceptionDeclaration.BlockIndex + 1;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        return null; // More than one inner exception declared. Can't handle. Run away!
                    }
                    
                    var exceptionVariableUsages = VariableUsages(catchClause, catchIdentifier);

                    // Check whats logged are actually the parameters
                    if (!AreAllMethodParametersLoggedInException(methodParameters, exceptionVariableUsages))
                    {
                        var actionInfo = new ExceptionLoggingActionInfo(document, catchClause, methodParameters, catchIdentifier, loggingStatementsInsertionLineNumber, exceptionVariableUsages);

                        codeIssues.Add(new CodeIssue(CodeIssueKind.Warning, catchClause.Span, "Method parameters not correctly logged in catch block", new AddParameterExceptionLoggingCodeAction(actionInfo)));
                    }
                }
            }

            if (codeIssues.Count > 0)
                return codeIssues;

            return null;
        }

        private bool AreAllMethodParametersLoggedInException(List<ParameterSyntax> parameters, List<ExpressionStatementSyntax> exceptionLoggingStatements)
        {
            if (parameters.Count() != exceptionLoggingStatements.Count())
                return false;

            var exceptionInvocations = new List<InvocationExpressionSyntax>();
            foreach (var exceptionLoggingStatement in exceptionLoggingStatements)
            {
                exceptionInvocations.Add(exceptionLoggingStatement.DescendantNodes().OfType<InvocationExpressionSyntax>().First());
            }

            string execeptionLoggingPrefix = SharedMethods.GetExceptionNamePrefix(parameters.First());

            foreach (var parameter in parameters)
            {
                // Determine whether parameter is used in exceptionLoggingStatements
                bool parameterIsLogged = false;
                foreach (var exceptionInvocation in exceptionInvocations)
                {
                    // Only interested in ex.Data.Add("sdfs", "sdfs") or AddToDataCollection (sfsdf). Think this is a bit flakey
                    if (exceptionInvocation.ArgumentList.Arguments.Count == 1 || exceptionInvocation.ArgumentList.Arguments.Count == 2) 
                    {
                        string exceptionArgumentValue = null;
                        string parameterValue = null;

                        // Bit gash this
                        if (exceptionInvocation.ArgumentList.Arguments.Count == 2)
                        {
                            var expression = exceptionInvocation.ArgumentList.Arguments[0].Expression as LiteralExpressionSyntax;
                            if (expression != null) exceptionArgumentValue = expression.Token.Value.ToString();
                            parameterValue = execeptionLoggingPrefix + parameter.Identifier.Value.ToString().FirstCharToUpper();
                        }

                        if (exceptionArgumentValue != null && exceptionArgumentValue == parameterValue)
                        {
                            parameterIsLogged = true;
                            break;
                        }
                    }
                }

                if (!parameterIsLogged)
                    return false;
            }

            return true;
        }

        /// <exception cref="InvalidOperationException">When more than one exception variable is defined in block</exception>
        private ExceptionVariableDeclaration FindExceptionVariableDeclarations(BlockSyntax block, ISemanticModel semanticModel)
        {
            ExceptionVariableDeclaration exceptionVariableDeclaration = null;
            var localDeclarations = block.DescendantNodes().OfType<LocalDeclarationStatementSyntax>();

            bool oneExceptionVariableFound = false;
            foreach (LocalDeclarationStatementSyntax localDeclaration in localDeclarations)
            {
                // TODO Could this fail? Probably
                var creationExpression = localDeclaration.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().First();
                var exceptionCreationIndentifier = creationExpression.DescendantNodes().OfType<IdentifierNameSyntax>().First();
                var exceptionTypeInfo = semanticModel.GetTypeInfo(exceptionCreationIndentifier);
                ITypeSymbol topMostBaseType = FindTopMostBaseType(exceptionTypeInfo.Type);

                if (topMostBaseType.Name == "Exception")
                {
                    if (oneExceptionVariableFound)
                        throw new InvalidOperationException("Run away! I don't know how to handle this!");

                    int indexInBlock = block.Statements.IndexOf(localDeclaration);
                    SyntaxToken identifier = localDeclaration.DescendantNodes().OfType<VariableDeclaratorSyntax>().First().Identifier;
                    exceptionVariableDeclaration  = new ExceptionVariableDeclaration(identifier, indexInBlock);
                    oneExceptionVariableFound = true;
                }
            }

            return exceptionVariableDeclaration;
        }

        private ITypeSymbol FindTopMostBaseType(ITypeSymbol myType)
        {
            var currentType = myType;

            while (currentType.BaseType != null && currentType.BaseType.Name != "Object")
                currentType = currentType.BaseType;

            return currentType;
        }

        private List<ExpressionStatementSyntax> VariableUsages(CommonSyntaxNode node, SyntaxToken identifier)
        {
            return (from c in node.DescendantNodes().OfType<ExpressionStatementSyntax>()
                    where (from id in c.DescendantNodes().OfType<IdentifierNameSyntax>()
                           where id.Identifier.Value == identifier.Value
                                && id.Parent is MemberAccessExpressionSyntax
                                && (((MemberAccessExpressionSyntax)id.Parent).Name.ToString() == "Data" || ((MemberAccessExpressionSyntax)id.Parent).Name.ToString() == "AddToDataCollection")
                           select id).Any()
                    select c).ToList();
        }

        

        public IEnumerable<Type> SyntaxNodeTypes
        {
            get
            {
                yield return typeof(SyntaxNode);
            }
        }

        #region Unimplemented ICodeIssueProvider members

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxToken token, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> SyntaxTokenKinds
        {
            get
            {
                return null;
            }
        }

        #endregion
    }
}
