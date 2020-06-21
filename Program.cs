using System;
using System.IO;

namespace ExtractShaderCache
{
    class Program
    {
        private byte[] headerBytes = {0x00, 0x03, 0xFE, 0xFF, 0xFE, 0xFF };
        private byte[] endBytes = {};

        static void Main(string[] args)
        {
            if(args.Length != 1)
            {
                Console.WriteLine("Please enter exactly one input file.");
            }
            else
            {
                // Make this do the extracting and writing
            }
        }
    }
}
