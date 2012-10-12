using System;

namespace PlayWithYourPeas.Engine.Services.Storage
{
	/// <summary>
	/// Event arguments for the SaveDevice after a MessageBox prompt
	/// has been closed.
	/// </summary>
	public sealed class StorageDevicePromptEventArgs : EventArgs
	{
		/// <summary>
		/// Gets whether or not the user has chosen to select a new
		/// StorageDevice.
		/// </summary>
		public bool ShowDeviceSelector { get; set; }
	}
}