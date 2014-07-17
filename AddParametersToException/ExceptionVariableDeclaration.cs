using Roslyn.Compilers.CSharp;

namespace AddParametersToException
{
    public class ExceptionVariableDeclaration
    {
        public SyntaxToken  Identifier { get; set; }

        private int blockIndex;
        public int          BlockIndex
        {
            get { return blockIndex; } 
            set
            {
                blockIndex = value;

                if (blockIndex < 0)
                    blockIndex = 0;
            }
        }

        public ExceptionVariableDeclaration(SyntaxToken identifier, int blockIndex)
        {
            Identifier = identifier;
            BlockIndex = blockIndex;
        }
    }
}