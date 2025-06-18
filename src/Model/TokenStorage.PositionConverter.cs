using EmmyLua.LanguageServer.Framework.Protocol.Model;
using NMLServer.Model.Lexis;

namespace NMLServer.Model;

using TRangeInfo = (Position offset, int length);

internal partial struct TokenStorage
{
    /// <summary>
    /// The wrapper entity to optimize converting ranges of sorted positions.
    /// </summary>
    /// <param name="lineLengths"></param>
    public struct PositionConverter(List<int> lineLengths)
    {
        private int _line = 0;
        private int _character = 0;
        private int _offset = 0;
        private List<TRangeInfo>? _linesInfo = null;

        /// <summary>
        /// Convert start position of token to position in file.
        /// </summary>
        /// <param name="offset">The index of first token symbol.</param>
        /// <returns>The two-coordinate representation of passed source position.</returns>
        public Position LocalToProtocol(int offset)
        {
            if (offset == _offset)
            {
                return new Position(_line, _character);
            }
            if (offset > _offset)
            {
                _character += offset - _offset;
            }
            else
            {
                _character = offset;
                _line = 0;
            }
            _offset = offset;

            int length = lineLengths[_line];

            while (_character > length)
            {
                _character -= length;
                length = lineLengths[++_line];
            }
            if (_character == length && _line < lineLengths.Count - 1)
            {
                ++_line;
                _character -= length;
            }
            return new Position(_line, _character);
        }

        /// <summary>
        /// For given token, get sequence of covering line ranges.
        /// </summary>
        public List<TRangeInfo> LocalToProtocol(CommentToken token)
        {
            var offset = token.start;
            var length = token.length;

            int lineLength;
            if (offset == _offset)
            {
                lineLength = lineLengths[_line];
                goto label_caughtUp;
            }
            if (offset > _offset)
            {
                _character += offset - _offset;
            }
            else
            {
                _character = offset;
                _line = 0;
            }

            lineLength = lineLengths[_line];
            while (_character >= lineLength)
            {
                _character -= lineLength;
                lineLength = lineLengths[++_line];
            }

            label_caughtUp:
            _offset = offset + length;
            (_linesInfo ??= []).Clear();

            {
                int oldCharacter = _character;
                _character += length;
                if (_character <= lineLength)
                {
                    _linesInfo.Add((new Position(_line, oldCharacter), length));
                    return _linesInfo;
                }
                _linesInfo.Add((new Position(_line, oldCharacter), lineLength - oldCharacter));
            }

            do
            {
                _character -= lineLength;
                _linesInfo.Add((new Position(_line, 0), lineLength));
                lineLength = lineLengths[++_line];
            }
            while (_character > lineLength);

            // Handle two different cases for tokens ending on last character: on last line and not
            if (_character == lineLength)
            {
                _linesInfo.Add((new Position(_line, 0), lineLength));
                if (_line < lineLengths.Count - 1)
                {
                    _character -= lineLength;
                    ++_line;
                }
            }
            else
            {
                _linesInfo.Add((new Position(_line, 0), _character));
            }
            return _linesInfo;
        }
    }
}