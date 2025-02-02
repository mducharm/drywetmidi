---
uid: a_dev_output
---

# Output device

In DryWetMIDI an output MIDI device is represented by [IOutputDevice](xref:Melanchall.DryWetMidi.Multimedia.IOutputDevice) interface. It allows to send events to a MIDI device. To understand what an output MIDI device is in DryWetMIDI, please read [Overview](Overview.md) article.

The library provides built-in implementation of `IOutputDevice`: [OutputDevice](xref:Melanchall.DryWetMidi.Multimedia.OutputDevice). To get an instance of `OutputDevice` you can use either [GetByName](xref:Melanchall.DryWetMidi.Multimedia.OutputDevice.GetByName(System.String)) or [GetByIndex](xref:Melanchall.DryWetMidi.Multimedia.OutputDevice.GetByIndex(System.Int32)) static methods. To retrieve count of output MIDI devices presented in the system there is the [GetDevicesCount](xref:Melanchall.DryWetMidi.Multimedia.OutputDevice.GetDevicesCount) method. You can get all output MIDI devices with [GetAll](xref:Melanchall.DryWetMidi.Multimedia.OutputDevice.GetAll) method:

```csharp
using System;
using Melanchall.DryWetMidi.Multimedia;

// ...

foreach (var outputDevice in OutputDevice.GetAll())
{
    Console.WriteLine(outputDevice.Name);
}
```

> [!IMPORTANT]
> You can use `OutputDevice` built-in implementation of `IOutputDevice` on Windows and macOS only. Of course you can create your own implementation of `IOutputDevice` as described in [Custom output device](#custom-output-device) section below.

After an instance of `OutputDevice` is obtained, you can send MIDI events to device via [SendEvent](xref:Melanchall.DryWetMidi.Multimedia.OutputDevice.SendEvent(Melanchall.DryWetMidi.Core.MidiEvent)) method. You cannot send [meta events](xref:Melanchall.DryWetMidi.Core.MetaEvent) since such events can be inside a MIDI file only. If you pass an instance of meta event class, `SendEvent` will do nothing. [EventSent](xref:Melanchall.DryWetMidi.Multimedia.IOutputDevice.EventSent) event will be fired for each event sent with `SendEvent` (except meta events) holding the MIDI event sent. The value of [DeltaTime](xref:Melanchall.DryWetMidi.Core.MidiEvent.DeltaTime) property of MIDI events will be ignored, events will be sent to device immediately. To take delta-times into account, use [Playback](xref:Melanchall.DryWetMidi.Multimedia.Playback) class.

If you need to interrupt all currently sounding notes, call the [TurnAllNotesOff](xref:Melanchall.DryWetMidi.Multimedia.OutputDevice.TurnAllNotesOff) method which will send _Note Off_ events on all channels for all note numbers (kind of "panic" button on MIDI devices).

Small example that shows sending MIDI data:

```csharp
using System;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;

// ...

using (var outputDevice = OutputDevice.GetByName("Some MIDI device"))
{
    outputDevice.EventSent += OnEventSent;

    outputDevice.SendEvent(new NoteOnEvent());
    outputDevice.SendEvent(new NoteOffEvent());
}

// ...

private void OnEventSent(object sender, MidiEventSentEventArgs e)
{
    var midiDevice = (MidiDevice)sender;
    Console.WriteLine($"Event sent to '{midiDevice.Name}' at {DateTime.Now}: {e.Event}");
}
```

> [!IMPORTANT]
> You should always take care about disposing an `OutputDevice`, so use it inside `using` block or call `Dispose` manually. Without it all resources taken by the device will live until GC collect them via finalizer of the `OutputDevice`. It means that sometimes you will not be able to use different instances of the same device across multiple applications or different pieces of a program.

First call of `SendEvent` method can take some time for allocating resources for device, so if you want to eliminate this operation on sending a MIDI event, you can call [PrepareForEventsSending](xref:Melanchall.DryWetMidi.Multimedia.IOutputDevice.PrepareForEventsSending) method before any MIDI event will be sent.

## Custom output device

You can create your own output device implementation and use it in your app. For example, let's create super simple device that just outputs MIDI events to console:

```csharp
private sealed class ConsoleOutputDevice : IOutputDevice
{
    public event EventHandler<MidiEventSentEventArgs> EventSent;

    public void PrepareForEventsSending()
    {
    }

    public void SendEvent(MidiEvent midiEvent)
    {
        Console.WriteLine(midiEvent);
    }

    public void Dispose()
    {
    }
}
```

You can then use this device, for example, for debug in [Playback](xref:Melanchall.DryWetMidi.Multimedia.Playback).

Another one use case for custom output device is plugging some synth. So you create output device where [SendEvent](xref:Melanchall.DryWetMidi.Multimedia.IOutputDevice.SendEvent(Melanchall.DryWetMidi.Core.MidiEvent)) will redirect MIDI events to synth.