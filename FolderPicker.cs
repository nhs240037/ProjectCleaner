using System.Runtime.InteropServices;

namespace ProjectCleaner;

internal static class FolderPicker
{
  public static string[] PickFolders(IWin32Window owner, string title)
  {
    IFileOpenDialog dialog = (IFileOpenDialog)new FileOpenDialog();
    try
    {
      dialog.SetTitle(title);
      dialog.SetOptions(FOS.FOS_PICKFOLDERS | FOS.FOS_ALLOWMULTISELECT | FOS.FOS_FORCEFILESYSTEM | FOS.FOS_PATHMUSTEXIST | FOS.FOS_DONTADDTORECENT);

      int hr = dialog.Show(owner.Handle);
      if (hr == (int)HRESULT.ERROR_CANCELLED)
      {
        return Array.Empty<string>();
      }

      Marshal.ThrowExceptionForHR(hr);

      dialog.GetResults(out IShellItemArray? results);
      try
      {
        results.GetCount(out uint count);
        List<string> folders = new((int)count);

        for (uint i = 0; i < count; i++)
        {
          results.GetItemAt(i, out IShellItem? item);
          try
          {
            item.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out nint pathPtr);
            try
            {
              string? path = Marshal.PtrToStringUni(pathPtr);
              if (!string.IsNullOrWhiteSpace(path))
              {
                folders.Add(path);
              }
            }
            finally
            {
              Marshal.FreeCoTaskMem(pathPtr);
            }
          }
          finally
          {
            _ = Marshal.ReleaseComObject(item);
          }
        }

        return folders.ToArray();
      }
      finally
      {
        _ = Marshal.ReleaseComObject(results);
      }
    }
    finally
    {
      _ = Marshal.ReleaseComObject(dialog);
    }
  }

  private enum HRESULT : int
  {
    ERROR_CANCELLED = unchecked((int)0x800704C7)
  }

  [Flags]
  private enum FOS : uint
  {
    FOS_OVERWRITEPROMPT = 0x00000002,
    FOS_STRICTFILETYPES = 0x00000004,
    FOS_NOCHANGEDIR = 0x00000008,
    FOS_PICKFOLDERS = 0x00000020,
    FOS_FORCEFILESYSTEM = 0x00000040,
    FOS_ALLNONSTORAGEITEMS = 0x00000080,
    FOS_NOVALIDATE = 0x00000100,
    FOS_ALLOWMULTISELECT = 0x00000200,
    FOS_PATHMUSTEXIST = 0x00000800,
    FOS_FILEMUSTEXIST = 0x00001000,
    FOS_CREATEPROMPT = 0x00002000,
    FOS_SHAREAWARE = 0x00004000,
    FOS_NOREADONLYRETURN = 0x00008000,
    FOS_NOTESTFILECREATE = 0x00010000,
    FOS_HIDEMRUPLACES = 0x00020000,
    FOS_HIDEPINNEDPLACES = 0x00040000,
    FOS_NODEREFERENCELINKS = 0x00100000,
    FOS_DONTADDTORECENT = 0x02000000,
    FOS_FORCESHOWHIDDEN = 0x10000000
  }

  private enum SIGDN : uint
  {
    SIGDN_FILESYSPATH = 0x80058000
  }

  [ComImport]
  [Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
  private class FileOpenDialog
  {
  }

  [ComImport]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid("D57C7288-D4AD-4768-BE02-9D969532D960")]
  private interface IFileOpenDialog
  {
    [PreserveSig] int Show(IntPtr parent);
    void SetFileTypes(uint cFileTypes, IntPtr rgFilterSpec);
    void SetFileTypeIndex(uint iFileType);
    void GetFileTypeIndex(out uint piFileType);
    void Advise(IntPtr pfde, out uint pdwCookie);
    void Unadvise(uint dwCookie);
    void SetOptions(FOS fos);
    void GetOptions(out FOS pfos);
    void SetDefaultFolder(IShellItem psi);
    void SetFolder(IShellItem psi);
    void GetFolder(out IShellItem ppsi);
    void GetCurrentSelection(out IShellItem ppsi);
    void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
    void GetFileName(out IntPtr pszName);
    void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
    void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
    void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
    void GetResult(out IShellItem ppsi);
    void AddPlace(IShellItem psi, int alignment);
    void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
    void Close(int hr);
    void SetClientGuid(ref Guid guid);
    void ClearClientData();
    void SetFilter(IntPtr pFilter);
    void GetResults(out IShellItemArray ppenum);
    void GetSelectedItems(out IShellItemArray ppsai);
  }

  [ComImport]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
  private interface IShellItem
  {
    void BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);
    void GetParent(out IntPtr ppsi);
    void GetDisplayName(SIGDN sigdnName, out IntPtr ppszName);
    void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
    void Compare(IShellItem psi, uint hint, out int piOrder);
  }

  [ComImport]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid("B63EA76D-1F85-456F-A19C-48159EFA858B")]
  private interface IShellItemArray
  {
    void BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppvOut);
    void GetPropertyStore(int flags, ref Guid riid, out IntPtr ppv);
    void GetPropertyDescriptionList(ref PROPERTYKEY keyType, ref Guid riid, out IntPtr ppv);
    void GetAttributes(uint dwAttribFlags, uint sfgaoMask, out uint psfgaoAttribs);
    void GetCount(out uint pdwNumItems);
    void GetItemAt(uint dwIndex, out IShellItem ppsi);
    void EnumItems(out IntPtr ppenumShellItems);
  }

  [StructLayout(LayoutKind.Sequential, Pack = 4)]
  private struct PROPERTYKEY
  {
    public Guid fmtid;
    public uint pid;
  }
}

