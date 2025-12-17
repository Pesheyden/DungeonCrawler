using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class BInGameConsole : Singleton<BInGameConsole>
{
    private UIDocument _consoleUIDocument;

    [SerializeField] private VisualTreeAsset _consoleAsset;
    [SerializeField] private VisualTreeAsset _consoleChatItemAsset;
    [SerializeField] private VisualTreeAsset _consoleSuggestionItemAsset;
    [SerializeField] private VisualTreeAsset _toolTipAsset;
    [SerializeField] private int _commandsHistoryLength = 10;


    [SerializeField] private List<string> _chatItems;
    [SerializeField] private List<string> _commandsHistory;
    [SerializeField] private List<ConsoleCommand> _suggestionsList;
    

    private VisualElement _consoleRoot;

    private TextField _inputTextField;
    
    private ListView _consoleChatListView;
    private ListView _consoleSuggestionsListView;

    
    [SerializeField] private int _selectedIndex;
    
    
    protected override void Awake()
    {
        base.Awake();
        Initialize();
        
        _selectedIndex = 0;
    }
    private void Initialize()
    {
        _chatItems = new List<string>();
        _commandsHistory = new List<string>();
        _suggestionsList = new List<ConsoleCommand>();
        
        
        _consoleUIDocument = GetComponent<UIDocument>();
        _consoleRoot = _consoleUIDocument.rootVisualElement;
        
        _consoleRoot.Add(_consoleAsset.CloneTree());

        _inputTextField = _consoleRoot.Q<TextField>("Input_TextField");
        _inputTextField.RegisterValueChangedCallback(OnInputTextFieldValueChange);
        _inputTextField.RegisterCallback<FocusEvent>(evt =>
        {
            int end = _inputTextField.value.Length;
            _inputTextField.cursorIndex = end;
            _inputTextField.selectIndex = end;
        });

        
        //Set up console chat view
        _consoleChatListView = _consoleRoot.Q<ListView>("Chat_ListView");
        _consoleChatListView.itemTemplate = _consoleChatItemAsset;
        _consoleChatListView.makeItem = () => _consoleChatListView.itemTemplate.CloneTree();
        _consoleChatListView.itemsSource = _chatItems;
        _consoleChatListView.reorderable = false;
        _consoleChatListView.selectionType = SelectionType.None;

        _consoleChatListView.bindItem += (element, i) =>
        {
            //Bind
            element.Q<Label>().text = _chatItems[i];
        };
        
        _consoleChatListView.Rebuild();
        
        
        //Set up suggestions view
        _consoleSuggestionsListView = _consoleRoot.Q<ListView>("Suggestions_ListView");
        _consoleSuggestionsListView.itemTemplate = _consoleSuggestionItemAsset;
        _consoleSuggestionsListView.itemsSource = _suggestionsList;
        _consoleSuggestionsListView.makeItem = () =>
        {
            var element = _consoleSuggestionsListView.itemTemplate.CloneTree();
            element.Q<Button>()?.AddManipulator(new ToolTipManipulator(_toolTipAsset));
            return element;
        };
        _consoleSuggestionsListView.reorderable = false;
        _consoleSuggestionsListView.selectionType = SelectionType.None;

        _consoleSuggestionsListView.bindItem += (element, i) =>
        {
            int index = i;

            var info = _suggestionsList[index];
            var inputTokens = _inputTextField.value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            bool showParams = inputTokens.Length > 0 && inputTokens[0].Equals(info.CallName, StringComparison.OrdinalIgnoreCase);
            
            
            var button = element.Q<Button>();
            
            button.text = showParams
                ? $"{info.CallName} {info.ParametersHint}"
                : info.CallName;
            button.tooltip = info.Description;

            if (button.userData is not null)
                button.UnregisterCallback(button.userData as EventCallback<ClickEvent>);

            EventCallback<ClickEvent> handler = evt =>
            {
                _inputTextField.value = _suggestionsList[index].CallName;
                _inputTextField.Focus();
            };
            button.RegisterCallback(handler);
            button.userData = handler;
        };
        _consoleSuggestionsListView.Rebuild();

        _consoleRoot.style.visibility = Visibility.Hidden;
    }
    
    private void Start()
    {
        InGameConsoleInputHandler.Instance.OnEnter += OnEnterPressed;
        InGameConsoleInputHandler.Instance.OnDirectionInput += OnDirectionInput;
        InGameConsoleInputHandler.Instance.OnOpen += OnOpen;
    }

    #region InputCallbacks

    private void OnOpen()
    {
        _consoleRoot.style.visibility = _consoleRoot.style.visibility == Visibility.Visible
            ? Visibility.Hidden
            : Visibility.Visible;
    }
    private void OnEnterPressed(bool value)
    {
        if (_selectedIndex > 0)
        {
            _inputTextField.value = _suggestionsList[_selectedIndex - 1].CallName;
            _selectedIndex = 0;
            _inputTextField.schedule.Execute(() => _inputTextField.Focus());
        }
        else
        {
            SendCommand();
        }
    }
    private void OnDirectionInput(int change)
    {
        // Update the selected suggestion index
        _selectedIndex -= change;

        // Ensure the index stays within bounds
        if (_selectedIndex > _suggestionsList.Count)
        {
            _selectedIndex = 1; // Wrap around to the first item
        }
        else if (_commandsHistory.Count > 0 && -_selectedIndex > _commandsHistory.Count)
        {
            _selectedIndex = -1;
        }
        else if(_commandsHistory.Count == 0 && _selectedIndex < 0)
        {
            _selectedIndex = 0;
        }

        // Execute logic
        if (_selectedIndex == 0)
        {
            _inputTextField.Focus();
            UpdateHighlight();
        }
        else if (_selectedIndex < 0)
        {
            _inputTextField.Focus();
            _inputTextField.value = _commandsHistory[-_selectedIndex - 1];
        }
        else
        {
            _inputTextField.Blur();
            UpdateHighlight();
        }
    }

    #endregion
    #region Suggestions
    private void UpdateHighlight()
    {
        // Clear previous highlights
        for (int i = 0; i < _consoleSuggestionsListView.itemsSource.Count; i++)
        {
            var element = _consoleSuggestionsListView.GetRootElementForIndex(i);
            if (element != null)
            {
                element.RemoveFromClassList("highlighted-item");
            }
        }

        // Add highlight to the selected suggestion
        var selectedElement = _consoleSuggestionsListView.GetRootElementForIndex(_selectedIndex - 1);
        if (selectedElement != null)
        {
            selectedElement.AddToClassList("highlighted-item");
        }
    }

    private void UpdateSuggestions(ChangeEvent<string> evt)
    {
        string input = evt.newValue;
        var tokens = input.Split(' ',StringSplitOptions.RemoveEmptyEntries).ToList();
        if(input.EndsWith(" "))
            tokens.Add(" ");

        _suggestionsList.Clear();

        var allCommands = ConsoleCommandRegistry.GetCommandInfos();

        if (tokens.Count >= 2)
        {
            var name = tokens[0];
            var arg = tokens.Count - 1;
            _suggestionsList.AddRange(
                allCommands
                    .Where(cmd => cmd.CallName.Contains(name, StringComparison.OrdinalIgnoreCase))
                    .Where(cmd => cmd.ParametersHint.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= arg)
                );
        }
        else if (tokens.Count == 1)
        {
            var name = tokens[0];

            // If the input exactly matches a command name → show all overloads
            var matchingCommands = allCommands
                .Where(cmd => cmd.CallName.Equals(name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchingCommands.Any())
            {
                _suggestionsList.AddRange(matchingCommands);
            }
            else
            {
                // Otherwise treat it as partial input → collapse duplicates
                _suggestionsList.AddRange(
                    allCommands
                        .Where(cmd => cmd.CallName.Contains(input, StringComparison.OrdinalIgnoreCase))
                        .GroupBy(cmd => cmd.CallName)
                        .Select(g => g.First())
                );
            }
        }

        else
        {
            _suggestionsList.AddRange(
                allCommands
                    .Where(cmd => cmd.CallName.Contains(input, StringComparison.OrdinalIgnoreCase))
                    .GroupBy(cmd => cmd.CallName)
                    .Select(g => g.First())
                    .ToList());
        }

        
        _consoleSuggestionsListView.Rebuild();

    }
    #endregion
    
    #region TextInputField
    private void SendCommand()
    {
        AddChatItem(_inputTextField.value);
        string[] tokens = _inputTextField.value.Split(' ');
        string commandName = tokens[0];
        string[] argStrings = tokens.Skip(1).ToArray();

        if (!ConsoleCommandRegistry.TryExecute(commandName, argStrings, out string executionMessage))
        {
            AddChatItem(executionMessage);
            return;
        }
        
        UpdateCommandsHistory(_inputTextField.value);
        _inputTextField.value = "";
    }

    private void UpdateCommandsHistory(string value)
    {
        _commandsHistory.Add(value);
        
        if(_commandsHistory.Count > _commandsHistoryLength)
            _commandsHistory.RemoveAt(0);
    }

    private void OnInputTextFieldValueChange(ChangeEvent<string> evt)
    {
        UpdateSuggestions(evt);
    }
    #endregion
    private void AddChatItem(string newItem)
    {
        _chatItems.Add(newItem);

        // Refresh so ListView knows about the new item
        _consoleChatListView.Rebuild();

        // Scroll to the last item
        _consoleChatListView.ScrollToItem(_chatItems.Count - 1);
    }

}
