using Godot;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;


//Research:

//http://www.pinvoke.net/index.aspx
//https://stackoverflow.com/questions/1978077/c-sharp-set-window-behind-desktop-icons
//https://fernandomachadopirizen.wordpress.com/2010/08/09/give-me-a-handle-and-i-will-move-the-earth/
//https://stackoverflow.com/questions/7542669/issue-using-show-desktop-with-setparent-in-wpf
//https://www.codeproject.com/Articles/856020/Draw-Behind-Desktop-Icons-in-Windows-plus
//https://stackoverflow.com/questions/115868/how-do-i-get-the-title-of-the-current-active-window-using-c
//https://stackoverflow.com/questions/54481799/getting-the-list-view-control-handle-from-an-ishellview-instance
//https://dynamicwallpaper.readthedocs.io/en/docs/dev/make-wallpaper.html


#region Closing Created workerW
//Essentially, instead of doing this clear the desktop window instead as shown here:  https://stackoverflow.com/a/50804645
//https://learn.microsoft.com/en-us/dotnet/core/extensions/workers?pivots=dotnet-7-0
//https://learn.microsoft.com/en-us/windows/win32/winmsg/wm-close?redirectedfrom=MSDN
//https://stackoverflow.com/questions/1129204/how-to-use-wm-close-in-c
//https://stackoverflow.com/questions/53109281/what-is-the-windows-workerw-windows-and-what-creates-them
#endregion
public partial class WindowManager : Node
{

	public IntPtr workerW = IntPtr.Zero;
   

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		IntPtr hWnd = NativeMethods.GetActiveWindow();

		//I did not come up with this, I learned it here, PLEASE SEE LISCENSE: https://www.codeproject.com/Articles/856020/Draw-Behind-Desktop-Icons-in-Windows-plus, https://www.codeproject.com/info/cpol10.aspx
		#region Getting Desktop and creating custom workerW

		IntPtr result = IntPtr.Zero;

		// Fetch the Progman window
		IntPtr progman = NativeMethods.FindWindow("Progman", null);

		// Send 0x052C to Progman. This message directs Progman to spawn a 
		// WorkerW behind the desktop icons. If it is already there, nothing 
		// happens.
		NativeMethods.SendMessageTimeout(progman,
							   0x052C,
							   new IntPtr(0),
							   IntPtr.Zero,
							   NativeMethods.SendMessageTimeoutFlags.SMTO_NORMAL,
							   1000,
							   out result);

		workerW = IntPtr.Zero;

		NativeMethods.EnumWindows(new NativeMethods.EnumWindowsProc((tophandle, topparamhandle) =>
		{
			IntPtr p = NativeMethods.FindWindowEx(tophandle,
										IntPtr.Zero,
										"SHELLDLL_DefView",
										null);

			if (p != IntPtr.Zero)
			{
				// Gets the WorkerW Window after the current one.
				workerW = NativeMethods.FindWindowEx(IntPtr.Zero,
										   tophandle,
										   "WorkerW",
										   null);
			}

			return 1;
		}), IntPtr.Zero);

		NativeMethods.SetParent(hWnd, workerW);

		#endregion

		//clearDesktop(); //If you want to see the clearDesktop() function working or don't want to log out to repaint your background uncomment this.
	}



	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public override void _Notification(int what)
	{

		if (what == NotificationWMCloseRequest) //an attempt at following the docs, I think if I was using the x button to close the window this would work
		{										//but it doesn't because we use the exit debug button in Godot to exit it.
			clearDesktop();
			GetTree().Quit(); // default behavior
		}
	}

	#region Clear Desktop Code
	//Didn't come up with this, found it here: https://stackoverflow.com/a/50804645


	public static void clearDesktop()
	{
		SetDesktopWallpaper(GetDesktopWallpaper());
	}

	private static readonly int MAX_PATH = 260;
	private static readonly uint SPI_GETDESKWALLPAPER = 0x73;
	private static readonly uint SPI_SETDESKWALLPAPER = 0x14;
	private static readonly uint SPIF_UPDATEINIFILE = 0x01;
	private static readonly uint SPIF_SENDWININICHANGE = 0x02;

	static string GetDesktopWallpaper()
	{
		string wallpaper = new string('\0', MAX_PATH);
		// NativeMethods.SystemParametersInfo(SPI_GETDESKWALLPAPER, wallpaper.Length, wallpaper, 0);
		NativeMethods.SystemParametersInfo(SPI_GETDESKWALLPAPER, (uint)wallpaper.Length, wallpaper, 0);
		return wallpaper.Substring(0, wallpaper.IndexOf('\0'));
	}

	static void SetDesktopWallpaper(string filename)
	{
		//NativeMethods.SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, filename, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
		NativeMethods.SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, filename, NativeMethods.SPIF.SPIF_UPDATEINIFILE | NativeMethods.SPIF.SPIF_SENDWININICHANGE);
	}

	#endregion



	//Ignore the stuff below here, I'm going to refactor it so it works with my new system, the old one lies below. Much cleaner than that.

	/*	/// <summary>
		/// Toggles desktop icons visibility by sending a low level command to desktop window in the same way as the popup menu "Show desktop  
		/// icons" does. Use <code>AreDesktopIconsVisible</code> to get current desktop icons visibility status.  
		/// </summary>  
		private static void ToggleDesktopIconsVisibility()
		{
			IntPtr defaultViewHandle = FindShellWindow();
			UIntPtr resultArgument;
			IntPtr returnValue = NativeMethods.SendMessageTimeout(
				defaultViewHandle,
				NativeMethods.WM_COMMAND,
				new UIntPtr(29698),
				IntPtr.Zero,
				NativeMethods.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG,
				1000,
			   out resultArgument);
		}


		private static string GetWindowTitle(IntPtr handle)
		{
			const int nChars = 256;
			StringBuilder Buff = new StringBuilder(nChars);

			if (GetWindowText(handle, Buff, nChars) > 0)
			{
				return Buff.ToString();
			}
			return null;
		}*/


	/*/// <summary>
	/// Called by EnumWindows. Sets <code>shellWindowHandle</code> if a window with class "SHELLDLL_DefView" is found during enumeration.
	/// </summary>
	/// <param name="handle">The handle of the window being enumerated.</param>
	/// <param name="param">The argument passed to <code>EnumWindowsProc</code>; not used in this application.</param>
	/// <returns>Allways returns 1.</returns>
	private static int EnumWindowsProc(IntPtr handle, int param)
	{
		try
		{
			IntPtr foundHandle = NativeMethods.FindWindowEx(handle, IntPtr.Zero, "SHELLDLL_DefView", null); //we need to get SysListView32 windows handle from under this handle.
			if (!foundHandle.Equals(IntPtr.Zero))
			{
				Debug.WriteLine(GetWindowTitle(foundHandle));
				shellWindowHandle = foundHandle;
				return 0;
			}
		}
		catch
		{
			// Intentionally left blank
		}

		return 1;
	}

	/// <summary>
	/// Finds the window containing desktop icons.
	/// </summary>
	/// <returns>The handle of the window.</returns>
	private static IntPtr FindShellWindow()
	{
		IntPtr progmanHandle;
		IntPtr defaultViewHandle = IntPtr.Zero;
		IntPtr workerWHandle;
		int errorCode = NativeMethods.ERROR_SUCCESS;

		// Try the easy way first. "SHELLDLL_DefView" is a child window of "Progman".
		progmanHandle = NativeMethods.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Progman", null);

		if (!progmanHandle.Equals(IntPtr.Zero))
		{
			defaultViewHandle = NativeMethods.FindWindowEx(progmanHandle, IntPtr.Zero, "SHELLDLL_DefView", null);
			errorCode = Marshal.GetLastWin32Error();
		}

		if (!defaultViewHandle.Equals(IntPtr.Zero))
		{
			return defaultViewHandle;
		}
		else if (errorCode != NativeMethods.ERROR_SUCCESS)
		{
			Marshal.ThrowExceptionForHR(errorCode);
		}

		// Try the not so easy way then. In some systems "SHELLDLL_DefView" is a child of "WorkerW".
		errorCode = NativeMethods.ERROR_SUCCESS;
		workerWHandle = NativeMethods.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "WorkerW", null);
		Debug.WriteLine("FindShellWindow.workerWHandle: {0}", workerWHandle);

		if (!workerWHandle.Equals(IntPtr.Zero))
		{
			defaultViewHandle = NativeMethods.FindWindowEx(workerWHandle, IntPtr.Zero, "SHELLDLL_DefView", null);
			errorCode = Marshal.GetLastWin32Error();
			Debug.WriteLine("ERROR");
		}

		if (!defaultViewHandle.Equals(IntPtr.Zero))
		{
			Debug.WriteLine("Found Default View Handle");
			return defaultViewHandle;
		}
		else if (errorCode != NativeMethods.ERROR_SUCCESS)
		{
			Marshal.ThrowExceptionForHR(errorCode);
		}

		shellWindowHandle = IntPtr.Zero;

		// Try the hard way. In some systems "SHELLDLL_DefView" is a child or a child of "Progman".
*//*		if (NativeMethods.EnumWindows(EnumWindowsProc, progmanHandle) == false)
		{
			Debug.WriteLine("Found Handle the hard Way");
			Debug.WriteLine("FindShellWindow.workerWHandle: {0}", shellWindowHandle);
			errorCode = Marshal.GetLastWin32Error();
			if (errorCode != NativeMethods.ERROR_SUCCESS)
			{
				Marshal.ThrowExceptionForHR(errorCode);
			}
		}*//*

		// Try the even more harder way. Just in case "SHELLDLL_DefView" is in another desktop.
		if (shellWindowHandle.Equals(IntPtr.Zero))
		{
			if (NativeMethods.EnumDesktopWindows(IntPtr.Zero, EnumWindowsProc, progmanHandle))
			{
				Debug.WriteLine("FindShellWindow.workerWHandle: {0}", shellWindowHandle);
				errorCode = Marshal.GetLastWin32Error();
				if (errorCode != NativeMethods.ERROR_SUCCESS)
				{
					Marshal.ThrowExceptionForHR(errorCode);
				}
			}
		}

		return shellWindowHandle;
	}*/
}
