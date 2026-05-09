using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LoadSceneCommand : DeveloperConsoleCommand
{
    public override bool CallMatchesThisCommand(List<string> commandAsArray)
    {
        if (commandAsArray[0] == "LoadScene")
        {
            return true;
        }
        return false;
    }

    public override void RunCommand(DeveloperConsole developerConsole, List<string> commandAsArray)
    {
        int indexOfSceneToLoad = -1;

        int.TryParse(commandAsArray[1], out indexOfSceneToLoad);

        try
        {
            SceneManager.LoadScene(indexOfSceneToLoad);
        }
        catch
        {
            developerConsole.outputText.text = OutputFailure(indexOfSceneToLoad);
        }
    }

    private string OutputFailure(int sceneIndex)
    {
        string failureMessage = "Error loading scene: | " + sceneIndex + " |" + "\n\n";
        failureMessage += OutputHelp();
        return failureMessage;
    }

    public override string OutputHelp()
    {
        string helpMessage = "|LoadScene| \n\nTo use |LoadScene| order your command as: |LoadScene| |SceneIndex| \n";
        helpMessage += "Without the vertical bars and where SceneIndex is the index of the scene in the build settings you wish to load." +
            "\nThis will load the scene with the given index if one exists.\n" +
            "\nTo get a list of scenes in the build use the command: |ListScenes|\n";
        return helpMessage;
    }
}
