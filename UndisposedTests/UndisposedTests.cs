// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)

using System.Text;
using NUnit.Framework;
using System;
using Undisposed;
using System.IO;

namespace UndisposedTests
{
	[TestFixture]
	public class UndisposedTests
	{
		private ModuleWeaverTestHelper _moduleWeaverTestHelper;
		private StringWriter _consoleOutput;

		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			_moduleWeaverTestHelper = new ModuleWeaverTestHelper(
				"../../../AssemblyToProcess/bin/Debug/AssemblyToProcess.dll");
			_consoleOutput = new StringWriter();
			Console.SetOut(_consoleOutput);
		}

		[SetUp]
		public void SetUp()
		{
			_consoleOutput.GetStringBuilder().Clear();
		}

		[Test]
		public void TurnedOff()
		{
			SetOutputKind(TrackerOutputKind.None);
			var instance = GetInstance("AllObjectsDisposed");
			instance.Dispose();
			var output = _consoleOutput.ToString();
			Assert.AreEqual(@"Created AllObjectsDisposed
Disposed AllObjectsDisposed
", output);
		}

		[Test]
		public void AllDisposed()
		{
			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var instance = GetInstance("AllObjectsDisposed");
			instance.Dispose();
			var output = _consoleOutput.ToString();
			Assert.AreEqual(@"Created AllObjectsDisposed
*** Creating AssemblyToProcess.AllObjectsDisposed 1
*** Disposing AssemblyToProcess.AllObjectsDisposed 1
**** Undisposed Object Dump:
Disposed AllObjectsDisposed
", output);
		}

		[Test]
		public void AllDisposedDumpOnly()
		{
			SetOutputKind(TrackerOutputKind.Dump);
			var instance = GetInstance("AllObjectsDisposed");
			instance.Dispose();
			var output = _consoleOutput.ToString();
			Assert.AreEqual(@"Created AllObjectsDisposed
**** Undisposed Object Dump:
Disposed AllObjectsDisposed
", output);
		}

		[Test]
		public void AllDisposedRegistrationOnly()
		{
			SetOutputKind(TrackerOutputKind.Registration);
			var instance = GetInstance("AllObjectsDisposed");
			instance.Dispose();
			var output = _consoleOutput.ToString();
			Assert.AreEqual(@"Created AllObjectsDisposed
*** Creating AssemblyToProcess.AllObjectsDisposed 1
*** Disposing AssemblyToProcess.AllObjectsDisposed 1
Disposed AllObjectsDisposed
", output);
		}

		[Test]
		public void NotDisposed()
		{
			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var instance = GetInstance("ObjectNotDisposed");
			instance.Dispose();
			var output = _consoleOutput.ToString();
			Assert.AreEqual(@"Created AllObjectsDisposed
*** Creating AssemblyToProcess.AllObjectsDisposed 1
*** Creating AssemblyToProcess.ObjectNotDisposed 1
*** Disposing AssemblyToProcess.ObjectNotDisposed 1
**** Undisposed Object Dump:
" + "\t" + @"AssemblyToProcess.AllObjectsDisposed: 1
", output);
		}

		[Test]
		public void TwoConstructorsCallingEachOther()
		{
			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var instance = GetInstance("TwoConstructorsCallingEachOther");
			instance.Dispose();
			var output = _consoleOutput.ToString();
			Assert.AreEqual(@"*** Creating AssemblyToProcess.TwoConstructorsCallingEachOther 1
*** Disposing AssemblyToProcess.TwoConstructorsCallingEachOther 1
**** Undisposed Object Dump:
", output);
		}

		[Test]
		public void TwoIndependentConstructors()
		{
			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var instance = GetInstance("TwoIndependentConstructors");
			instance.Dispose();
			var output = _consoleOutput.ToString();
			Assert.AreEqual(@"*** Creating AssemblyToProcess.TwoIndependentConstructors 1
*** Disposing AssemblyToProcess.TwoIndependentConstructors 1
**** Undisposed Object Dump:
", output);
		}

		[Test]
		public void DerivedClass()
		{
			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var tic = GetInstance("TwoIndependentConstructors");
			var instance = GetInstance("DerivedClass");
			instance.Dispose();
			tic.Dispose();
			var output = _consoleOutput.ToString();
			Assert.AreEqual(@"*** Creating AssemblyToProcess.TwoIndependentConstructors 1
Created AllObjectsDisposed
*** Creating AssemblyToProcess.DerivedClass 1
*** Disposing AssemblyToProcess.DerivedClass 1
**** Undisposed Object Dump:
" + "\t" + @"AssemblyToProcess.TwoIndependentConstructors: 1
Disposed AllObjectsDisposed
*** Disposing AssemblyToProcess.TwoIndependentConstructors 1
**** Undisposed Object Dump:
", output);
		}

		[Test]
		public void NoDefaultCtor()
		{
			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var instance = GetInstance("NoDefaultCtor", string.Empty);
			instance.Dispose();
			var output = _consoleOutput.ToString();
			Assert.AreEqual(@"*** Creating AssemblyToProcess.NoDefaultCtor 1
*** Disposing AssemblyToProcess.NoDefaultCtor 1
**** Undisposed Object Dump:
", output);
		}

		[Test]
		public void DerivedClassImplementsIDisposable()
		{
			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var instance = GetInstance("DerivedClassImplementsIDisposable");
			instance.Dispose();
			var output = _consoleOutput.ToString();
			Assert.AreEqual(@"*** Creating AssemblyToProcess.DerivedClassImplementsIDisposable 1
*** Disposing AssemblyToProcess.DerivedClassImplementsIDisposable 1
**** Undisposed Object Dump:
", output);
		}

		[Test]
		public void DerivedFromExternalClass()
		{
			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var instance = GetInstance("DerivedFromExternalClass");
			instance.Dispose();
			var output = _consoleOutput.ToString();
			Assert.AreEqual(@"*** Creating AssemblyToProcess.DerivedFromExternalClass 1
*** Disposing AssemblyToProcess.DerivedFromExternalClass 1
**** Undisposed Object Dump:
", output);
		}

		[Test]
		public void DerivedFromInternalClass()
		{
			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var instance = GetInstance("DerivedFromInternalClass");
			instance.Dispose();
			var output = _consoleOutput.ToString();
			Assert.AreEqual(@"*** Creating AssemblyToProcess.DerivedFromInternalClass 1
*** Disposing AssemblyToProcess.DerivedFromInternalClass 1
**** Undisposed Object Dump:
", output);
		}

		[Test]
		public void RedirectOutput()
		{
			var oldOutput = DisposeTracker.LogWriter;

			var writer = new StringBuilder();
			DisposeTracker.LogWriter = s => writer.AppendLine(s);

			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var instance = GetInstance("AllObjectsDisposed");
			instance.Dispose();
			var output = writer.ToString();
			Assert.AreEqual(@"*** Creating AssemblyToProcess.AllObjectsDisposed 1
*** Disposing AssemblyToProcess.AllObjectsDisposed 1
**** Undisposed Object Dump:
", output);

			DisposeTracker.LogWriter = oldOutput;
		}

		private static void SetOutputKind(TrackerOutputKind outputKind)
		{
			DisposeTracker.Reset();
			DisposeTracker.OutputKind = outputKind;
		}

		private dynamic GetInstance(string className, params object[] args)
		{
			var type = _moduleWeaverTestHelper.Assembly.GetType(
				              "AssemblyToProcess." + className, true);
			return Activator.CreateInstance(type, args);
		}
	}
}
