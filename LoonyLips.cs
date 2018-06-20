using Godot;
using System;
using System.Collections.Generic;

struct Story
{
    public List<String> prompts;
    public String story;
}

public class LoonyLips : Node2D
{
    // private instance variables for state
    Story currentStory;
    Dictionary<string, string> strings = new Dictionary<string, string>();
    List<String> playerWords = new List<string>();

    // cached references for readability
    RichTextLabel buttonLabel;
    RichTextLabel storyText;
    LineEdit textEntryBox;

    // messages, then public methods, then private methods...
    public override void _Ready()
    {
        CacheComponents();
        ShowIntro();
        SetRandomStory();
        PromptPlayer();
    }

    private void CacheComponents()
    {
        // for more refactorability use "if c extends DesiredClass" or Groups
        buttonLabel = FindNode("ButtonLabel") as RichTextLabel;
        storyText = FindNode("StoryText") as RichTextLabel;
        textEntryBox = FindNode("TextBox") as LineEdit;
    }

    public void OnButtonPressed()
    {
        if (IsStoryDone())  // Button is now play again
        {
            GetTree().ReloadCurrentScene();
        }
        else
        {
            var userInput = textEntryBox.GetText();
            OnTextEntry(userInput);
        }
    }

    public void OnTextEntry(string entry)  // Note no need to bind in Signals
    {
        playerWords.Add(entry);
        textEntryBox.SetText("");
        storyText.SetText("");
        if (IsStoryDone())
        {
            TellStory();
        }
        else
        {
            PromptPlayer();
        }
    }

    private void ShowIntro()
    {
        SetStringsFromFile("other_strings.json");
        storyText.Text = strings["intro_text"];
    }

    private void PromptPlayer()
    {
        string nextPrompt = currentStory.prompts[playerWords.Count];
        storyText.Text += String.Format(strings["prompt"], nextPrompt); 
    }

    private void TellStory()
    {
        storyText.Text = String.Format(currentStory.story, playerWords.ToArray());
        buttonLabel.SetText(strings["again"]);
        EndGame();
    }


    private bool IsStoryDone()
    {
        return playerWords.Count == currentStory.prompts.Count;
    }

    private JSONParseResult GetJSONParseResult(string localFileName)
    {
        var file = new File();
        file.Open(localFileName, 1);  // Mode 1 is read only
        string text = file.GetAsText();
        file.Close();

        var parseResult = JSON.Parse(text);

        if (parseResult.Error != 0)
        {
            GD.Print(localFileName + " parse error");
            return null;
        }
        else
        {
            GD.Print(localFileName + " read OK");
            return parseResult;
        }
    }

    private void SetStringsFromFile(string localFileName)
    {
        var parseResult = GetJSONParseResult(localFileName);
        var dict = parseResult.Result as Dictionary<System.Object, System.Object>;

        foreach (var entry in dict)
        {
            var keyString = entry.Key as string;
            strings[keyString] = dict[keyString] as string;
        }
    }

    private void SetRandomStory()
    {
        var parseResult = GetJSONParseResult("stories.json");
        var stories = parseResult.Result as Array;
        
        Random rnd = new Random();
        int storyIndex = rnd.Next(0, stories.Length);
        var randomStory = stories.GetValue(storyIndex) as Dictionary<System.Object, System.Object>;
        
        var tempPrompts = new List<string>();
        var prompts = randomStory["prompt"] as Array;
        foreach (System.Object o in prompts)
        {
            tempPrompts.Add(o as string);
            GD.Print(o as string);
        }

        currentStory.prompts = tempPrompts;
        currentStory.story = "Once upon a time {0} ate a {1} and felt very {2}. It was a {3} day for all good {4}.";
    }

    private void EndGame()
    {
        textEntryBox.QueueFree();  // Remove text box
    }
}
