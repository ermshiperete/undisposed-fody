// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using NUnit.Framework;
using System;
using System.Reflection;
using Undisposed;
using System.Text;
using System.IO;
using Mono.Cecil;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace UndisposedTests
{
	public class MyModuleWeaver: ModuleWeaver
	{
		private StringBuilder bldr = new StringBuilder();

		public MyModuleWeaver()
		{
			LogInfo = s => bldr.Append(s);
			LogWarning = s => bldr.Append(s);
			LogError = s => bldr.Append(s);
		}

		public string GetLog()
		{
			return bldr.ToString();
		}
	}

	public class Tester: MarshalByRefObject
	{
		private readonly StringWriter _consoleOutput;

		public Tester()
		{
			_consoleOutput = new StringWriter();
			Console.SetOut(_consoleOutput);
		}

		public void SetOutputKind(TrackerOutputKind outputKind)
		{
			DisposeTracker.Reset();
			DisposeTracker.OutputKind = outputKind;
		}

		public string AllObjectsDisposed()
		{
			_consoleOutput.GetStringBuilder().Clear();
			using (var a = new A())
			{
			}
			return _consoleOutput.ToString();
		}

		public string ObjectNotDisposed()
		{
			_consoleOutput.GetStringBuilder().Clear();
			using (var b = new B())
			{
			}
			return _consoleOutput.ToString();
		}

		public string TwoConstructorsCallingEachOther()
		{
			_consoleOutput.GetStringBuilder().Clear();
			using (var c = new C())
			{
			}
			return _consoleOutput.ToString();
		}

		public string TwoIndependentConstructors()
		{
			_consoleOutput.GetStringBuilder().Clear();
			using (var d = new D())
			{
			}
			return _consoleOutput.ToString();
		}

		public string DerivedClass()
		{
			_consoleOutput.GetStringBuilder().Clear();
			using (var d = new D())
			{
				using (var e = new E())
				{
				}
			}
			return _consoleOutput.ToString();
		}

		public string NoDefaultCtor()
		{
			_consoleOutput.GetStringBuilder().Clear();
			using (var f = new F(string.Empty))
			{
			}
			return _consoleOutput.ToString();
		}

		public string DerivedClassImplementsIDisposable()
		{
			_consoleOutput.GetStringBuilder().Clear();
			using (var g = new G())
			{
			}
			return _consoleOutput.ToString();
		}
	}

	public class A: IDisposable
	{
		public A()
		{
			Console.WriteLine("Created A");
		}

		public void Dispose()
		{
			Console.WriteLine("Disposed A");
		}
	}

	public class B: IDisposable
	{
		private A a;

		public B()
		{
			a = new A();
		}

		public void Dispose()
		{
		}
	}

	public class C: IDisposable
	{
		public C()
			: this(string.Empty)
		{
		}

		public C(string x)
		{
		}

		public void Dispose()
		{
		}
	}

	public class D: IDisposable
	{
		public D()
		{
		}

		public D(string x)
		{
		}

		public void Dispose()
		{
		}
	}

	public class E: A
	{
		public E()
			: base()
		{
		}
	}

	public class F: IDisposable
	{
		public F(string x)
		{
		}

		public void Dispose()
		{
		}
	}

	public class G: F, IDisposable
	{
		public G()
			: base(string.Empty)
		{
		}

		void IDisposable.Dispose()
		{
		}
	}

	[TestFixture]
	public class UndisposedTests
	{
		private MyModuleWeaver _moduleWeaver;
		private string _tempAssemblyFileName;
		private Tester _Tester;
		private AppDomain _TestDomain;

		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			_tempAssemblyFileName = Path.Combine(assemblyDir, Path.GetRandomFileName() + ".dll");
			var def = Mono.Cecil.ModuleDefinition.ReadModule(Assembly.GetExecutingAssembly().Location);
			_moduleWeaver = new MyModuleWeaver();
			_moduleWeaver.ModuleDefinition = def;
			_moduleWeaver.Execute();
			def.Write(_tempAssemblyFileName);

			// Construct and initialize settings for a second AppDomain.
			var ads = new AppDomainSetup();
			ads.ApplicationBase = assemblyDir;
			ads.DisallowBindingRedirects = false;
			ads.DisallowCodeDownload = true;
			ads.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

			// Create the second AppDomain.
			_TestDomain = AppDomain.CreateDomain("Test domain", null, ads);
			_Tester = (Tester)_TestDomain.CreateInstanceAndUnwrap(Path.GetFileNameWithoutExtension(_tempAssemblyFileName), typeof(Tester).FullName);
		}

		[TestFixtureTearDown]
		public void FixtureTeardDown()
		{
			AppDomain.Unload(_TestDomain);
			File.Delete(_tempAssemblyFileName);
		}

		[Test]
		public void TurnedOff()
		{
			_Tester.SetOutputKind(TrackerOutputKind.None);
			var output = _Tester.AllObjectsDisposed();
			Assert.AreEqual("Created A\nDisposed A\n", output);
		}

		[Test]
		public void AllDisposed()
		{
			_Tester.SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var output = _Tester.AllObjectsDisposed();
			Assert.AreEqual("Created A\n*** Creating UndisposedTests.A 1\n*** Disposing UndisposedTests.A 1\n**** Undisposed Object Dump:\nDisposed A\n", output);
		}

		[Test]
		public void AllDisposedDumpOnly()
		{
			_Tester.SetOutputKind(TrackerOutputKind.Dump);
			var output = _Tester.AllObjectsDisposed();
			Assert.AreEqual("Created A\n**** Undisposed Object Dump:\nDisposed A\n", output);
		}

		[Test]
		public void AllDisposedRegistrationOnly()
		{
			_Tester.SetOutputKind(TrackerOutputKind.Registration);
			var output = _Tester.AllObjectsDisposed();
			Assert.AreEqual("Created A\n*** Creating UndisposedTests.A 1\n*** Disposing UndisposedTests.A 1\nDisposed A\n", output);
		}

		[Test]
		public void NotDisposed()
		{
			_Tester.SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var output = _Tester.ObjectNotDisposed();
			Assert.AreEqual("Created A\n*** Creating UndisposedTests.A 1\n*** Creating UndisposedTests.B 1\n*** Disposing UndisposedTests.B 1\n**** Undisposed Object Dump:\n\tUndisposedTests.A: 1,\n", output);
		}

		[Test]
		public void TwoConstructorsCallingEachOther()
		{
			_Tester.SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var output = _Tester.TwoConstructorsCallingEachOther();
			Assert.AreEqual("*** Creating UndisposedTests.C 1\n*** Disposing UndisposedTests.C 1\n**** Undisposed Object Dump:\n", output);
		}

		[Test]
		public void TwoIndependentConstructors()
		{
			_Tester.SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var output = _Tester.TwoIndependentConstructors();
			Assert.AreEqual("*** Creating UndisposedTests.D 1\n*** Disposing UndisposedTests.D 1\n**** Undisposed Object Dump:\n", output);
		}

		[Test]
		public void DerivedClass()
		{
			_Tester.SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var output = _Tester.DerivedClass();
			Assert.AreEqual("*** Creating UndisposedTests.D 1\nCreated A\n*** Creating UndisposedTests.E 1\n" +
				"*** Disposing UndisposedTests.E 1\n**** Undisposed Object Dump:\n\tUndisposedTests.D: 1,\n" +
				"Disposed A\n*** Disposing UndisposedTests.D 1\n**** Undisposed Object Dump:\n", output);
		}

		[Test]
		public void NoDefaultCtor()
		{
			_Tester.SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var output = _Tester.NoDefaultCtor();
			Assert.AreEqual("*** Creating UndisposedTests.F 1\n*** Disposing UndisposedTests.F 1\n**** Undisposed Object Dump:\n", output);
		}

		[Test]
		public void DerivedClassImplementsIDisposable()
		{
			_Tester.SetOutputKind(TrackerOutputKind.Dump | TrackerOutputKind.Registration);
			var output = _Tester.DerivedClassImplementsIDisposable();
			Assert.AreEqual("*** Creating UndisposedTests.G 1\n*** Disposing UndisposedTests.G 1\n**** Undisposed Object Dump:\n", output);
		}

	}
}

