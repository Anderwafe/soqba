// format of generated file:
// sizes in bytes
// byte is 8 bits
// lines starting with -- is informational
// strings consists of UTF-8 characters
// endianness is big

// -- VERSION 0x0
// -- format of next lines: offset size contains
// -- 0x0 0xA [HEADER]
// 0 9 SOQBAFORM -- in ascii
// 9 8 [CRC-64-ECMA starting from 18 byte of file]
// 17 1 [version of format]
// 18 ?? [questions...]

// question format:
// 0 1 [question type]
// 1 32 [question label]
// 33 ?? [question data]
// question label needed for in questionnare communication, or future machinery processing

// text question needs to only show text to user
// text question data:
// 0 4 [size of text]
// 4 [size of text] [text data in UTF-8]

// input field question needs to freely get text from user, with some tip on top of inputfield control
// input field data:
// 0 4 [size of tip]
// 4 [size of tip] [tip text in UTF-8]
// in GUI, this question will be something like:
// [tip]:
// [input field]

// select is typical selecting question with some choices (yes/no)
// 0 1 [min count of selected choices]
// 1 1 [max count of selected choices]
// 2 4 [size of text]
// 6 [size of text] [tip text]
// 6+[size of text] 1 [choices number]
// -- for every choice
// n 4 [length of text]
// n+4 [length of text] [text of choice in UTF-8]


// format of input file:
// file consists of continuous blocks of questions
// each question starts from question type with optional label, delimited by space with max length = 8. next lines consists of questions parameters.
// questions delimits by empty line.

using System.IO.Hashing;
using System.Net;
using System.Text;

namespace Program
{
    enum QuestionType : byte
    {
        Text,
        Input,
        Select,
    }

    static class ResGen
    {
        static long lineCount = 0;

        private static int Main()
        {
            // byte[] output = Crc64.Hash(Random.Shared.GetItems<byte>(Enumerable.Range(0, 256).Select(byte.CreateTruncating).ToArray(), 128));
            // Console.WriteLine(output.Length);
            // Console.WriteLine(string.Join(' ', output));
            // Console.WriteLine(BitConverter.ToInt64(output));

            var magicBytes = Encoding.ASCII.GetBytes("SOQBAFORM");
            byte formatVersion = 0;

            List<byte> output = new List<byte>();
            
            string? inputLine = null;
            while((inputLine = Console.ReadLine()) is not null)
            {
                lineCount++;
                if(string.IsNullOrWhiteSpace(inputLine)) continue;
                var splittedLine = inputLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                QuestionType type;
                try
                {
                    type = Enum.Parse<QuestionType>(splittedLine[0], true);
                }
                catch(Exception e)
                {
                    Console.Error.WriteLine($"Line {lineCount} should contain question type. {e.Message}");
                    if(Console.IsInputRedirected) return 1;
                    continue;
                }

                byte[] labelBuffer = new byte[32];
                if(splittedLine.Length != 1)
                {
                    if(splittedLine[1].Length > 8)
                    {
                        Console.Error.WriteLine($"Label on {lineCount} line should consists of less or equal than 8 bytes");
                        if(Console.IsInputRedirected) return 1;
                        Console.Error.WriteLine("Please, write question type with optional label");
                        continue;
                    }

                    Encoding.UTF8.GetBytes(splittedLine[1], labelBuffer);
                }
                    
                switch(type)
                {
                    case QuestionType.Text:
                    {
                        var buffer = TextQuestion(Console.In).ToArray();
                        if(buffer.Length == 0)
                        {
                            if(Console.IsInputRedirected) return 1;
                            Console.Error.WriteLine("Please, write question type with optional label");
                            continue;
                        }
                
                        output.Add((byte)type);
                        output.AddRange(labelBuffer);
                        output.AddRange(buffer);
                    } break;
                    case QuestionType.Input:
                    {
                        var buffer = InputQuestion(Console.In).ToArray();
                        if(buffer.Length == 0)
                        {
                            if(Console.IsInputRedirected) return 1;
                            Console.Error.WriteLine("Please, write question type with optional label");
                            continue;
                        }

                        output.Add((byte)type);
                        output.AddRange(labelBuffer);
                        output.AddRange(buffer);
                    } break;
                    case QuestionType.Select:
                    {
                        var buffer = SelectQuestion(Console.In).ToArray();
                        if(buffer.Length == 0)
                        {
                            if(Console.IsInputRedirected) return 1;
                            Console.Error.WriteLine("Please, write question type with optional label");
                            continue;
                        }

                        output.Add((byte)type);
                        output.AddRange(labelBuffer);
                        output.AddRange(buffer);
                    } break;
                }
            }

            byte[] result = [..magicBytes, ..Crc64.Hash(output.ToArray()), formatVersion, ..output];

            if(Console.IsOutputRedirected) 
            {
                Stream outputStream = Console.OpenStandardOutput();
                outputStream.Write(result);
                return 0;
            }
            File.AppendAllBytes("./output.sor", result);
            return 0;
        }

        private static IEnumerable<byte> TextQuestion(TextReader inputReader)
        {
            List<byte> result = new ();

            long resultSize = 0;

            string? input = null;
            while(!string.IsNullOrWhiteSpace((input = inputReader.ReadLine())))
            {
                lineCount++;
                var encoded = Encoding.UTF8.GetBytes(input);
                if((resultSize += encoded.Length) > int.MaxValue)
                {
                    Console.Error.WriteLine("Too much, onii-chan. Truncate by prev line");
                    Console.Error.WriteLine($"{lineCount}: Cancelling text question events.");
                    break;
                }
                result.AddRange(encoded);
            }
            lineCount++;

            return [..BitConverter.GetBytes(IPAddress.HostToNetworkOrder(result.Count)), ..result];
        }

        private static IEnumerable<byte> InputQuestion(TextReader inputReader) => TextQuestion(inputReader);

        private static IEnumerable<byte> SelectQuestion(TextReader inputReader)
        {
            List<byte> result = new ();

            if(!Console.IsInputRedirected) Console.Error.WriteLine("Format:\nmin count of selected choices\nmax count of selected choices\nquestion text\nchoices text");

            string? input = inputReader.ReadLine();
            lineCount++;
            if(input is null) return [];
            if(!int.TryParse(input, out int min))
            {
                Console.Error.WriteLine($"At {lineCount}: should provide correct integer type");
                return [];
            }

            input = inputReader.ReadLine();
            lineCount++;
            if(input is null) return [];
            if(!int.TryParse(input, out int max))
            {
                Console.Error.WriteLine($"At {lineCount}: should provide correct integer type");
                return [];
            }

            input = inputReader.ReadLine();
            lineCount++;
            if(input is null) return [];
            string tipText = input;

            int choicesCount = 0;
            while(!string.IsNullOrWhiteSpace((input = inputReader.ReadLine())))
            {
                choicesCount++;
                lineCount++;

                result.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Encoding.UTF8.GetByteCount(input))));
                result.AddRange(Encoding.UTF8.GetBytes(input));
            }
            lineCount++;
            
            if(choicesCount < 2 ||
                min > choicesCount ||
                min < 0 ||
                max > choicesCount ||
                max < min) 
                {
                    Console.Error.WriteLine($"Incorrect value at {lineCount} line");
                    return [];
                }

            return [(byte)min,
                    (byte)max,
                    ..BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Encoding.UTF8.GetByteCount(tipText))),
                    ..Encoding.UTF8.GetBytes(tipText),
                    (byte)choicesCount,
                    ..result];
        }
    }
}
