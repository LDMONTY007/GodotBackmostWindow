using Godot;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;


//Research:

//https://fernandomachadopirizen.wordpress.com/2010/08/09/give-me-a-handle-and-i-will-move-the-earth/
public partial class WindowManager : Node
{

	public IntPtr workerW = IntPtr.Zero;
   

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		IntPtr hWnd = NativeMethods.GetActiveWindow();
		/*            IntPtr desktophWnd = FindShellWindow();//NativeMethods.FindWindow("Progman", "Program Manager");

					if (hWnd == IntPtr.Zero)
					{
						Debug.WriteLine("NOTWORKING");
					}

					IntPtr newFoundWindow = NativeMethods.FindWindowEx(desktophWnd, IntPtr.Zero, "SysListView32", null);

					Debug.WriteLine(GetWindowTitle(desktophWnd));
					Debug.WriteLine(GetWindowTitle(newFoundWindow));*/


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

/*        IntPtr dc = NativeMethods.GetDCEx(workerW, IntPtr.Zero, (NativeMethods.DeviceContextValues)0x403);
		if (dc != IntPtr.Zero)
		{
			// Create a Graphics instance from the Device Context
			using (Graphics g = Graphics.FromHdc(dc))
			{

				// Use the Graphics instance to draw a white rectangle in the upper 
				// left corner. In case you have more than one monitor think of the 
				// drawing area as a rectangle that spans across all monitors, and 
				// the 0,0 coordinate being in the upper left corner.
				g.FillRectangle(new SolidBrush(Color.Blue), 0, 0, 500, 500);

			}
			// make sure to release the device context after use.
			NativeMethods.ReleaseDC(workerW, dc);
		}*/

/*        // Add a randomly moving button to the form
		Button button = new Button() { Text = "Catch Me" };
		this.Controls.Add(button);
		Random rnd = new Random();
		System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
		timer.Interval = 100;
		timer.Tick += new EventHandler((sender, eventArgs) =>
		{
			button.Left = rnd.Next(0, this.Width - button.Width);
			button.Top = rnd.Next(0, this.Height - button.Height);
		});
		timer.Start();*/

		// This line makes the form a child of the WorkerW window, 
		// thus putting it behind the desktop icons and out of reach 
		// for any user input. The form will just be rendered, no 
		// keyboard or mouse input will reach it. You would have to use 
		// WH_KEYBOARD_LL and WH_MOUSE_LL hooks to capture mouse and 
		// keyboard input and redirect it to the windows form manually, 
		// but that's another story, to be told at a later time.
		//NativeMethods.SetParent(this.Handle, workerW);

		NativeMethods.SetParent(hWnd, workerW);

		//clearDesktop();
		//NativeMethods.SendMessage(workerW, NativeMethods.WM_CLOSE, 0, 0); //should close the workerW window we created.
	}



	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
/*		IntPtr hWnd = NativeMethods.GetActiveWindow();

		if (hWnd != IntPtr.Zero)
		{
			SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
		}*/
	}

	public override void _Notification(int what)
	{

		if (what == NotificationWMCloseRequest)
		{
			clearDesktop();
			GetTree().Quit(); // default behavior
		}
	}

	#region Clear Desktop Code

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
