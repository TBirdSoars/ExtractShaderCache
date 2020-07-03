using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ExtractShaderCache
{
    class Program
    {
        // Bool to check if cache is actually a shader cache
        private static bool cacheCheck = false;
        // Bools for if shaders are vertex or pixel shaders
        private static bool isVS = false;
        private static bool isPS = false;

        // Full path of cache file for filestream
        private static string cachePath = "";
        // File name of shader cache
        private static string cacheName = "";
        // Path for output shader files, w/o file name
        private static string outputPath = AppDomain.CurrentDomain.BaseDirectory;
        // File extension, to be determined by shader type
        private static string fileExt = "";

        // First 4 bytes of shader cache
        private static byte[] fileCheck = {0x44, 0x43, 0x41, 0x43}; //DCAC
        // First 6 bytes of compiled vertex shader
        private static byte[] headerBytesVS = {0x00, 0x03, 0xFE, 0xFF, 0xFE, 0xFF};
        // First 6 bytes of compiled pixel shader
        private static byte[] headerBytesPS = {0x00, 0x03, 0xFF, 0xFF, 0xFE, 0xFF};
        // Last 4 bytes of compiled shader
        private static byte[] endBytes = {0xFF, 0xFF, 0x00, 0x00};
        // The shader data to be output
        private static byte[] shaderData;

        // Filestream and reader/writer to handle file inputs and outputs
        private static FileStream fs;
        private static BinaryReader br;
        private static BinaryWriter bw;

        // Start position of shader in cache
        private static long startPosition = long.MaxValue;
        // End position of shader in cache
        private static long endPosition = long.MaxValue;
        // Position needed to reset the binary reader
        private static long resetHeaderPosition = long.MaxValue;
        // Position need to reset the binary reader
        private static long resetEndPosition = long.MaxValue;

        // Shader counter for output shader names
        private static int shaderNum = 1;
        // Length of the shader in bytes
        private static int numBytes = 0;

        public static void Main(string[] args)
        {
            // If input is valid, continue with extraction
            if(inputValidation(args))
            {
                // Display that cache was identified
                Console.WriteLine("Valid Shader Cache Found: " + cacheName);

                // Identify if cache is vertex or pixel shader cache
                isVS = cacheName.Contains("-vs-");
                isPS = cacheName.Contains("-ps-");

                // Set the file stream
                using (fs = new FileStream(cachePath, FileMode.Open, FileAccess.Read))
                {
                    // Set the reader
                    using (br = new BinaryReader(fs, Encoding.ASCII))
                    {
                        // Until the end of the file, find the shader headers
                        // Stop 6 bytes before file's end - the last 6 bytes obviously aren't a header
                        while (br.BaseStream.Position < br.BaseStream.Length - headerBytesVS.Length)
                        {
                            // Record starting position for next iteration of loop
                            resetHeaderPosition = br.BaseStream.Position + 1;

                            // Read in 6 bytes
                            byte[] buffer = br.ReadBytes(headerBytesVS.Length);

                            // If the buffer matches the header, record its starting position
                            // and then find the end of the shader
                            if (buffer.SequenceEqual(headerBytesVS) || buffer.SequenceEqual(headerBytesPS))
                            {
                                // Record position of start of shader header
                                startPosition = br.BaseStream.Position - headerBytesVS.Length;

                                // SANITY CHECK - display start positions
                                Console.WriteLine("Shader starts at byte: " + startPosition);

                                // Scan until the end of the file for the end of the shader
                                while (br.BaseStream.Position < br.BaseStream.Length - endBytes.Length &&
                                       endPosition == long.MaxValue)
                                {
                                    // Record starting position for next iteration of loop
                                    resetEndPosition = br.BaseStream.Position + 1;

                                    // Read in 4 bytes
                                    buffer = br.ReadBytes(endBytes.Length);

                                    // If the buffer matches the footer, record the position at the end of the footer
                                    // and then proceed with extraction
                                    if (buffer.SequenceEqual(endBytes))
                                    {
                                        // Record position of end of shader footer
                                        endPosition = br.BaseStream.Position;

                                        // SANITY CHECK - display end positions
                                        Console.WriteLine("Shader ends at byte: " + endPosition);

                                        // Determine length of shader
                                        numBytes = (int)(endPosition - startPosition);

                                        // Set the reader back to the start of the shader
                                        br.BaseStream.Position = startPosition;

                                        // Instantiate shader data array
                                        shaderData = new byte[numBytes];

                                        // Read the bytes into the shader array
                                        shaderData = br.ReadBytes(numBytes);
                                        
                                        // Choose appropriate file extension based on type of shader
                                        if(isPS)
                                        {
                                            fileExt = ".ps";
                                        }
                                        if(isVS)
                                        {
                                            fileExt = ".vs";
                                        }

                                        // Begin writing shader data to output file
                                        using (bw = new BinaryWriter(File.Open(outputPath + shaderNum + fileExt, FileMode.Create)))
                                        {
                                            bw.Write(shaderData);
                                        }

                                        // Increment shader number for next shader
                                        shaderNum++;
                                    }

                                    // Set br's position to 2nd byte of buffer
                                    br.BaseStream.Position = resetEndPosition;
                                }

                                // Reset endPosition for next shader
                                endPosition = long.MaxValue;
                            }

                            // Set br's position to 2nd byte of buffer
                            br.BaseStream.Position = resetHeaderPosition;
                        }
                    }
                }
            }
            else // Otherwise, output error message
            {
                Console.WriteLine("Please supply one valid Ishiiruka D3D9 shader cache file");
            }
        }

        // Input validation
        // Determines if input exists, where it is, and if it is a shader cache
        private static bool inputValidation(string[] args)
        {
            // Ensure something was passed to the program
            if (args.Length != 1)
            {
                return false;
            }
            else
            {
                // Set path of file
                cachePath = args[0];
                // Get cache file's name w/ extension
                cacheName = Path.GetFileName(cachePath);

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
                                return true;
                            } 
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    }
}
