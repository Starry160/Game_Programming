using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetHealthCommand : DeveloperConsoleCommand
{
    public override bool CallMatchesThisCommand(List<string> commandAsArray)
    {
        if (commandAsArray[0] == "SetHealth")
        {
            return true;
        }
        return false;
    }

    public override void RunCommand(DeveloperConsole developerConsole, List<string> commandAsArray)
    {
        if (commandAsArray[0] == "SetHealth" && commandAsArray.Count == 2)
        {
            string setHealthOutput = "Attempting to set player health to" + commandAsArray[1] + ": \n";
            int setHealthTo = -1;
            bool errorEncountered = false;
            try
            {
                int.TryParse(commandAsArray[1], out setHealthTo);
            }
            catch
            {
                setHealthOutput += "Second entry in command was not readable as an integer \n";
                setHealthOutput += OutputFailure();
                errorEncountered = true;
            }

            // Attempt to find the player in the scene and adjust their health value
            try
            {
                Health playerHealth = GameManager.instance.player.GetComponent<Health>();
                playerHealth.currentHealth = setHealthTo;
            }
            catch
            {
                setHealthOutput += "Error finding the health script or player in the scene, could not set health \n";
                errorEncountered = true;
            }
            if (!errorEncountered)
            {
                setHealthOutput += "No error encountered. Player health should now be changed. \n";
            }
            developerConsole.outputText.text = setHealthOutput;
        }
        else
        {
            developerConsole.outputText.text = OutputFailure();
        }
    }

    private string OutputFailure()
    {
        string failureMessage = "Invalid Use of command |SetHealth| \n\n";
        failureMessage += OutputHelp();
        return failureMessage;
    }

    public override string OutputHelp()
    {
        string helpMessage = "|SetHealth|\n\n" +
            "To use |SetHealth| order your command as: |SetHealth| |X| \n" +
            "Where X is an integer value \n" + 
            "This will set the player health to the provided value if possible \n";
        return helpMessage;
    }
}
