namespace esharp.solidity.compiler
{

    private int _position;

    private readonly string _text;

    public class Lexer
    {
        public Lexer(string text)
        {
            _text = text;
            _position = 0;
        }

        public void Next()
        {
            _position++;
        }
    }
}