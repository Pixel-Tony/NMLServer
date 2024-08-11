namespace NMLServer;

internal partial class Document
{
    public ref struct PositionConverter(IReadOnlyList<int> lineLengths)
    {
        private int _line = 0;
        private int _character = 0;
        private int _offset = 0;

        /// <summary>
        /// Convert start position of token to position in file.
        /// </summary>
        /// <param name="offset">The index of first token symbol.</param>
        /// <returns>The two-coordinate representation of passed source position.</returns>
        public (int line, int character) LocalToProtocol(int offset)
        {
            if (offset > _offset)
            {
                // Return to the start of current line
                _character = offset - (_offset - _character);
            }
            else if (offset == _offset)
            {
                return (_line, _character);
            }
            else
            {
                _line = 0;
                _character = offset;
            }
            _offset = offset;

            for (int length = lineLengths[_line]; _character >= length; length = lineLengths[_line])
            {
                _character -= length;
                ++_line;
            }
            return (_line, _character);
        }

        /// <summary>
        /// For given token, get sequence of ranges in form (line, startCharacter, length), that cover it.
        /// </summary>
        /// <param name="offset">The index of first symbol of the token.</param>
        /// <param name="length">The length of the token.</param>
        public IEnumerable<(int line, int @char, int length)> LocalToProtocol(int offset, int length)
        {
            int lineLength;
            if (offset > _offset)
            {
                _offset = offset + length;
                _character = offset - _offset;
                lineLength = lineLengths[_line];
                while (_character >= lineLength)
                {
                    _character -= lineLength;
                    ++_line;
                    lineLength = lineLengths[_line];
                }
            }
            else if (offset == _offset)
            {
                lineLength = lineLengths[_line];
            }
            else
            {
                _line = 0;
                _offset = offset + length;
                _character = offset;
                lineLength = lineLengths[0];
                while (_character >= lineLength)
                {
                    _character -= lineLength;
                    lineLength = lineLengths[++_line];
                }
            }
            // length now contains shift from index of first character of fist line
            length += _character;
            // If on the same line - return only this single line
            if (length <= lineLength)
            {
                return new[] { (_line, _character, length - _character) };
            }
            // Otherwise - return 'end piece' of first line, middle lines and 'start piece' of last line.
            // Getting coordinates of ending position
            List<(int line, int @char, int offset)> result = [(_line, _character, lineLength - _character)];

            lineLength = lineLengths[++_line];
            // > not >= because length is not offset
            while (length > lineLength)
            {
                result.Add((_line, 0, lineLength));
                length -= lineLength;
                lineLength = lineLengths[++_line];
            }
            _character = length;
            result.Add((_line, 0, _character));

            return result;
        }
    }
}