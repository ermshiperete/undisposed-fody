Undisposed.Fody
===============

Undisposed.Fody is a [Fody](https://github.com/Fody/Fody) addin and a standalone application
that helps to track down undisposed objects. Occasionally
crashes or application hangs are caused by undisposed objects,
especially if the objects references COM objects.

This tool injects some code in all classes that implement
IDisposable. It registers the creation of such objects and dumps
the undisposed objects together with object creation counter. This
helps to set a breakpoint in the constructor and break on the n-th
call, thus making it possible to see where the object got created
(and not disposed later on).

Usage
-----

Several methods are available to control the behavior of the
dispose tracker:

- `Undisposed.DisposeTracker.OutputKind`: controls the output on
  creating and disposing objects.

  `TrackerOutputKind.Registration`: output a message to console on
  the creation and disposal of objects

  `TrackerOutputKind.Dump`: after disposing an object dump all
  remaining undisposed objects.

  `TrackerOutputKind.None`: no output. This is useful to speed up
  the application. Object creation and disposal are still tracked;
  one of the output options can be set in debugger at a relevant
  point in code.

  The default value is to output both.

- `Undisposed.DisposeTracker.TrackCreationStackTrace`: if set to `true` the call stack will
  be dumped below the dumped undisposed object.

- `Undisposed.DisposeTracker.DumpUndisposedObjects()`:
  Dumps all undisposed objects.

### Simple Case

All objects which implement `IDisposable` will have their creation/disposal tracked.

#### Your Code

    public class Sample : IDisposable
    {
        public void Dispose()
        {
        }
    }

#### What gets compiled

    public class Sample : IDisposable
    {
        public Sample()
        {
            Undisposed.DisposeTracker.Register(this);
        }

        public void Dispose()
        {
            Undisposed.DisposeTracker.Unregister(this);
        }
    }

### What if I have a class I don't want to be tracked

Simple. Just add the `[Undisposed.DoNotTrack]` attribute to the class you don't want to be
tracked. Then Undisposed will not touch it.

Installation
------------

### From nuget package

Simply install the `Undisposed.Fody` nuget package.

### From source
Create a subdirectory `Tools` in the solution directory and copy
`Undisposed.Fody.dll` there. Add the nuget package `Fody` to your
project, add <Undisposed/> to the `FodyWeavers.xml` file, add a
`WeaverFiles` item group which includes `Undisposed.Fody.dll` like this:

```
  <ItemGroup>
    <WeaverFiles Include="..\Tools\Undisposed.Fody.dll" />
  </ItemGroup>
```

and add a reference to `Undisposed.Fody.dll`.

When you build your project the dispose tracking calls get
injected. The Sample project demonstrates the usage.
