using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Services;
using Roslyn.Compilers;
using AddParametersToException.Utils;
using Roslyn.Services.Formatting;

namespace AddParametersToException
{
    public class AddParameterExceptionLoggingCodeAction : ICodeAction
    {
        private readonly ExceptionLoggingActionInfo actionInfo;
        private readonly ISemanticModel semanticModel;

        public AddParameterExceptionLoggingCodeAction(ExceptionLoggingActionInfo actionInfo)
        {
            this.actionInfo = actionInfo;
            this.semanticModel = actionInfo.Document.GetSemanticModel();
        }

        public CodeActionEdit GetEdit(CancellationToken cancellationToken = new CancellationToken())
        {
            return new CodeActionEdit(actionInfo.Document.UpdateSyntaxRoot(UpdateCatchClause(cancellationToken)));
        }

        public string Description { get { return "Log parameters in catch block"; } }

        // TODO Remove parameters logged in wrong exception
        // Done Dealing with multiple catches? Test first
        // Done Add DAL_ as appropriate
        // TODO Code errors
        // Done Remove existing parameter exception logging
        // Done Insert at correct location
        // Done Logging objects (AddToDataCollection)
        // Done Check for existing of inner exception. E.g. var newEx = new SomeTombolaException(ex). Difficult
        public CommonSyntaxNode UpdateCatchClause(CancellationToken cancellationToken)
        {
            var exceptionDeclarationIdentifier = actionInfo.ExceptionDeclarationToken;

            var newCatchBlockWithOldLoggingStatmentsRemoved = actionInfo.CatchClause.RemoveNodes(actionInfo.ExceptionVariableUsages, SyntaxRemoveOptions.KeepNoTrivia);
            SyntaxList<StatementSyntax> newCatchBlockStatements = newCatchBlockWithOldLoggingStatmentsRemoved.Block.Statements.Insert(actionInfo.LoggingStatementsInsertionLineNumber, CreateNewExceptionLoggingStatements(exceptionDeclarationIdentifier));
            var newCatchBlock = Syntax.Block(newCatchBlockStatements);
            var oldRoot = actionInfo.Document.GetSyntaxRoot(cancellationToken);
            CommonSyntaxNode newRoot = oldRoot.ReplaceNode(actionInfo.CatchClause, actionInfo.CatchClause.WithBlock(newCatchBlock));
            
            return newRoot.Format(FormattingOptions.GetDefaultOptions()).GetFormattedRoot();
        }



        private StatementSyntax[] CreateNewExceptionLoggingStatements(SyntaxToken exceptionDeclarationIdentifier)
        {
            List<StatementSyntax> statements = new List<StatementSyntax>();
            string execeptionLoggingPrefix = SharedMethods.GetExceptionNamePrefix(actionInfo.ContainingMethodParameters.First());

            foreach (var parameter in actionInfo.ContainingMethodParameters)
            {
                // For parameters you need get the type of parameter.Type for whatever reason
                var parameterType = semanticModel.GetTypeInfo(parameter.Type).Type;
                var exceptionMethodName = "Data.Add";

                if (parameterType.IsReferenceType && parameterType.SpecialType != SpecialType.System_String)
                    exceptionMethodName = "AddToDataCollection";

                statements.Add(Syntax.ParseStatement(string.Format("{0}.{1}(\"{2}\", {3});", exceptionDeclarationIdentifier, exceptionMethodName, execeptionLoggingPrefix + parameter.Identifier.Value.ToString().FirstCharToUpper(), parameter.Identifier.Value)).WithTrailingTrivia(Syntax.EndOfLine("\r\n"))); 
            }

            return statements.ToArray();
        }

        
    }
}