using System.Text;
using System.Text.RegularExpressions;

namespace HsTracker.Core.Parsers;

public partial class PowerLogParser
{
    [GeneratedRegex(@".* PowerTaskList.DebugPrintPower\(\) - BLOCK_START BlockType=PLAY")]
    private static partial Regex PowerTaskListDebugPrintPowerBlockTypePlay { get; }

    [GeneratedRegex(@".* PowerTaskList.DebugPrintPower\(\) -")]
    private static partial Regex PowerTaskListDebugPrintPower { get; }

    [GeneratedRegex(
        @"SHOW_ENTITY.*(?=.*player=(?<player>\d+))(?=.*CardID=(?<card_id>\S+))",
        RegexOptions.Multiline
    )]
    private static partial Regex ShowEntityProperties { get; }

    [GeneratedRegex(
        @"BLOCK_START BlockType=PLAY.*(?=.*player=(?<player>\d+))(?=.*cardId=(?<card_id>\S+))(?=.*entityName=(?<entity_name>.*?) (?=\w+=))",
        RegexOptions.Multiline
    )]
    private static partial Regex BlockTypePlayProperties { get; }

    StringBuilder powerLogBlock = new();
    bool keepReading = false;

    public void ParseBlock(StringBuilder powerLogBlock)
    {
        if (ShowEntityProperties.Match(powerLogBlock.ToString()) is Match op && op.Success)
        {
            string player = op.Groups["player"].Value;
            string cardId = op.Groups["card_id"].Value;
            Console.WriteLine($"Player: {player}, CardID: {cardId}");
        }
        else if (BlockTypePlayProperties.Match(powerLogBlock.ToString()) is Match p && p.Success)
        {
            string player = p.Groups["player"].Value;
            string cardId = p.Groups["card_id"].Value;
            string entityName = p.Groups["entity_name"].Value;
            Console.WriteLine($"Player: {player}, CardID: {cardId}, EntityName: {entityName}");
        }
    }

    public void ReadBlock(StreamReader? streamReader)
    {
        // TODO: Without a breakpoint here, the reader don't read the whole file. Because the game can't write all lines to the file at once.
        // StringBuilder powerLogBlock = new();
        // bool keepReading = false;
        while (streamReader?.ReadLine() is string line)
        {
            if (PowerTaskListDebugPrintPowerBlockTypePlay.IsMatch(line))
            {
                // System.Console.WriteLine($"First line detected: {line}");
                powerLogBlock.AppendLine(line);
                keepReading = true;
            }
            else if (PowerTaskListDebugPrintPower.IsMatch(line) && keepReading)
            {
                // System.Console.WriteLine($"Continue line: {line}");
                powerLogBlock.AppendLine(line);
            }
            else if (powerLogBlock.Length > 0)
            {
                keepReading = false;
                // System.Console.WriteLine($"Sent block to parser: {powerLogBlock}");
                ParseBlock(powerLogBlock);
                powerLogBlock.Clear();
            }
        }
    }
}
