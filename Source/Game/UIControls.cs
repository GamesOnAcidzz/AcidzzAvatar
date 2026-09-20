using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.GUI;
using System.Linq;
using System.IO;
using FlaxEngine.Json;

namespace Game;

/// <summary>
/// UIControls Script.
/// </summary>
public class UIControls : Script
{
    public UIControl DanceButton;
    public UIControl DropdownCostumes;
    public UIControl SpinButton;
    public UIControl SnoopButton;
    public UIControl BreakDanceButton;
    public UIControl AngryButton;
    public UIControl SensitivitySlider;
    public UIControl SaveButton;
    private Button _danceButton;
    private Button _breakDanceButton;
    private Button _angryButton;
    private Button _spinButton;
    private Button _snoopButton;
    private Button _saveButton;
    private Dropdown _dropdownCostumes;
    private Slider _sensitivitySlider;
    public AnimatedModel AvatarModel;
    private AnimState _animState = AnimState.Idle;
    private AvatarSettings _avatarSettings;
    public Avatar AvatarScript;

    /// <inheritdoc/>
    public override void OnStart()
    {
        Screen.CursorLock = CursorLockMode.None;
        _danceButton = DanceButton.Control as Button;
        _breakDanceButton = BreakDanceButton.Control as Button;
        _angryButton = AngryButton.Control as Button;
        _spinButton = SpinButton.Control as Button;
        _snoopButton = SnoopButton.Control as Button;
        _saveButton = SaveButton.Control as Button;
        _dropdownCostumes = DropdownCostumes.Control as Dropdown;
        _sensitivitySlider = SensitivitySlider.Control as Slider;
        //Add costumes to dropdown
        Actor costumesActor = AvatarModel.GetChild(1);
        List<Actor> costumes = costumesActor.GetChildren<Actor>().ToList();
        List<String> costumesNames = new List<String>();
        foreach (Actor costume in costumes)
        {
            costumesNames.Add(costume.Name);
        }
        ;
        _dropdownCostumes.AddItems(costumesNames);

        _danceButton.Clicked += () =>
        {

            if (_animState == AnimState.Idle)
                _animState = AnimState.Emoting;
            else
                _animState = AnimState.Idle;
            AvatarModel.SetParameterValue("Pose", 0);
            AvatarModel.SetParameterValue("animState", _animState);
        };
        _snoopButton.Clicked += () =>
        {

            if (_animState == AnimState.Idle)
                _animState = AnimState.Emoting;
            else
                _animState = AnimState.Idle;
            AvatarModel.SetParameterValue("Pose", 1);
            AvatarModel.SetParameterValue("animState", _animState);
        };
        _breakDanceButton.Clicked += () =>
        {
            _animState = AnimState.SingleEmoting;
            AvatarModel.SetParameterValue("Pose", 0);
            AvatarModel.SetParameterValue("animState", _animState);
        };


        _angryButton.Clicked += () =>
        {
            _animState = AnimState.SingleEmoting;
            AvatarModel.SetParameterValue("Pose", 1);
            AvatarModel.SetParameterValue("animState", _animState);
        };

        _spinButton.Clicked += () =>
        {
            _animState = AnimState.SingleEmoting;
            AvatarModel.SetParameterValue("Pose", 2);
            AvatarModel.SetParameterValue("animState", _animState);
        };
        _saveButton.Clicked += () =>
        {
            SaveSettings();
        };
        _dropdownCostumes.SelectedItemChanged += () =>
        {
            foreach (Actor costume in costumes)
                costume.IsActive = false;
            costumes[_dropdownCostumes.SelectedIndex].IsActive = true;
        };
        _sensitivitySlider.ValueChanged += () =>
        {
            AvatarScript.Sensitivity = _sensitivitySlider.Value / 100;
        };
        LoadSettings();
        // Here you can add code that needs to be called when script is created, just before the first game update
    }

    /// <inheritdoc/>
    public override void OnEnable()
    {
        // Here you can add code that needs to be called when script is enabled (eg. register for events)
    }

    /// <inheritdoc/>
    public override void OnDisable()
    {
        // Here you can add code that needs to be called when script is disabled (eg. unregister from events)
    }

    /// <inheritdoc/>
    public override void OnUpdate()
    {
        // Here you can add code that needs to be called every frame
    }
    private void SaveSettings()
    {
        _avatarSettings.Sensitivity = _sensitivitySlider.Value;
        // _avatarSettings.Costume = _dropdownCostumes.SelectedIndex;
        String path = Path.Combine(Globals.ProductLocalFolder, "avatar_settings.json");
        String json = JsonSerializer.Serialize(_avatarSettings);
        File.WriteAllText(path, json);
        Debug.Log("Avatar Settings saved");
    }
    private void LoadSettings()
    {
        String path = Path.Combine(Globals.ProductLocalFolder, "avatar_settings.json");
        Directory.CreateDirectory(Globals.ProductLocalFolder);
        _avatarSettings = new AvatarSettings();
        if (!File.Exists(path))
        {
            String JsonSave = JsonSerializer.Serialize(_avatarSettings);
            File.WriteAllText(path, JsonSave);
            return;
        }
        String json = File.ReadAllText(path);
        JsonSerializer.Deserialize(_avatarSettings, json);
        _sensitivitySlider.Value = _avatarSettings.Sensitivity;
        //   _dropdownCostumes.SelectedIndex = _avatarSettings.Costume;
    }
}
