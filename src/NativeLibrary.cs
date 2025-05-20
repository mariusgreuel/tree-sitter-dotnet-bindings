//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TreeSitter;

internal static class NativeLibrary
{
    static class Kernel32
    {
        const string Library = "kernel32";

        [DllImport(Library, SetLastError = true)]
        public static extern IntPtr LoadLibraryEx(string libraryName, IntPtr handle, int flags);

        [DllImport(Library, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr handle, string name);

        [DllImport(Library, SetLastError = true)]
        public static extern int FreeLibrary(IntPtr handle);
    }

    static class Libdl
    {
        const string Library = "libdl.so.2";

        public const int RTLD_NOW = 2;

        [DllImport(Library)]
        public static extern IntPtr dlopen(string libraryName, int flags);

        [DllImport(Library)]
        public static extern int dlclose(IntPtr handle);

        [DllImport(Library)]
        public static extern IntPtr dlsym(IntPtr handle, string name);

        [DllImport(Library)]
        public static extern IntPtr dlerror();
    }

    abstract class Loader
    {
        public abstract IntPtr Load(string libraryName, int flags, bool throwOnError);
        public abstract void Free(IntPtr handle);
        public abstract IntPtr GetExport(IntPtr handle, string name);
        public abstract IEnumerable<string> GetLibraryNameVariations(string name);

        public static Loader GetPlatformDependentLoader()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsLoader();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new LinuxLoader();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public string GetNativeRuntimeTarget(string name)
        {
            return Path.Combine(AppContext.BaseDirectory, "runtimes", GetRID(), "native", name);
        }

        static string GetRID()
        {
            string architecture = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return $"win-{architecture}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return $"linux-{architecture}";
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }

    class WindowsLoader : Loader
    {
        public override IntPtr Load(string libraryName, int flags, bool throwOnError)
        {
            var library = Kernel32.LoadLibraryEx(libraryName, IntPtr.Zero, flags);
            if (library == IntPtr.Zero && throwOnError)
            {
                var innerException = new Win32Exception();
                throw new DllNotFoundException($"Unable to load dynamic link library '{libraryName}' or one of its dependencies.", innerException);
            }

            return library;
        }

        public override void Free(IntPtr handle) => Kernel32.FreeLibrary(handle);
        public override IntPtr GetExport(IntPtr handle, string name) => Kernel32.GetProcAddress(handle, name);

        public override IEnumerable<string> GetLibraryNameVariations(string name)
        {
            yield return name;

            if (!name.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
            {
                yield return name + ".dll";
            }
        }
    }

    class LinuxLoader : Loader
    {
        public override IntPtr Load(string libraryName, int flags, bool throwOnError)
        {
            var library = Libdl.dlopen(libraryName, Libdl.RTLD_NOW);
            if (library == IntPtr.Zero && throwOnError)
            {
                var error = Marshal.PtrToStringAnsi(Libdl.dlerror());
                throw new DllNotFoundException($"Unable to load shared library '{libraryName}' or one of its dependencies: {error}");
            }

            return library;
        }

        public override void Free(IntPtr handle) => Libdl.dlclose(handle);
        public override IntPtr GetExport(IntPtr handle, string name) => Libdl.dlsym(handle, name);

        public override IEnumerable<string> GetLibraryNameVariations(string name)
        {
            yield return name;
            yield return "lib" + name;
            yield return name + ".so";
            yield return "lib" + name + ".so";
        }
    }

    public static IntPtr Load(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        return LoadLibraryByName(libraryName, assembly, searchPath, throwOnError: true);
    }

    public static bool TryLoad(string libraryName, Assembly assembly, DllImportSearchPath? searchPath, out IntPtr handle)
    {
        handle = LoadLibraryByName(libraryName, assembly, searchPath, throwOnError: false);
        return handle != IntPtr.Zero;
    }

    public static void Free(IntPtr handle)
    {
        if (handle != IntPtr.Zero)
        {
            s_loader.Free(handle);
        }
    }

    public static IntPtr GetExport(IntPtr handle, string name)
    {
        var address = s_loader.GetExport(handle, name);
        if (address == IntPtr.Zero)
        {
            throw new EntryPointNotFoundException($"Could not find entry point '{name}' in library");
        }

        return address;
    }

    public static bool TryGetExport(IntPtr handle, string name, out IntPtr address)
    {
        address = s_loader.GetExport(handle, name);
        return address != IntPtr.Zero;
    }

    static IntPtr LoadLibraryByName(string libraryName, Assembly assembly, DllImportSearchPath? searchPath, bool throwOnError)
    {
        return LoadLibraryByName(libraryName, assembly, searchPath ?? DllImportSearchPath.AssemblyDirectory, throwOnError);
    }

    static IntPtr LoadLibraryByName(string libraryName, Assembly assembly, DllImportSearchPath searchPath, bool throwOnError)
    {
        int flags = (int)(searchPath & ~DllImportSearchPath.AssemblyDirectory);
        bool searchAssemblyDirectory = (searchPath & DllImportSearchPath.AssemblyDirectory) != 0;

        foreach (var variation in s_loader.GetLibraryNameVariations(Path.GetFileName(libraryName)))
        {
            IntPtr handle;

            if (Path.IsPathRooted(libraryName))
            {
                handle = s_loader.Load(Path.Combine(Path.GetDirectoryName(libraryName), variation), flags, throwOnError: false);
                if (handle != IntPtr.Zero)
                {
                    return handle;
                }
            }
            else if (searchAssemblyDirectory && assembly != null)
            {
                handle = s_loader.Load(Path.Combine(AppContext.BaseDirectory, variation), flags, throwOnError: false);
                if (handle != IntPtr.Zero)
                {
                    return handle;
                }

                handle = s_loader.Load(s_loader.GetNativeRuntimeTarget(variation), flags, throwOnError: false);
                if (handle != IntPtr.Zero)
                {
                    return handle;
                }
            }

            handle = s_loader.Load(variation, flags, throwOnError: false);
            if (handle != IntPtr.Zero)
            {
                return handle;
            }
        }

        return s_loader.Load(libraryName, flags, throwOnError);
    }

    static readonly Loader s_loader = Loader.GetPlatformDependentLoader();
}
