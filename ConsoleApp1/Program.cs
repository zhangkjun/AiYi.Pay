using AiYi.Core;
using AiYi.Pay;
using System;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var id = (int)GlobalEnumVars.WeiChatPayTradeType.APP;// Enum.Parse(typeof(GlobalEnumVars.WeiChatPayTradeType), GlobalEnumVars.WeiChatPayTradeType.APP) ;
            Console.WriteLine("Hello, World!");
        }
    }
}
