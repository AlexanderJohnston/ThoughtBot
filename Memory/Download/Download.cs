using System.Net;

namespace Memory.Download
{
    public class TextDownloader
    {
        private string _documentLink;
        private string _downloadedText;
        private Tokenizer _tokenizer;

        public TextDownloader(string documentLink)
        {
            _documentLink = documentLink;
            _tokenizer = new();
        }

        public void DownloadText()
        {
            using (WebClient client = new WebClient())
            {
                _downloadedText = client.DownloadString(_documentLink);
            }
        }

        public int[] TokenizeText()
        {
            return _tokenizer.Tokenize(_downloadedText);
        }
    }
}