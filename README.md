# SemVer.FromAssembly [![Build Status](https://travis-ci.org/wallymathieu/SemVer.FromAssembly.svg?branch=master)](https://travis-ci.org/wallymathieu/SemVer.FromAssembly) [![Build status](https://ci.appveyor.com/api/projects/status/8de3t84iae9utkcd/branch/master?svg=true)](https://ci.appveyor.com/project/wallymathieu/semver-fromassembly/branch/master) [![NuGet](http://img.shields.io/nuget/v/SemVer.FromAssembly.svg)](https://www.nuget.org/packages/SemVer.FromAssembly/)

# NOTE!

This library/exe is going to be replaced by [SyntacticVersioning](https://github.com/fsprojects/SyntacticVersioning).

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

## Together with Albacore and Rake 

```
require "tmpdir"
require "fileutils"
require "securerandom"
require "nuget_helper"

def magnitude_next_nuget_version(package_name, next_dll)
  begin
    tmp = File.join(Dir.tmpdir, SecureRandom.hex)
    NugetHelper.exec("install '#{package_name}' -o #{tmp} ")
    orig = Dir.glob( File.join(tmp, "**", File.basename(next_dll)) ).first
    path_to_package = Dir.glob( File.join(tmp, "*") ).first 
    v = SemVer.parse(path_to_package)
    m = NugetHelper.run_tool_with_result(NugetHelper.semver_fromassembly_path, " --magnitude #{orig} #{next_dll}").strip
    case m
    when 'Patch'
      v.patch += 1
      v
    when 'Major'
      v.major += 1
      v.minor = 0
      v.patch = 0
      v
    when 'Minor'
      v.minor += 1
      v.patch = 0
      v
    end
  ensure
    FileUtils.rm_rf(tmp)
  end
end

task :bump do
    v = magnitude_next_nuget_version "PACKAGE_NAME", "PACKAGE/bin/Debug/PACKAGE.dll"
    # use v to set the version
end
```
