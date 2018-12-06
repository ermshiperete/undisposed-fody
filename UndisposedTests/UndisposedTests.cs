// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)

using System.Text;
using NUnit.Framework;
using System;
using Undisposed;
using System.IO;
using System.Reflection;

namespace UndisposedTests
{
	[TestFixture]
	public class UndisposedTests
	{
		private ModuleWeaverTestHelper _moduleWeaverTestHelper;
		private StringWriter _consoleOutput;
		private Action<string> _originalLogWriter;

		[OneTimeSetUp]
		public void FixtureSetUp()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var directoryThisAssembly = Path.GetDirectoryName(assembly.Location);
			_moduleWeaverTestHelper = new ModuleWeaverTestHelper(Path.Combine(directoryThisAssembly,
				"../../../AssemblyToProcess/bin/Debug/AssemblyToProcess.dll"));
			_consoleOutput = new StringWriter();
			Console.SetOut(_consoleOutput);
			_originalLogWriter = DisposeTracker.LogWriter;
		}

		[SetUp]
		public void SetUp()
		{
			_consoleOutput.GetStringBuilder().Clear();
		}

		[TearDown]
		public void TearDown()
		{
			DisposeTracker.LogWriter = _originalLogWriter;
		}

		[Test]
		public void TurnedOff()
		{
			SetOutputKind(TrackerOutputKind.None);
			var instance = GetInstance("AllObjectsDisposed");
			instance.Dispose();
			Assert.That(_consoleOutput.ToString(),
				Is.EqualTo(@"Created AllObjectsDisposed
Disposed AllObjectsDisposed
"));
		}

		[Test]
		public void AllDisposed()
		{
			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var instance = GetInstance("AllObjectsDisposed");
			instance.Dispose();
			Assert.That(_consoleOutput.ToString(),
				Is.EqualTo(@"Created AllObjectsDisposed
*** Creating AssemblyToProcess.AllObjectsDisposed 1
*** Disposing AssemblyToProcess.AllObjectsDisposed 1
**** Undisposed Object Dump:
Disposed AllObjectsDisposed
"));
		}

		[Test]
		public void AllDisposedDumpOnly()
		{
			SetOutputKind(TrackerOutputKind.Dump);
			var instance = GetInstance("AllObjectsDisposed");
			instance.Dispose();
			Assert.That(_consoleOutput.ToString(),
				Is.EqualTo(@"Created AllObjectsDisposed
**** Undisposed Object Dump:
Disposed AllObjectsDisposed
"));
		}

		[Test]
		public void AllDisposedRegistrationOnly()
		{
			SetOutputKind(TrackerOutputKind.Registration);
			var instance = GetInstance("AllObjectsDisposed");
			instance.Dispose();
			Assert.That(_consoleOutput.ToString(),
				Is.EqualTo(@"Created AllObjectsDisposed
*** Creating AssemblyToProcess.AllObjectsDisposed 1
*** Disposing AssemblyToProcess.AllObjectsDisposed 1
Disposed AllObjectsDisposed
"));
		}

		[Test]
		public void NotDisposed()
		{
			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var instance = GetInstance("ObjectNotDisposed");
			instance.Dispose();
			Assert.That(_consoleOutput.ToString(),
				Is.EqualTo(@"Created AllObjectsDisposed
*** Creating AssemblyToProcess.AllObjectsDisposed 1
*** Creating AssemblyToProcess.ObjectNotDisposed 1
*** Disposing AssemblyToProcess.ObjectNotDisposed 1
**** Undisposed Object Dump:
" + "\t" + @"AssemblyToProcess.AllObjectsDisposed: 1
"));
		}

		[Test]
		public void TwoConstructorsCallingEachOther()
		{
			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var instance = GetInstance("TwoConstructorsCallingEachOther");
			instance.Dispose();
			Assert.That(_consoleOutput.ToString(),
				Is.EqualTo(@"*** Creating AssemblyToProcess.TwoConstructorsCallingEachOther 1
*** Disposing AssemblyToProcess.TwoConstructorsCallingEachOther 1
**** Undisposed Object Dump:
"));
		}

		[Test]
		public void TwoIndependentConstructors()
		{
			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var instance = GetInstance("TwoIndependentConstructors");
			instance.Dispose();
			Assert.That(_consoleOutput.ToString(),
				Is.EqualTo(@"*** Creating AssemblyToProcess.TwoIndependentConstructors 1
*** Disposing AssemblyToProcess.TwoIndependentConstructors 1
**** Undisposed Object Dump:
"));
		}

		[Test]
		public void DerivedClass()
		{
			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var tic = GetInstance("TwoIndependentConstructors");
			var instance = GetInstance("DerivedClass");
			instance.Dispose();
			tic.Dispose();
			Assert.That(_consoleOutput.ToString(),
				Is.EqualTo(@"*** Creating AssemblyToProcess.TwoIndependentConstructors 1
Created AllObjectsDisposed
*** Creating AssemblyToProcess.DerivedClass 1
*** Disposing AssemblyToProcess.DerivedClass 1
**** Undisposed Object Dump:
" + "\t" + @"AssemblyToProcess.TwoIndependentConstructors: 1
Disposed AllObjectsDisposed
*** Disposing AssemblyToProcess.TwoIndependentConstructors 1
**** Undisposed Object Dump:
"));
		}

		[Test]
		public void NoDefaultCtor()
		{
			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var instance = GetInstance("NoDefaultCtor", string.Empty);
			instance.Dispose();
			Assert.That(_consoleOutput.ToString(),
				Is.EqualTo(@"*** Creating AssemblyToProcess.NoDefaultCtor 1
*** Disposing AssemblyToProcess.NoDefaultCtor 1
**** Undisposed Object Dump:
"));
		}

		[Test]
		public void DerivedClassImplementsIDisposable()
		{
			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var instance = GetInstance("DerivedClassImplementsIDisposable");
			instance.Dispose();
			Assert.That(_consoleOutput.ToString(),
				Is.EqualTo(@"*** Creating AssemblyToProcess.DerivedClassImplementsIDisposable 1
*** Disposing AssemblyToProcess.DerivedClassImplementsIDisposable 1
**** Undisposed Object Dump:
"));
		}

		[Test]
		public void DerivedFromExternalClass()
		{
			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var instance = GetInstance("DerivedFromExternalClass");
			instance.Dispose();
			Assert.That(_consoleOutput.ToString(),
				Is.EqualTo(@"*** Creating AssemblyToProcess.DerivedFromExternalClass 1
*** Disposing AssemblyToProcess.DerivedFromExternalClass 1
**** Undisposed Object Dump:
"));
		}

		[Test]
		public void DerivedFromInternalClass()
		{
			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var instance = GetInstance("DerivedFromInternalClass");
			instance.Dispose();
			Assert.That(_consoleOutput.ToString(),
				Is.EqualTo(@"*** Creating AssemblyToProcess.DerivedFromInternalClass 1
*** Disposing AssemblyToProcess.DerivedFromInternalClass 1
**** Undisposed Object Dump:
"));
		}

		[Test]
		public void RedirectOutput()
		{
			var writer = new StringBuilder();
			DisposeTracker.LogWriter = s => writer.AppendLine(s);

			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var instance = GetInstance("AllObjectsDisposed");
			instance.Dispose();
			Assert.That(writer.ToString(),
				Is.EqualTo(@"*** Creating AssemblyToProcess.AllObjectsDisposed 1
*** Disposing AssemblyToProcess.AllObjectsDisposed 1
**** Undisposed Object Dump:
"));
		}

		[Test]
		public void UntrackedClass()
		{
			SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var instance = GetInstance("UntrackedClass");
			instance.Dispose();
			Assert.That(_consoleOutput.ToString(), Is.Empty);
		}

		private static void SetOutputKind(TrackerOutputKind outputKind)
		{
			DisposeTracker.Reset();
			DisposeTracker.OutputKind = outputKind;
		}

		private dynamic GetInstance(string className, params object[] args)
		{
			var type = _moduleWeaverTestHelper.Assembly.GetType($"AssemblyToProcess.{className}", true);
			return Activator.CreateInstance(type, args);
		}
	}
}
