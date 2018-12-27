using System.IO;
using System.IO.Compression;

namespace UnityEngine.XR.iOS
{
    public static class CompressionHelper
    {
	    /// <summary>
	    ///     Compress using deflate.
	    /// </summary>
	    /// <returns>The byte compress.</returns>
	    /// <param name="source">Source.</param>
	    public static byte[] ByteArrayCompress(byte[] source)
        {
            using (var ms = new MemoryStream())
            using (var compressedDStream = new DeflateStream(ms, CompressionMode.Compress, true))
            {
                compressedDStream.Write(source, 0, source.Length);

                compressedDStream.Close();

                var destination = ms.ToArray();

                Debug.Log(source.Length + " vs " + ms.Length);

                return destination;
            }
        }

	    /// <summary>
	    ///     Decompress using deflate.
	    /// </summary>
	    /// <returns>The byte decompress.</returns>
	    /// <param name="source">Source.</param>
	    public static byte[] ByteArrayDecompress(byte[] source)
        {
            using (var input = new MemoryStream(source))
            using (var output = new MemoryStream())
            using (var decompressedDstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                decompressedDstream.CopyTo(output);

                var destination = output.ToArray();

                Debug.Log("Decompress Size : " + output.Length);

                return destination;
            }
        }

        public static long CopyTo(this Stream source, Stream destination)
        {
            var buffer = new byte[2048];
            int bytesRead;
            long totalBytes = 0;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, bytesRead);
                totalBytes += bytesRead;
            }

            return totalBytes;
        }
    }
}