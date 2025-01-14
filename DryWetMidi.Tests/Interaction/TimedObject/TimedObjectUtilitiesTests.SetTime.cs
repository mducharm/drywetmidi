﻿using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using System;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimedObjectUtilitiesTests
    {
        #region Constants

        private static readonly ObjectsFactory Factory = ObjectsFactory.Default;

        #endregion

        #region Test methods

        [Test]
        public void SetTime_TimedEvent_Midi([Values(0, 100)] long time)
        {
            var timedEvent = Factory.GetTimedEvent("100");

            // TODO: use extension syntax after OBS14 removed
            var result = TimedObjectUtilities.SetTime(timedEvent, (MidiTimeSpan)time, Factory.TempoMap);

            Assert.AreSame(timedEvent, result, "Result is not the same object.");
            Assert.AreEqual(time, result.Time, "Invalid time.");
        }

        [Test]
        public void SetTime_Note_Midi([Values(0, 100)] long time)
        {
            var note = Factory.GetNote("100", "50");

            // TODO: use extension syntax after OBS14 removed
            var result = TimedObjectUtilities.SetTime(note, (MidiTimeSpan)time, Factory.TempoMap);

            Assert.AreSame(note, result, "Result is not the same object.");
            Assert.AreEqual(time, result.Time, "Invalid time.");
            Assert.AreEqual(50, result.Length, "Invalid length.");
        }

        [Test]
        public void SetTime_Chord_Midi([Values(0, 100)] long time)
        {
            var chord = Factory.GetChord(
                "100", "50",
                "110", "40");

            // TODO: use extension syntax after OBS14 removed
            var result = TimedObjectUtilities.SetTime(chord, (MidiTimeSpan)time, Factory.TempoMap);

            Assert.AreSame(chord, result, "Result is not the same object.");
            Assert.AreEqual(time, result.Time, "Invalid time.");
            Assert.AreEqual(50, result.Length, "Invalid length.");
        }

        [Test]
        public void SetTime_TimedEvent_Metric([Values(0, 250000)] int ms)
        {
            var timedEvent = Factory.GetTimedEvent("100");

            // TODO: use extension syntax after OBS14 removed
            var result = TimedObjectUtilities.SetTime(timedEvent, new MetricTimeSpan(0, 0, 0, ms), Factory.TempoMap);

            Assert.AreSame(timedEvent, result, "Result is not the same object.");
            Assert.AreEqual(
                new MetricTimeSpan(0, 0, 0, ms),
                result.TimeAs<MetricTimeSpan>(Factory.TempoMap),
                "Invalid time.");
        }

        [Test]
        public void SetTime_NullObject()
        {
            var timedEvent = default(TimedEvent);

            // TODO: use extension syntax after OBS14 removed
            Assert.Throws<ArgumentNullException>(() => TimedObjectUtilities.SetTime(timedEvent, new MidiTimeSpan(), Factory.TempoMap));
        }

        [Test]
        public void SetTime_NullTime()
        {
            var timedEvent = Factory.GetTimedEvent("10");

            // TODO: use extension syntax after OBS14 removed
            Assert.Throws<ArgumentNullException>(() => TimedObjectUtilities.SetTime(timedEvent, null, Factory.TempoMap));
        }

        [Test]
        public void SetTime_NullTempoMap()
        {
            var timedEvent = Factory.GetTimedEvent("10");

            // TODO: use extension syntax after OBS14 removed
            Assert.Throws<ArgumentNullException>(() => TimedObjectUtilities.SetTime(timedEvent, new MidiTimeSpan(), null));
        }

        #endregion
    }
}
