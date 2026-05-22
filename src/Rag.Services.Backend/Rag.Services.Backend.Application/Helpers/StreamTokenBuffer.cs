using System.Text;

namespace Rag.Services.Backend.Application.Helpers
{
    public class StreamTokenBuffer
    {
        private readonly StringBuilder _buffer = new();
        private const int MinBufferSize = 5; // Minimum characters to buffer before yielding

        public IEnumerable<string> ProcessToken(string token)
        {
            _buffer.Append(token);

            // Check for natural break points (spaces, punctuation)
            var bufferText = _buffer.ToString();
            var lastSpaceIndex = bufferText.LastIndexOf(' ');
            var lastPunctuationIndex = Math.Max(
                bufferText.LastIndexOf('.'),
                Math.Max(bufferText.LastIndexOf(','), bufferText.LastIndexOf('!'))
            );

            var breakPoint = Math.Max(lastSpaceIndex, lastPunctuationIndex);

            // If we have a natural break point and enough buffered content
            if (breakPoint > 0 && breakPoint >= MinBufferSize)
            {
                // Yield up to and including the break point
                var toYield = bufferText.Substring(0, breakPoint + 1);
                _buffer.Clear();
                _buffer.Append(bufferText.Substring(breakPoint + 1));
                yield return toYield;
            }
            // If buffer is getting too large without a break point, yield anyway
            else if (_buffer.Length > 50)
            {
                var toYield = _buffer.ToString();
                _buffer.Clear();
                yield return toYield;
            }
        }

        public string Flush()
        {
            var remaining = _buffer.ToString();
            _buffer.Clear();
            return remaining;
        }
    }
}
