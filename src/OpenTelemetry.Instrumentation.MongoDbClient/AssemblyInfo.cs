// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// In SDK-style projects such as this one, several assembly attributes that were historically
// defined in this file are now automatically added during build and populated with
// values defined in project properties. For details of which attributes are included
// and how to customize this process see: https://aka.ms/assembly-info-properties

[assembly: AssemblyTitle("OpenTelemetry.Instrumentation.MongoDbClient")]
[assembly: AssemblyDescription("Auto-instrumentation for MongoDB client using OpenTelemetry.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Your Company Name")]
[assembly: AssemblyProduct("OpenTelemetry.Instrumentation.MongoDbClient")]
[assembly: AssemblyCopyright("Copyright © Your Company Name")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible to COM
// components.  If you need to access a type in this assembly from COM, set the ComVisible
// attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM.
[assembly: Guid("f143ade8-d456-4a3a-a90c-242d3f5074b4")]
[assembly: InternalsVisibleTo("OpenTelemetry.Instrumentation.MongoDbClient.Tests")]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]