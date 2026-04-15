namespace HsTracker.Core;

// Consider using abstract class in order to use inheriting in related parser classes
abstract class LogParser
{
    // TODO: Create an event that will listen EVERY new line of PowerLog and will GET lines that are relevant to the game state
    /*
        1. PowerLog.ReadLines() reads logs line by line.
        2. For each line, it should check if the line contains relevant information about the game state (e.g., player actions, card plays, etc.).
        3. If a line is relevant, it should trigger an event (e.g., OnRelevantLineParsed) and pass the relevant information to any subscribers of that event.
        4. Here it should do the business logic of parsing the line and send it to the appropriate handlers (e.g., updating the game state, notifying the UI, etc.).
    */
    // Every class should override it
    public abstract void ProcessLine(string line);
}
