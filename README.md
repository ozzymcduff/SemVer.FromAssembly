# SemVer.FromAssembly [![Build Status](https://travis-ci.org/wallymathieu/SemVer.FromAssembly.svg?branch=master)](https://travis-ci.org/wallymathieu/SemVer.FromAssembly) [![Build status](https://ci.appveyor.com/api/projects/status/8de3t84iae9utkcd/branch/master?svg=true)](https://ci.appveyor.com/project/wallymathieu/semver-fromassembly/branch/master) [![NuGet](http://img.shields.io/nuget/v/SemVer.FromAssembly.svg)](https://www.nuget.org/packages/SemVer.FromAssembly/)

## Goal

Autogenerate nuget package version based on surface area of the new package


## Usage

USAGE: SemVer.FromAssembly.exe [--help] [--surface-of <path>] [--output <path>] [--diff <original> <new>] [--magnitude <original> <new>]

OPTIONS:

    --surface-of <path>   Get the public api surface of the .net binary as json
    --output <path>       Send output to file
    --diff <original> <new>
                          Get the difference between two .net binaries as json
    --magnitude <original> <new>
                          Get the magnitude of the difference between two .net binaries
    --help                display this list of options.

## Together with FAKE

Download and modify [SemVer.FromAssembly.FAKE.fsx](https://github.com/wallymathieu/SemVer.FromAssembly/blob/master/SemVer.FromAssembly.FAKE.fsx) 

## Together with Cake

Use [Cake.SemVer.FromAssembly](https://github.com/wallymathieu/Cake.SemVer.FromAssembly)
