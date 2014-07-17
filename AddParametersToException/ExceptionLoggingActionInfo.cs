using System.Collections.Generic;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;

namespace AddParametersToException
{
    public class ExceptionLoggingActionInfo
    {
        public IDocument                        Document                             { get; set; }
        public CatchClauseSyntax                CatchClause                          { get; set; }
        public List<ParameterSyntax>            ContainingMethodParameters           { get; set; }
        public SyntaxToken                      ExceptionDeclarationToken            { get; set; }
        public int                              LoggingStatementsInsertionLineNumber { get; set; }
        public List<ExpressionStatementSyntax>  ExceptionVariableUsages              { get; set; }

        public ExceptionLoggingActionInfo(IDocument document, CatchClauseSyntax catchClause, List<ParameterSyntax> containingMethodParameters, SyntaxToken exceptionDeclarationToken, int loggingStatementsInsertionLineNumber, List<ExpressionStatementSyntax> exceptionVariableUsages)
        {
            Document                             = document;
            CatchClause                          = catchClause;
            ContainingMethodParameters           = containingMethodParameters;
            ExceptionDeclarationToken            = exceptionDeclarationToken;
            LoggingStatementsInsertionLineNumber = loggingStatementsInsertionLineNumber;
            ExceptionVariableUsages              = exceptionVariableUsages;

            if(exceptionVariableUsages == null)
                ExceptionVariableUsages = new List<ExpressionStatementSyntax>();
        }
    }
}