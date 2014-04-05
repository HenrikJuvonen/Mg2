Mg2
===

This is an experimental package manager GUI for OneGet
(https://oneget.codeplex.com/).

The GUI is heavily influenced by Synaptic Package Manager
(http://www.nongnu.org/synaptic/).

![Screenshot](https://raw.githubusercontent.com/henjuv/Mg2/master/screenshot.png)

License
-------

Apache License 2.0

Requirements
------------

* OneGet PowerShell-module
* .NET 4.5 Framework
* Galasoft.MvvmLight.WPF45 + Extras (included)
* System.Windows.Interactivity (included)
* Microsoft.Practices.ServiceLocation (included)

Build
-----

Open solution file with Visual Studio 2012 and hit 'Build'.

Background
----------

This is based on some code I wrote over a year ago while I was trying out
some MVVM toolkits and new .NET 4.5 features. Back then I attempted to add
support for NuGet, but it didn't go too well, so I stopped working on this.
Yesterday (2014/04/05) I decided to check out if I could add support for
OneGet without much effort.

For now, the GUI supports only package listing, installing and uninstalling,
and package source add and remove (for Chocolatey). Also it does not have any
progress updates nor dependency handling for packages (since the PowerShell
does not seem to output these). Filtering and options work. Loading and saving
markings is not yet implemented.

Originally I didn't plan to release the code, so it might require some
commenting and refactoring here and there.
