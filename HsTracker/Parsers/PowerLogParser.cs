using System.Text;
using System.Text.RegularExpressions;
using HsTracker;
using HsTracker.Cache;
using HsTracker.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HsTracker.Parsers;

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
        @"BLOCK_START BlockType=PLAY.*(?=.*player=(?<player>\d+))(?=.*cardId=(?<card_id>\S+))",
        RegexOptions.Multiline
    )]
    private static partial Regex BlockTypePlayProperties { get; }

    private readonly StringBuilder powerLogBlock = new();

    private bool keepReading = false;

    private readonly ILogger<PowerLogParser> _logger;

    private readonly IMemoryCache _memoryCache;

    public PowerLogParser(ILogger<PowerLogParser> logger, IMemoryCache memoryCache)
    {
        _logger = logger;
        _memoryCache = memoryCache;
        _memoryCache.Set(HsTrackerConsts.MyCards, new List<HsCard>());
        _memoryCache.Set(HsTrackerConsts.EnemyCards, new List<HsCard>());
    }

    public void ParseBlock(StringBuilder powerLogBlock)
    {
        if (ShowEntityProperties.Match(powerLogBlock.ToString()) is Match enemy && enemy.Success)
        {
            string player = enemy.Groups["player"].Value;
            string cardId = enemy.Groups["card_id"].Value;

            var cardData = _memoryCache.Get<List<HsCardData>>(HsTrackerConsts.HsCardData);

            var playedCard = new HsCard
            {
                Player = sbyte.Parse(player),
                CardId = cardId,
                CardData = cardData?.FirstOrDefault(cd => cd.CardId == cardId),
            };

            _memoryCache.Get<List<HsCard>>(HsTrackerConsts.EnemyCards)?.Add(playedCard);

            _logger.LogInformation(
                "{cardName} was played by player {player} (Local Card ID: {cardId} | Global Card ID: {id})",
                playedCard.CardData?.Name,
                player,
                cardId,
                playedCard.CardData?.Id
            );
        }
        else if (BlockTypePlayProperties.Match(powerLogBlock.ToString()) is Match me && me.Success)
        {
            string player = me.Groups["player"].Value;
            string cardId = me.Groups["card_id"].Value;

            var cardData = _memoryCache.Get<List<HsCardData>>(HsTrackerConsts.HsCardData);

            var playedCard = new HsCard
            {
                Player = sbyte.Parse(player),
                CardId = cardId,
                CardData = cardData?.FirstOrDefault(cd => cd.CardId == cardId),
            };

            _memoryCache.Get<List<HsCard>>(HsTrackerConsts.MyCards)?.Add(playedCard);

            _logger.LogInformation(
                "{cardName} was played by player {player} (Local Card ID: {cardId} | Global Card ID: {id})",
                playedCard.CardData?.Name,
                player,
                cardId,
                playedCard.CardData?.Id
            );
        }
    }

    public void ReadBlock(StreamReader? streamReader)
    {
        while (streamReader?.ReadLine() is string line)
        {
            if (PowerTaskListDebugPrintPowerBlockTypePlay.IsMatch(line))
            {
                powerLogBlock.AppendLine(line);
                keepReading = true;
            }
            else if (PowerTaskListDebugPrintPower.IsMatch(line) && keepReading)
            {
                powerLogBlock.AppendLine(line);
            }
            else if (powerLogBlock.Length > 0)
            {
                keepReading = false;
                ParseBlock(powerLogBlock);
                powerLogBlock.Clear();
            }
        }
    }
}
