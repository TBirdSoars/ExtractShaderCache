using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ExtractShaderCache
{
    class Program
    {
        // Full path of cache file for filestream
        private static string cachePath = "";
        // Path for output shader files
        private static string outputPath = AppDomain.CurrentDomain.BaseDirectory;

        // First 4 bytes of shader cache
        private static byte[] fileCheck = {0x44, 0x43, 0x41, 0x43}; //DCAC
        // First 6 bytes of compiled shader
        private static byte[] headerBytes = {0x00, 0x03, 0xFE, 0xFF, 0xFE, 0xFF};
        // Last 4 bytes of compiled shader
        private static byte[] endBytes = {0xFF, 0xFF, 0x00, 0x00};

        private static FileStream fs;
        private static BinaryReader br;
        private static BinaryWriter bw;

        private static bool cacheCheck = false;
        
        public static void Main(string[] args)
        {
            // Input Validation
            //
            // Ensure something was passed to the program
            if(args.Length != 1)
            {
                Console.WriteLine("Please enter one valid Ishiiruka D3D9 shader cache file");
            }
            else
            {
                // Set path of file
                cachePath = args[0];

                // First, check if the input file is actually a shader cache
                using (fs = new FileStream(cachePath, FileMode.Open, FileAccess.Read))
                {
                    // Make sure that the file has at least enough data for the comparison
                    if (fs.Length >= 4)
                    {
                        // Read file stream
                        using (br = new BinaryReader(fs, Encoding.ASCII))
                        {
                            // Read in first 4 bytes
                            byte[] buffer = br.ReadBytes(fileCheck.Length);

                            // Now compare byte sequence to known intro bytes
                            if (buffer.SequenceEqual(fileCheck))
                            {
                                cacheCheck = true;
                            }
                        }
                    }
                }
            }

            // If input is valid, continue with extraction
            if(cacheCheck)
            {

            }
            else
            {
                Console.WriteLine("Please enter one valid Ishiiruka D3D9 shader cache file");
            }
        }
    }
}
