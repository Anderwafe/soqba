using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace soqba.ViewModels;

    enum QuestionType : byte
    {
        Text,
        Input,
        Select,
    }

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private List<ViewModelBase>? _questions = null;

    public ViewModelBase FallbackValue {get; private set;} = new TextQuestionViewModel("Check your internet connection, or files, idk");

    public MainViewModel()
    {
        if(Design.IsDesignMode)
        {
            Questions = [FallbackValue];
        }
    }

    public void Initialize(Stream? inputStream)
    {
        Console.Error.WriteLine("Initializing");
        if(inputStream is null)
        {
            Questions = [FallbackValue];
            Console.Error.WriteLine("Fallback");
            return;
        }

        byte[] bytebuffer = new byte[4096];

        //Console.Error.WriteLine(sr.Read(charbuffer, 0, 9));
        inputStream.ReadExactly(bytebuffer, 0, 9);
        if(!string.Equals(new string(Encoding.UTF8.GetChars(bytebuffer[..9])), "SOQBAFORM"))
        {
            Questions = [FallbackValue];
            Console.Error.WriteLine("Magic invalid - " + new string(Encoding.UTF8.GetChars(bytebuffer[..9])));
            return;
        }
        /* int readed = 0;
        while((readed += inputStream.Read(bytebuffer, readed, 8-readed)) != 8){} */
        inputStream.ReadExactly(bytebuffer, 0, 8);

        long crc = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(bytebuffer, 0));

        Questions = [];
        inputStream.ReadByte(); // skip version byte
        while(true)
        {
            int questType = inputStream.ReadByte();
            Console.Error.WriteLine("Question type: " + questType);
            if(questType == -1) break;
            inputStream.ReadExactly(bytebuffer, 0, 32);
            switch((QuestionType)questType)
            {
                case QuestionType.Text:
                {
                    Console.Error.WriteLine("Text");
                    /* readed = 0;
                    while((readed += inputStream.Read(bytebuffer, readed, 4-readed)) != 4){} */
                    inputStream.ReadExactly(bytebuffer, 0, 4);

                    int size = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytebuffer, 0));
                    Console.Error.WriteLine(size);
                    inputStream.ReadExactly(bytebuffer, 0, size);
                    Console.Error.WriteLine(new string(Encoding.UTF8.GetChars(bytebuffer[..size])));
                    Questions.Add(new TextQuestionViewModel(new string(Encoding.UTF8.GetChars(bytebuffer[..size]))));
                } break;
                case QuestionType.Input:
                {
                    Console.Error.WriteLine("Input");
                    /* readed = 0;
                    while((readed += inputStream.Read(bytebuffer, readed, 4-readed)) != 4){} */
                    inputStream.ReadExactly(bytebuffer, 0, 4);

                    int size = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytebuffer, 0));
                    inputStream.ReadExactly(bytebuffer, 0, size);
                    Console.Error.WriteLine(new string(Encoding.UTF8.GetChars(bytebuffer[..size])));
                    Questions.Add(new InputQuestionViewModel(new string(Encoding.UTF8.GetChars(bytebuffer[..size]))));
                } break;
                case QuestionType.Select:
                {
                    Console.Error.WriteLine("Select");
                    int minCount = inputStream.ReadByte();
                    int maxCount = inputStream.ReadByte();

                    /* readed = 0;
                    while((readed += inputStream.Read(bytebuffer, readed, 4-readed)) != 4){} */
                    inputStream.ReadExactly(bytebuffer, 0, 4);
                    int size = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytebuffer, 0));
                    inputStream.ReadExactly(bytebuffer, 0, size);
                    string tip = new string(Encoding.UTF8.GetChars(bytebuffer[..size]));

                    int count = inputStream.ReadByte();
                    List<string> varis = new List<string>(count);
                    while(count-- > 0)
                    {
                        /* readed = 0;
                        while((readed += inputStream.Read(bytebuffer, readed, 4-readed)) != 4){} */
                        inputStream.ReadExactly(bytebuffer, 0, 4);
                        size = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytebuffer, 0));
                        inputStream.ReadExactly(bytebuffer, 0, size);
                        varis.Add(new string(Encoding.UTF8.GetChars(bytebuffer[..size])));
                    }
                    Console.Error.WriteLine($"{minCount} - {maxCount}; {tip}; {string.Join(';', varis)}");

                    Questions.Add(new SelectQuestionViewModel((minCount, maxCount), tip, varis.ToArray()));
                } break;
                default:
                {
                    Questions.Add(new TextQuestionViewModel("OOpsie..."));
                } break;
            }
        }
    }

    public Func<string, Task>? CopyToClipboardRequest {get; set;}

    private bool CanCopyClipboard()
    {
        return (CopyToClipboardRequest is not null) && (Questions?.All(x => (x as IResultViewModel<string>)?.CanGetResult() ?? true) ?? false);
    }

    [RelayCommand(CanExecute = nameof(CanCopyClipboard))]
    private async Task CopyClipboard()
    {
        if(Questions is not null)
            await (CopyToClipboardRequest?.Invoke(string.Join('\n', Questions.Where(x => x is IResultViewModel<string>).Select(x => x switch 
            {
                InputQuestionViewModel ques => $"{ques.Tip}:\n{ques.UserInput}",
                SelectQuestionViewModel ques => $"{ques.Tip}:\n{ques.GetResult().Result}",
                _ => $"?",
            }))) ?? Task.CompletedTask);
    }
}
