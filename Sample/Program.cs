// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;

namespace Sample
{
	class A: IDisposable
	{
		public A()
		{
		}

		public void Dispose()
		{
		}
	}

	class B: IDisposable
	{
		private A a;
		public B()
		{
			a = new A();
		}

		public void Dispose()
		{
			// forget to dispose A
		}
	}

	class MainClass
	{
		public static void Main(string[] args)
		{
			using (new B())
			{}
		}
	}
}
