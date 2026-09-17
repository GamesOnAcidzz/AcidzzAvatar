using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FlaxEngine;
using System.Linq;
using PortAudioSharp;

namespace Game;

/// <summary>
/// Avatar Script.
/// </summary>
public class Avatar : Script
{
    private AnimGraphParameter _isEmoting;
    private AnimGraphParameter _micVolume;
    private AnimGraphParameter _pose;
    private List<Actor> _mouths;
    private PortAudioSharp.Stream _stream;
    public float Level { get; private set; }
    public float Decibels { get; private set; }
    [EditorDisplay("Settings")]
    public float Sensitivity = 1.2f;
    private float _levelProcessed = 0.0f;
    private float _timer;
    private long _callbackCount;
    private long _lastCallbackCount;
    private StreamCallbackFlags _lastStatusFlags;

    /// <inheritdoc/>
    public override void OnStart()
    {
        _mouths = Actor.GetChild(0).GetChild(0).GetChildren<Actor>().ToList();
        _isEmoting = Actor.As<AnimatedModel>().GetParameter("isEmoting");
        _micVolume = Actor.As<AnimatedModel>().GetParameter("micVolume");
        _pose = Actor.As<AnimatedModel>().GetParameter("Pose");

        PortAudio.Initialize();
        int deviceIndex = PortAudio.DefaultInputDevice;
        if (deviceIndex == PortAudio.NoDevice)
        {
            Debug.LogError("No Microphone found");
        }
        DeviceInfo info = PortAudio.GetDeviceInfo(deviceIndex);

        Debug.Log($"Microphone: {info.name}");
        Debug.Log($"Device index: {deviceIndex}");
        Debug.Log($"Max input channels: {info.maxInputChannels}");
        Debug.Log($"Max output channels: {info.maxOutputChannels}");
        Debug.Log($"Default sample rate: {info.defaultSampleRate}");
        Debug.Log($"Low input latency: {info.defaultLowInputLatency}");
        Debug.Log($"High input latency: {info.defaultHighInputLatency}");

        var parameters = new StreamParameters
        {
            device = deviceIndex,
            channelCount = 1,
            sampleFormat = SampleFormat.Float32,
            suggestedLatency = info.defaultLowInputLatency,
            hostApiSpecificStreamInfo = IntPtr.Zero
        };

        PortAudioSharp.Stream.Callback callback =
            (IntPtr input,
             IntPtr output,
             UInt32 frameCount,
             ref StreamCallbackTimeInfo timeInfo,
             StreamCallbackFlags statusFlags,
             IntPtr userData) =>
            {
                if (input == IntPtr.Zero || frameCount == 0)
                    return StreamCallbackResult.Continue;

                int count = (int)frameCount;

                float sum = 0.0f;

                unsafe
                {
                    float* samples = (float*)input;

                    for (int i = 0; i < count; i++)
                    {
                        float sample = samples[i];
                        sum += sample * sample;
                    }
                }

                float rms = MathF.Sqrt(sum / count);

                float db = 20.0f * MathF.Log10(
                    MathF.Max(rms, 0.00001f)
                );

                float level = Math.Clamp(
                    (db + 60.0f) / 60.0f,
                    0.0f,
                    1.0f
                );

                Level = level;
                Decibels = db;

                if (statusFlags != 0)
                {
                    _lastStatusFlags = statusFlags;
                }
                _callbackCount++;

                return StreamCallbackResult.Continue;
            };
        try
        {
            _stream = new PortAudioSharp.Stream(
                inParams: parameters,
                outParams: null,
                sampleRate: info.defaultSampleRate,
                framesPerBuffer: 2048,
                streamFlags: StreamFlags.ClipOff,
                callback: callback,
                userData: IntPtr.Zero
            );
        }
        catch (PortAudioException ex)
        {
            Debug.LogError($"PortAudio error code: {ex.ErrorCode}");
            Debug.LogError($"PortAudio error: {PortAudio.GetErrorText(ex.ErrorCode)}");
        }
        _stream.Start();

        Debug.Log("Microphone started.");
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

    public override void OnDestroy()
    {
        if (_stream != null)
        {
            _stream.Stop();
            _stream.Dispose();
            _stream = null;
        }

        PortAudio.Terminate();
    }

    /// <inheritdoc/>
    public override void OnUpdate()
    {
        _levelProcessed = Level * Sensitivity;
        _micVolume.Value = Mathf.Lerp(_levelProcessed, 0.0f, Time.DeltaTime / 1) * 100;
        toggleMouths();
        _timer += Time.DeltaTime;

        _timer += Time.DeltaTime;

        if (_timer >= 1.0f)
        {
            _timer -= 1.0f;

            long callbacksThisSecond =
                _callbackCount - _lastCallbackCount;

            _lastCallbackCount = _callbackCount;

            Debug.Log(
                $"Audio callbacks/sec: {callbacksThisSecond}, " +
                $"status: {_lastStatusFlags}, " +
                $"level: {Level:F3}, " +
                $"dB: {Decibels:F1}"
            );

            _lastStatusFlags = 0;
        }
        // Here you can add code that needs to be called every frame
    }
    private void toggleMouths()
    {
        _mouths.ForEach(mouth => mouth.IsActive = false);
        if (_levelProcessed < 0.4)
        {
            _mouths[0].IsActive = true;
        }
        if (_levelProcessed > 0.4 && _levelProcessed < 0.6)
        {
            _mouths[1].IsActive = true;
        }
        if (_levelProcessed > 0.6 && _levelProcessed < 0.8)
        {
            _mouths[2].IsActive = true;
        }
        if (_levelProcessed > 0.8)
        {
            _mouths[3].IsActive = true;
        }
    }
}
