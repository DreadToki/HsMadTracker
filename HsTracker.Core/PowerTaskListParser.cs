namespace HsTracker.Core;

// TODO: Initial pseudo class to parse PowerTaskList logs.
class PowerTaskListParser : LogParser
{
    public event EventHandler<string>? CardPlayed;

    public override void ProcessLine(string line)
    {
        // TODO: Implement the logic of parsing PowerTaskList logs and extracting relevant information about card plays.
        // Or extract every feature of the line(card) and create methods to handle different features.
        // I need cardIds, zones they are played, any other information.
    }
    // TODO: Create an event that will be triggered when a card is detected
}
