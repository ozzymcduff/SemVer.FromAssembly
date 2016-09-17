# SemVer.FromAssembly [![Build Status](https://travis-ci.org/wallymathieu/SemVer.FromAssembly.svg?branch=master)](https://travis-ci.org/wallymathieu/SemVer.FromAssembly) [![Build status](https://ci.appveyor.com/api/projects/status/8de3t84iae9utkcd/branch/master?svg=true)](https://ci.appveyor.com/project/wallymathieu/semver-fromassembly/branch/master) [![NuGet](http://img.shields.io/nuget/v/SemVer.FromAssembly.svg)](https://www.nuget.org/packages/SemVer.FromAssembly/)

## Goal

Autogenerate nuget package version based on surface area of the new package


## Usage

USAGE: SemVer.FromAssembly.exe [--help] [--surface-of <path>] [--diff <original> <new>] [--magnitude <original> <new>]

OPTIONS:

    --surface-of <path>   specify a file.
    --diff <original> <new>
                          specify original and new
    --magnitude <original> <new>
                          specify original and new
    --help                display this list of options.
