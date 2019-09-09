// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UndisposedTests")]

namespace Undisposed
{
	public static class DisposeTracker
	{
		private class MyWeakReference : WeakReference
		{
			public MyWeakReference(object obj) : base(obj)
			{
			}

			public override bool Equals(object obj)
			{
				if (!IsAlive)
					return false;

				switch (obj)
				{
					case MyWeakReference otherWeakReference:
						return Target.Equals(otherWeakReference.Target);
					case WeakReference _:
						return base.Equals(obj);
					default:
						return Target.Equals(obj);
				}
			}

			public override int GetHashCode()
			{
				return IsAlive ? Target.GetHashCode() : base.GetHashCode();
			}
		}

		private static Dictionary<Type, int> _Registrations = new Dictionary<Type, int>();
		private static Dictionary<MyWeakReference, int> _ObjectNumber = new Dictionary<MyWeakReference, int>();
		private static Dictionary<Type, List<(int, string)>>_UndisposedObjects =
			new Dictionary<Type, List<(int, string)>>();

		public static TrackerOutputKind OutputKind { get; set; }
		public static Action<string> LogWriter { get; set; }
		public static bool TrackCreationStackTrace { get; set; }

		static DisposeTracker()
		{
			OutputKind = TrackerOutputKind.Dump | TrackerOutputKind.Registration;
			LogWriter = Console.WriteLine;
			LogWriter("*** Undisposed.Fody loaded");
		}

		internal static void Reset()
		{
			lock (_Registrations)
			{
				_Registrations = new Dictionary<Type, int>();
				_ObjectNumber = new Dictionary<MyWeakReference, int>();
				_UndisposedObjects = new Dictionary<Type, List<(int, string)>>();
			}
		}

		public static void Register(object obj)
		{
			lock (_Registrations)
			{
				var t = obj.GetType();
				if (!_Registrations.ContainsKey(t))
					_Registrations.Add(t, 1);
				if (!_UndisposedObjects.ContainsKey(t))
					_UndisposedObjects.Add(t, new List<(int, string)>());

				var thisNumber = _Registrations[t]++;
				_ObjectNumber.Add(new MyWeakReference(obj), thisNumber);
				_UndisposedObjects[t].Add(TrackCreationStackTrace
					? (number: thisNumber, stack: GetStackTraceString())
					: (number: thisNumber, stack: string.Empty));

				if ((OutputKind & TrackerOutputKind.Registration) != 0)
					LogWriter($"*** Creating {t.FullName} {thisNumber}");
			}
		}

		private static string GetStackTraceString()
		{
			var stack = new StackTrace();
			var stackTraceFrames = stack.ToString().Split(new[] { Environment.NewLine },
				StringSplitOptions.None);
			return string.Join(Environment.NewLine, stackTraceFrames.Skip(2));
		}

		public static void Unregister(object obj)
		{
			lock (_Registrations)
			{
				var objType = obj.GetType();
				var reference = new MyWeakReference(obj);
				if (!_ObjectNumber.TryGetValue(reference, out var thisNumber))
				{
					LogWriter($"Disposing {objType.FullName}: Error: Object was not registered");
					return;
				}

				if ((OutputKind & TrackerOutputKind.Registration) != 0)
					LogWriter($"*** Disposing {objType.FullName} {thisNumber}");

				_ObjectNumber.Remove(reference);

				var target = _UndisposedObjects[objType].FirstOrDefault(y => y.Item1 == thisNumber);
				if (target != default)
					_UndisposedObjects[objType].Remove(target);

				if (_UndisposedObjects[objType].Count == 0)
					_UndisposedObjects.Remove(objType);

				DumpUndisposedObjects();
			}
		}

		public static void DumpUndisposedObjects()
		{
			if (TrackCreationStackTrace)
				DumpUndiposedObjectWithStackTrace();
			else
				DumpUndiposedObjectWithoutStackTrace();
		}

		private static void DumpUndiposedObjectWithStackTrace()
		{
			lock (_Registrations)
			{
				if ((OutputKind & TrackerOutputKind.Dump) == 0)
					return;

				LogWriter("**** Undisposed Object Dump:");
				foreach (var type in _UndisposedObjects.Keys)
				{
					foreach (var (number, stack) in _UndisposedObjects[type])
					{
						LogWriter(
							$"\t{type.FullName}: {number}\n\tStack Trace:\n{stack}");
					}
				}
			}
		}

		private static void DumpUndiposedObjectWithoutStackTrace()
		{
			lock (_Registrations)
			{
				if ((OutputKind & TrackerOutputKind.Dump) == 0)
					return;

				LogWriter("**** Undisposed Object Dump:");
				foreach (var type in _UndisposedObjects.Keys)
				{
					LogWriter(
						$"\t{type.FullName}: {string.Join(",", _UndisposedObjects[type].Select(x => { var (n, _) = x; return n.ToString(); }))}");
				}
			}
		}
	}
}

