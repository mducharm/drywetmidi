﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed partial class InputDeviceTests
    {
        #region Test methods

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_SingleEventWithStatusByte() => ReceiveData(
            data: new byte[] { 0x90, 0x75, 0x56 },
            indices: new[] { 0 },
            expectedEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)0x75, (SevenBitNumber)0x56)
            });

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_MultipleEventsWithStatusBytes() => ReceiveData(
            data: new byte[] { 0x90, 0x75, 0x56, 0x80, 0x55, 0x65, 0x90, 0x75, 0x56 },
            indices: new[] { 0, 3, 6 },
            expectedEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)0x75, (SevenBitNumber)0x56),
                new NoteOffEvent((SevenBitNumber)0x55, (SevenBitNumber)0x65),
                new NoteOnEvent((SevenBitNumber)0x75, (SevenBitNumber)0x56),
            });

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_MultipleEventsWithRunningStatus() => ReceiveData(
            data: new byte[] { 0x90, 0x15, 0x56, 0x55, 0x65, 0x45, 0x60 },
            indices: new[] { 0 },
            expectedEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)0x15, (SevenBitNumber)0x56),
                new NoteOnEvent((SevenBitNumber)0x55, (SevenBitNumber)0x65),
                new NoteOnEvent((SevenBitNumber)0x45, (SevenBitNumber)0x60),
            });

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_LotOfEventsWithStatusBytes()
        {
            const int eventsCount = 3333;

            ReceiveData(
                data: Enumerable
                    .Range(0, eventsCount)
                    .SelectMany(i => new byte[] { 0x90, 0x75, 0x56 })
                    .ToArray(),
                indices: Enumerable
                    .Range(0, eventsCount)
                    .Select(i => i * 3)
                    .ToArray(),
                expectedEvents: Enumerable
                    .Range(0, eventsCount)
                    .Select(i => new NoteOnEvent((SevenBitNumber)0x75, (SevenBitNumber)0x56))
                    .ToArray());
        }

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_UnexpectedRunningStatus()
        {
            var deviceName = MidiDevicesNames.DeviceA;
            var deviceNamePtr = Marshal.StringToHGlobalAnsi(deviceName);

            var data = new byte[] { 0x56, 0x67, 0x45 };
            var indices = new[] { 0 };

            using (var inputDevice = InputDevice.GetByName(deviceName))
            {
                Exception exception = null;

                inputDevice.ErrorOccurred += (_, e) => exception = e.Exception;
                inputDevice.StartEventsListening();

                SendData(deviceNamePtr, data, data.Length, indices, indices.Length);

                var timeout = SendReceiveUtilities.MaximumEventSendReceiveDelay;
                var errorOccurred = WaitOperations.Wait(() => exception != null, timeout);
                Assert.IsTrue(errorOccurred, $"Error was not occurred for [{timeout}].");
                Assert.IsInstanceOf(typeof(MidiDeviceException), exception, "Exception type is invalid");
                Assert.IsInstanceOf(typeof(UnexpectedRunningStatusException), exception.InnerException, "Inner exception type is invalid.");
            }
        }

        [Test]
        [Platform("MacOsX")]
        public void GetInputDeviceSupportedProperties_Mac()
        {
            CollectionAssert.AreEquivalent(
                new[]
                {
                    InputDeviceProperty.Product,
                    InputDeviceProperty.Manufacturer,
                    InputDeviceProperty.DriverVersion,
                    InputDeviceProperty.UniqueId,
                    InputDeviceProperty.DriverOwner,
                },
                InputDevice.GetSupportedProperties(),
                "Invalid collection of supported properties.");
        }

        [Test]
        [Platform("MacOsX")]
        public void GetInputDeviceProperty_Product_Mac()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual("InputProduct", inputDevice.GetProperty(InputDeviceProperty.Product), "Product is invalid.");
        }

        [Test]
        [Platform("MacOsX")]
        public void GetInputDeviceProperty_Manufacturer_Mac()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual("InputManufacturer", inputDevice.GetProperty(InputDeviceProperty.Manufacturer), "Manufacturer is invalid.");
        }

        [Test]
        [Platform("MacOsX")]
        public void GetInputDeviceProperty_DriverVersion_Mac()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual(100, inputDevice.GetProperty(InputDeviceProperty.DriverVersion), "Driver version is invalid.");
        }

        [Test]
        [Platform("MacOsX")]
        public void GetInputDeviceProperty_UniqueId_Mac()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.IsNotNull(inputDevice.GetProperty(InputDeviceProperty.UniqueId), "Device unique ID is null.");
        }

        [Test]
        [Platform("MacOsX")]
        public void GetInputDeviceProperty_DriverOwner_Mac()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual("InputDriverOwner", inputDevice.GetProperty(InputDeviceProperty.DriverOwner), "Driver owner is invalid.");
        }

        [Test]
        [Platform("MacOsX")]
        public void CheckInputDevicesEquality_ViaEquals_SameDevices_Mac()
        {
            var inputDevice1 = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            var inputDevice2 = InputDevice.GetByName(MidiDevicesNames.DeviceA);

            Assert.AreEqual(inputDevice1, inputDevice2, "Devices are not equal.");
        }

        [Test]
        [Platform("MacOsX")]
        public void CheckInputDevicesEquality_ViaEquals_DifferentDevices_Mac()
        {
            var inputDevice1 = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            var inputDevice2 = InputDevice.GetByName(MidiDevicesNames.DeviceB);

            Assert.AreNotEqual(inputDevice1, inputDevice2, "Devices are equal.");
        }

        [Test]
        [Platform("MacOsX")]
        public void CheckInputDevicesEquality_ViaOperator_SameDevices_Mac()
        {
            var inputDevice1 = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            var inputDevice2 = InputDevice.GetByName(MidiDevicesNames.DeviceA);

            Assert.IsTrue(inputDevice1 == inputDevice2, "Devices are not equal via equality.");
            Assert.IsFalse(inputDevice1 != inputDevice2, "Devices are not equal via inequality.");
        }

        [Test]
        [Platform("MacOsX")]
        public void CheckInputDevicesEquality_ViaOperator_DifferentDevices_Mac()
        {
            var inputDevice1 = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            var inputDevice2 = InputDevice.GetByName(MidiDevicesNames.DeviceB);

            Assert.IsFalse(inputDevice1 == inputDevice2, "Devices are equal via equality.");
            Assert.IsTrue(inputDevice1 != inputDevice2, "Devices are equal via inequality.");
        }

        #endregion
    }
}
