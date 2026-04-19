namespace HsTracker.Core;

public class PowerLogParser
{
    public void ParseLine(string line)
    {
        int index = line.IndexOf('-');
        if (index == -1)
        {
            return;
        }
        // "BlockType=(?<BlockType>.*?) Entity=\[(?<Entity>.*?)\]"m
        // PowerTaskList.DebugPrintPower()
        //  BLOCK_START BlockType=POWER Entity=[entityName=Warhorse Trainer id=56 zone=PLAY zonePos=3 cardId=CORE_AT_075 player=2]
        //  EffectCardId=System.Collections.Generic.List`1[System.String] EffectIndex=0 Target=0 SubOption=-1
        string logEntryKey = line[..index].Trim();
        string logEntryValue = line[(index + 1)..].Trim();

        logEntryKey = logEntryKey[logEntryKey.LastIndexOf(' ')..];
    }
}
