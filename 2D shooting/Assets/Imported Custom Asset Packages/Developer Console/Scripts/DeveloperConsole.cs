using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Reflection;

public class DeveloperConsole : MonoBehaviour
{
    public static DeveloperConsole instance;

    [Header("Needed In Scene References")]
    public Text outputText;
    public InputField commandInputField;
    public EventSystem backUpEventSystem;
    public GameObject devLogCanvas;

    [Header("List of Console Commands")]
    public List<DeveloperConsoleCommand> developerConsoleCommands;

    private EventSystem eventSystem;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
        eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            eventSystem = backUpEventSystem;
            eventSystem.gameObject.SetActive(true);
        }
        devLogCanvas.SetActive(false);

        // Acquire all developer console commands in the assembly!
        developerConsoleCommands =  ReflectiveEnumerator.GetEnumerableOfType<DeveloperConsoleCommand>().ToList();
    }

    bool inputReleased = true;
    public void Update()
    {
        if ((Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed) && Keyboard.current.backquoteKey.isPressed && inputReleased)
        {
            ToggleDevConsole();
            inputReleased = false;
        }
        else if (!(Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed) || !Keyboard.current.backquoteKey.isPressed)
        {
            inputReleased = true;
        }
    }

    public void ToggleDevConsole()
    {
        outputText.text = "This is the Developer Console meant for use by Course Admin. \nIf you are not a TA or instructor you can put in commands if you want, " +
            "just don't edit any of the Console's code base since we might have to use it when grading your projects. \n\nUse command: |?| or |Help| to get a list of commands.";
        devLogCanvas.SetActive(!devLogCanvas.activeSelf);
        if (devLogCanvas.activeSelf)
        {
            eventSystem.SetSelectedGameObject(commandInputField.gameObject);
            commandInputField.text = "";
            commandInputField.ActivateInputField();
        }
    }

    private void OnEnable()
    {
        eventSystem.SetSelectedGameObject(commandInputField.gameObject);
    }

    private void OnDisable()
    {
        backUpEventSystem.gameObject.SetActive(false);
    }

    public void ParseCommand()
    {
        List<string> commandAsArray = commandInputField.text.Split().ToList();
        bool commandRun = false;
        if (commandAsArray[0] == "?" || commandAsArray[0] == "Help")
        {
            Help(commandAsArray);
            commandRun = true;
        }
        else
        {
            foreach(DeveloperConsoleCommand consoleCommand in developerConsoleCommands)
            {
                if (consoleCommand.CallMatchesThisCommand(commandAsArray))
                {
                    consoleCommand.RunCommand(this, commandAsArray);
                    commandRun = true;
                    break;
                }
            }
        }
        if (!commandRun) 
        {
            string invalidCommand = "";
            foreach(string commandPart in commandAsArray)
            {
                invalidCommand += "|" + commandPart + "| ";
            }
            outputText.text = "Invalid command: " + invalidCommand + "\n" +
                "To see a full list of commands use the |?| or |Help| commands" + "\n" +
                "Commands are entered without the vertical bars.";
        }
        eventSystem.SetSelectedGameObject(commandInputField.gameObject);
        commandInputField.text = "";
        commandInputField.ActivateInputField();
    }

    #region Help
    void Help(List<string> commandAsArray)
    {
        if ((commandAsArray[0] == "?" || commandAsArray[0] == "Help") && commandAsArray.Count == 1)
        {
            string helpOutput = "";
            helpOutput += OutputHelpHelp() + "\n";

            foreach(DeveloperConsoleCommand consoleCommand in developerConsoleCommands)
            {
                helpOutput += consoleCommand.OutputHelp() + "\n";
            }
            outputText.text = helpOutput;
        }
        else
        {
            outputText.text = OutputHelpFailure();
        }
    }

    string OutputHelpFailure()
    {
        string failureMessage = "Invalid Use of command |?| or |Help| \n\n";
        failureMessage += OutputHelpHelp();
        return failureMessage;
    }

    string OutputHelpHelp()
    {
        string helpMessage = "|?| or |Help|\n\n" +
            "To use |?| or |Help| order your command as: |?| or |Help|\n" +
            "This will list all the avaiable commands, what they do, and how to use them\n";
        return helpMessage;
    }
    #endregion
}
