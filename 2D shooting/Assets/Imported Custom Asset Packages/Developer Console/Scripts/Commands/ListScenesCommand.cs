using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ListScenesCommand : DeveloperConsoleCommand
{
    public override bool CallMatchesThisCommand(List<string> commandAsArray)
    {
        if (commandAsArray[0] == "ListScenes")
        {
            return true;
        }
        return false;
    }

    public override void RunCommand(DeveloperConsole developerConsole, List<string> commandAsArray)
    {
        if (commandAsArray[0] == "ListScenes" && commandAsArray.Count == 1)
        {
            string listScenesOutput = "All Scenes in build: \n";
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCountInBuildSettings; sceneIndex++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
                string sceneName = path.Substring(0, path.Length - 6).Substring(path.LastIndexOf('/') + 1);
                listScenesOutput += sceneIndex + " | " + sceneName + "\n";
            }
            developerConsole.outputText.text = listScenesOutput;
        }
        else
        {
            developerConsole.outputText.text = OutputFailure();
        }
    }

    private string OutputFailure()
    {
        string failureMessage = "Invalid Use of command |ListScenes| \n\n";
        failureMessage += OutputHelp();
        return failureMessage;
    }

    public override string OutputHelp()
    {
        string helpMessage = "|ListScenes|\n\n" +
            "To use |ListScenes| order your command as: |ListScenes| \n" +
            "This will list out all scenes available in the build\n";
        return helpMessage;
    }
}
