﻿namespace Melanchall.DryMidi
{
    internal static class MessageStatusBytes
    {
        internal static class Global
        {
            public const byte Meta        = 0xFF;
            public const byte NormalSysEx = 0xF0;
            public const byte EscapeSysEx = 0xF7;
        }

        internal static class Meta
        {
            public const byte SequenceNumber    = 0x00;
            public const byte Text              = 0x01;
            public const byte CopyrightNotice   = 0x02;
            public const byte SequenceTrackName = 0x03;
            public const byte InstrumentName    = 0x04;
            public const byte Lyric             = 0x05;
            public const byte Marker            = 0x06;
            public const byte CuePoint          = 0x07;
            public const byte ProgramName       = 0x08;
            public const byte DeviceName        = 0x09;
            public const byte ChannelPrefix     = 0x20;
            public const byte PortPrefix        = 0x21;
            public const byte EndOfTrack        = 0x2F;
            public const byte SetTempo          = 0x51;
            public const byte SmpteOffset       = 0x54;
            public const byte TimeSignature     = 0x58;
            public const byte KeySignature      = 0x59;
            public const byte SequencerSpecific = 0x7F;
        }

        internal static class Channel
        {
            public const byte NoteOff           = 0x8;
            public const byte NoteOn            = 0x9;
            public const byte NoteAftertouch    = 0xA;
            public const byte ControlChange     = 0xB;
            public const byte ProgramChange     = 0xC;
            public const byte ChannelAftertouch = 0xD;
            public const byte PitchBend         = 0xE;
        }
    }
}